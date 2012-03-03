#region File Description
//-----------------------------------------------------------------------------
// Ship.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System;
using GameStateManagement;
#endregion

namespace GameStateManagementSample
{
    public class Ship
    {
        #region Fields

        private PlayerIndex player;
        
        private const float MinimumAltitude = 0;

        /// <summary>
        /// A reference to the graphics device used to access the viewport for touch input.
        /// </summary>
        private GraphicsDevice graphicsDevice;

        /// <summary>
        /// Location of ship in world space.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Direction ship is facing.
        /// </summary>
        public Vector3 Direction;

        /// <summary>
        /// Ship's up vector.
        /// </summary>
        public Vector3 Up;

        public bool onAir;

        private Vector3 right;
        /// <summary>
        /// Ship's right vector.
        /// </summary>
        public Vector3 Right
        {
            get { return right; }
        }

        /// <summary>
        /// Full speed at which ship can rotate; measured in radians per second.
        /// </summary>
        private const float RotationRate = 1.5f;

        /// <summary>
        /// Mass of ship.
        /// </summary>
        private const float Mass = 1.0f;

        /// <summary>
        /// Maximum force that can be applied along the ship's direction.
        /// </summary>
        private const float ThrustForce = 24000.0f;

        /// <summary>
        /// Velocity scalar to approximate drag.
        /// </summary>
        private const float DragFactor = 0.97f;

        /// <summary>
        /// Current ship velocity.
        /// </summary>
        public Vector3 Velocity;

        /// <summary>
        /// Ship world transform matrix.
        /// </summary>
        public Matrix World
        {
            get { return world; }
        }
        private Matrix world;

        public BoundingSphere boundingSphere;

        public float mach = 0;

        public float thrustAmount;

        public float life;

        public List<Bullet> bullets;
        public List<Missile> missiles;

        public int leftWeapon, rightWeapon;

        /// <summary>
        /// Record the time for last weapon fired
        /// </summary>
        public float lastLeftBullet, lastRightBullet;
        public float lastLeftMissile, lastRightMissile;

        /// <summary>
        /// Amount of ammunition
        /// </summary>
        public int leftBulletAmt, rightBulletAmt;
        public int leftMissileAmt,  rightMissileAmt;

        // Maximum number of ammunition
        public int maxBullets = 100;
        private const int maxMissiles = 10;

        float currTime;

        GamePadState currGamePadState;
        GamePadState lastGamePadState;

        float rowAmount;

        float fallRate;

        public float vibrationLeft, vibrationRight;

        #endregion

        #region Initialization

        public Ship(GraphicsDevice device, PlayerIndex player, Vector3 position, Vector3 direction)
        {
            graphicsDevice = device;
            Reset(position, direction);

            this.player = player;

            bullets = new List<Bullet>();
            missiles = new List<Missile>();
        }

        /// <summary>
        /// Restore the ship to its original starting state
        /// </summary>
        public void Reset(Vector3 position, Vector3 direction)
        {
            Position = new Vector3(position.X, position.Y, position.Z);
            Direction = direction;
            Up = Vector3.Up;
            right = Vector3.Right;
            Velocity = Vector3.Zero;
            life = 100;
            leftWeapon = 0;
            rightWeapon = 0;
            lastLeftMissile = 0;
            lastRightMissile = 0;
            leftBulletAmt = maxBullets;
            leftMissileAmt = maxMissiles;
            rightBulletAmt = maxBullets;
            rightMissileAmt = maxMissiles;
        }

        #endregion

        /// <summary>
        /// Applies a simple rotation to the ship and animates position based on simple linear motion physics.
        /// </summary>
        public Vector3 RotateVector(Vector3 vec)
        {
            Matrix rot;
            rot = Matrix.CreateFromAxisAngle(Right, Direction.Y) *
                Matrix.CreateRotationY(Direction.X);//Matrix.CreateFromYawPitchRoll(Direction.X, Direction.Y, Direction.Z);

            Vector3 rvec;
            rvec = Vector3.TransformNormal(vec, rot);
            return rvec;
        }

        public void Update(GameTime gameTime)
        {
#if XBOX360
            currGamePadState = GamePad.GetState(player);
#else
            
#endif

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            currTime = (float)gameTime.TotalGameTime.TotalMilliseconds;

            //update the position of the bounding sphere
            boundingSphere = new BoundingSphere(new Vector3(Position.X, Position.Y + 200.0f, Position.Z), 1200.0f);

            // Determine rotation amount from input
            Vector2 rotationAmount = Vector2.Zero;

            if (onAir)
            {
                rotationAmount = -currGamePadState.ThumbSticks.Left;
            }
            else
            {
                if (thrustAmount >= 1.0f)
                {
                    rotationAmount.Y = -currGamePadState.ThumbSticks.Left.Y;
                    if (rotationAmount.Y < 0)
                        rotationAmount.Y = 0;
                }
                rowAmount = 0;
            }

            // Scale rotation amount to radians per second
            rotationAmount = rotationAmount * RotationRate * elapsed;

            // Correct the X axis steering when the ship is upside down
            if (Up.Y < 0)
                rotationAmount.X = -rotationAmount.X;

            // Create rotation matrix from rotation amount
            Matrix rotationMatrix =
                Matrix.CreateFromAxisAngle(Right, rotationAmount.Y) *
                Matrix.CreateRotationY(rotationAmount.X);

            // Rotate orientation vectors
            Direction = Vector3.TransformNormal(Direction, rotationMatrix);
            Up = Vector3.TransformNormal(Up, rotationMatrix);

            // Re-normalize orientation vectors
            // Without this, the matrix transformations may introduce small rounding
            // errors which add up over time and could destabilize the ship.
            Direction.Normalize();
            Up.Normalize();

            // Re-calculate Right
            right = Vector3.Cross(Direction, Up);

            // The same instability may cause the 3 orientation vectors may
            // also diverge. Either the Up or Direction vector needs to be
            // re-computed with a cross product to ensure orthagonality
            Up = Vector3.Cross(Right, Direction);

            mach += currGamePadState.Triggers.Right * elapsed;
            mach -= currGamePadState.Triggers.Left * elapsed;
            mach = MathHelper.Clamp(mach, 0, 10);

            // Determine thrust amount from input
            if (thrustAmount < mach)
                thrustAmount += 0.01f;
            else if (thrustAmount > 0)
                thrustAmount -= 0.01f;

            // Calculate force from thrust amount
            Vector3 force = Direction * thrustAmount * ThrustForce;

            // Apply acceleration
            Vector3 acceleration = force / Mass;
            Velocity += acceleration * elapsed;

            // Apply psuedo drag
            Velocity *= DragFactor;

            // Apply velocity
            Position += Velocity * elapsed;

            // Simulate gravity
            if (onAir && mach == 0)
                fallRate += 16.35f;
            else if (onAir && Velocity.Y < 0)
                fallRate += 16.35f * 2;
            else
                fallRate -= 16.35f / 2;

            if (fallRate < 0)
                fallRate = 0;

            Position.Y -= fallRate * elapsed;

            // Prevent ship from flying under the ground
            Position.Y = Math.Max(Position.Y, MinimumAltitude);

            rowAmount += currGamePadState.ThumbSticks.Left.X / 25;
            rowAmount = MathHelper.Clamp(rowAmount, -0.75f, 0.75f);
            Matrix rowMatrix = Matrix.CreateFromAxisAngle(Direction, rowAmount);

            if (rowAmount > 0)
                rowAmount -= 0.01f;
            else if (rowAmount < 0)
                rowAmount += 0.01f;

            // Reconstruct the ship's world matrix
            world = Matrix.Identity;
            world.Up = Up;
            world.Right = right;
            world *= rowMatrix;
            world.Forward = Direction;
            world.Translation = Position;

            if (Position.Y > 1000 && thrustAmount > 0 && !onAir)
                onAir = true;

            lastGamePadState = currGamePadState;
        }

        public void UpdateAmmo(GameTime gameTime, ScreenManager ScreenManager, ParticleSystem explosionParticles, Ship enemy, List<Building> buildings)
        {
            // Update bullets
            for (int i = bullets.Count - 1; i >= 0; i--)
            {
                bullets[i].Update(gameTime);

                if (bullets[i].position.Y < 0)
                {
                    bullets.RemoveAt(i);
                    break;
                }

                // Collision with enemy
                if (bullets[i].bulletSphere.Intersects(enemy.boundingSphere))
                {
                    bullets.RemoveAt(i);
                    enemy.life -= 5.0f;
                    enemy.vibrationLeft = enemy.vibrationRight = 0.6f;
                    break;
                }
            }

            // Update missiles
            for (int i = missiles.Count - 1; i >= 0; i--)
            {
                // Update missile
                missiles[i].Update(gameTime);

                // Hit ground
                if (missiles[i].position.Y < 0)
                {
                    ScreenManager.Sound.PlayMissileExplosion(missiles[i].position, Position);
                    ScreenManager.Sound.PlayMissileExplosion(missiles[i].position, enemy.Position);
                    explosionParticles.AddParticle(missiles[i].position, new Vector3(0, 0, 0));
                    missiles.RemoveAt(i);
                    break;
                }

                // Hit enemy
                if (missiles[i].missileSphere.Intersects(enemy.boundingSphere))
                {
                    ScreenManager.Sound.PlayMissileExplosion(missiles[i].position, Position);
                    ScreenManager.Sound.PlayMissileExplosion(missiles[i].position, enemy.Position);
                    explosionParticles.AddParticle(missiles[i].position, new Vector3(0, 0, 0));
                    enemy.life -= 30.0f;
                    enemy.vibrationLeft = enemy.vibrationRight = 1.0f;
                    missiles.RemoveAt(i);
                    break;
                }

                for (int j = buildings.Count - 1; j >= 0; j--)
                {
                    if (missiles[i].missileSphere.Intersects(buildings[j].boundingBox))
                    {
                        ScreenManager.Sound.PlayMissileExplosion(missiles[i].position, Position);
                        ScreenManager.Sound.PlayMissileExplosion(missiles[i].position, enemy.Position);
                        explosionParticles.AddParticle(missiles[i].position, new Vector3(0, 0, 0));
                        missiles.RemoveAt(i);
                        break;
                    }
                }
            }

            DeleteDeadBullet();
            DeleteDeadMissile();
        }

        public void UpdateVibration(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (vibrationLeft > 0)
                vibrationLeft -= elapsed;
            else
                vibrationLeft = 0;

            if (vibrationRight > 0)
                vibrationRight -= elapsed;
            else
                vibrationRight = 0;
#if XBOX360
            GamePad.SetVibration(player, vibrationLeft, vibrationRight);
#else

#endif
        }

        public void FireBullet(ScreenManager ScreenManager, Ship enemy)
        {
            leftWeapon = rightWeapon = 0;
            FireLeftWeapon(ScreenManager.Game, enemy);
            FireRightWeapon(ScreenManager.Game, enemy);
            if(leftBulletAmt > 0 && rightBulletAmt >0)
                ScreenManager.Sound.PlayBullet(Position, enemy.Position);
        }

        public void FireMissile(ScreenManager ScreenManager, Ship enemy)
        {
            leftWeapon = rightWeapon = 1;
            FireLeftWeapon(ScreenManager.Game, enemy);
            FireRightWeapon(ScreenManager.Game, enemy);
            if (leftMissileAmt > 0 && rightMissileAmt > 0)
                ScreenManager.Sound.PlayMissile(Position, enemy.Position);
        }

        private void FireLeftWeapon(Game game, Ship enemy)
        {
            switch (leftWeapon)
            {
                case 0:
                    if (leftBulletAmt > 0 && currTime - lastLeftBullet > 100)
                    {
                        leftBulletAmt -= 1;
                        Bullet bullet = new Bullet(game);
                        bullet.direction = Direction;
                        bullet.up = Up;
                        bullet.right = Right;
                        
                        Vector3 offset = new Vector3(200, 400, 0);
                        offset = Vector3.TransformNormal(offset, World);
                        bullet.position = new Vector3(Position.X - offset.X, Position.Y + offset.Y, Position.Z - offset.Z);

                        lastLeftBullet = currTime;
                        bullets.Add(bullet);
                        vibrationLeft = 0.4f;
                    }
                    break;
                case 1:
                    if (currTime - lastLeftMissile > 5000 && leftMissileAmt > 0)
                    {
                        leftMissileAmt -= 1;
                        Missile missile = new Missile(game, enemy);
                        missile.direction = Direction;
                        missile.up = Up;
                        missile.right = Right;

                        Vector3 offset = new Vector3(900, 400, 0);
                        offset = Vector3.TransformNormal(offset, World);
                        missile.position = new Vector3(Position.X - offset.X, Position.Y + offset.Y, Position.Z - offset.Z);

                        lastLeftMissile = currTime;
                        missiles.Add(missile);
                        vibrationLeft = 0.8f;
                    }
                    break;
            }
        }

        private void FireRightWeapon(Game game, Ship enemy)
        {
            switch (rightWeapon)
            {
                case 0:
                    if (rightBulletAmt > 0 && currTime - lastRightBullet > 100)
                    {
                        rightBulletAmt -= 1;
                        Bullet bullet = new Bullet(game);
                        bullet.direction = Direction;
                        bullet.up = Up;
                        bullet.right = Right;

                        Vector3 offset = new Vector3(200, 400, 0);
                        offset = Vector3.TransformNormal(offset, World);
                        bullet.position = new Vector3(Position.X + offset.X, Position.Y + offset.Y, Position.Z + offset.Z);

                        lastRightBullet = currTime;
                        bullets.Add(bullet);
                        vibrationRight = 0.4f;
                    }
                    break;
                case 1:
                    if (currTime - lastRightMissile > 5000 && rightMissileAmt > 0)
                    {
                        rightMissileAmt -= 1;

                        Missile missile = new Missile(game, enemy);
                        missile.direction = Direction;
                        missile.up = Up;
                        missile.right = Right;

                        Vector3 offset = new Vector3(900, 400, 0);
                        offset = Vector3.TransformNormal(offset, World);
                        missile.position = new Vector3(Position.X + offset.X, Position.Y + offset.Y, Position.Z + offset.Z);

                        lastRightMissile = currTime;
                        missiles.Add(missile);
                        vibrationRight = 0.8f;
                    }
                    break;
            }
        }

        private void DeleteDeadBullet()
        {
            for (int i = bullets.Count - 1; i >= 0; i--)
            {
                if (bullets[i].life <= TimeSpan.Zero)
                {
                    bullets.RemoveAt(i);
                    break;
                }
            }
        }

        private void DeleteDeadMissile()
        {
            for (int i = missiles.Count - 1; i >= 0; i--)
            {
                if (missiles[i].life <= TimeSpan.Zero)
                {
                    missiles.RemoveAt(i);
                    break;
                }
            }
        }
    }
}
