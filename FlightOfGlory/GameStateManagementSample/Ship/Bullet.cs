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
    public class Bullet : Microsoft.Xna.Framework.GameComponent
    {
        #region Fields

        public Vector3 velocity = Vector3.Zero;
        public Vector3 direction;
        public Vector3 position;
        public Vector3 up;
        public Vector3 right;
        public BoundingSphere bulletSphere;

        private const float mass = 0.1f;
        private const float ThrustForce = 16000.0f;
        private const float thrustAmount = 1.0f;
        public TimeSpan life = TimeSpan.FromSeconds(2);
        public float damage = 50.0f;

        private Matrix world;

        #endregion
        public Bullet(Game game) : base(game)
        {
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

            Vector3 force = direction * thrustAmount * ThrustForce;

            //apply acceleration
            Vector3 acceleration = force / mass;
            velocity += acceleration * elapsed;

            position += velocity * elapsed;

            bulletSphere = new BoundingSphere(position, 600.0f);
            
            life -= gameTime.ElapsedGameTime;

            world = Matrix.Identity;
            world.Forward = direction;
            world.Translation = position;
            world.Up = up;
            world.Right = right;
        }

    }
}
