using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace GameStateManagementSample
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Missile : Microsoft.Xna.Framework.GameComponent
    {
        #region Fields

        public Vector3 velocity = Vector3.Zero;
        public Vector3 direction;
        public Vector3 position;
        public Vector3 up;
        public Vector3 right;
        public BoundingSphere missileSphere;

        private const float mass = 0.5f;
        private const float ThrustForce = 18000.0f;
        private const float thrustAmount = 1.0f;
        public TimeSpan life = TimeSpan.FromSeconds(10);

        private Matrix world;
        private Ship target;

        #endregion
        public Missile(Game game, Ship ship) : base(game)
        {
            target = ship;
            // TODO: Construct any child components here
        }

        public Matrix World
        {
            get { return world; }
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            direction = Vector3.Zero;
            position = Vector3.Zero;
            up = Vector3.Up;
            right = Vector3.Right;

        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if ((position - target.Position).Length() <= 80000 &&
                (position.Y - target.Position.Y <= 80000 || target.Position.Y - position.Y <= 80000) &&
                (position.X - target.Position.X <= 80000 || target.Position.X - position.X <= 80000) &&
                (position.Z - target.Position.Z <= 80000 || target.Position.Z - position.Z <= 80000)
                )
            {
                direction = Vector3.Normalize(target.Position - position);
            }

            Vector3 force = direction * thrustAmount * ThrustForce;

            //apply acceleration
            Vector3 acceleration = force / mass;
            velocity += acceleration * elapsed;

            position += velocity * elapsed;

            life -= gameTime.ElapsedGameTime;

            missileSphere = new BoundingSphere(position, 1000.0f);

            world = Matrix.Identity;
            world.Forward = direction;
            world.Translation = position;
            world.Up = up;
            world.Right = right;
        }

    }
}
