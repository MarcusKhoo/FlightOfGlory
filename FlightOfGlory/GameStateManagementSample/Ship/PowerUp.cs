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
    public class PowerUp
    {
        #region Fields

        ContentManager content;

        public Vector3 position;
        public Vector3 up;
        public Vector3 right;
        public Vector3 direction;
        public BoundingSphere boundingSphere;

        private Matrix world;
        private Matrix rot;
        private float rotAmount;
        private int rotDir;

        public Model model;

        static private Random randomiser = new Random();

        private Game Game;

        #endregion
        public PowerUp(Game game)
        {
            this.Game = game;

            Initialize();
        }

        public Matrix World
        {
            get { return world; }
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public void Initialize()
        {
            // TODO: Add your initialization code here
            content = Game.Content;

            position = Vector3.Zero;
            direction = Vector3.Zero;
            right = Vector3.Right;
            up = Vector3.Up;

            int max = 500000;
            int min = -460000;

            position = new Vector3(randomiser.Next(min, max),
                randomiser.Next(5000,450000),
                randomiser.Next(min, max));

            int random = randomiser.Next(10);

            model = content.Load<Model>("Models/PowerUp/powerup");
            boundingSphere = new BoundingSphere(position, 5000.0f);

            rotDir = randomiser.Next(0,1);

            world = Matrix.CreateTranslation(position);

            Effect cartoonEffect = content.Load<Effect>("CartoonShader/CartoonEffect");
            ChangeEffectUsedByModel(model, cartoonEffect);
        }

        /// <summary>
        /// Alters a model so it will draw using a custom effect, while preserving
        /// whatever textures were set on it as part of the original effects.
        /// </summary>
        static void ChangeEffectUsedByModel(Model model, Effect replacementEffect)
        {
            // Table mapping the original effects to our replacement versions.
            Dictionary<Effect, Effect> effectMapping = new Dictionary<Effect, Effect>();

            foreach (ModelMesh mesh in model.Meshes)
            {
                if (mesh.Effects[0].GetType() == typeof(BasicEffect))
                {
                    // Scan over all the effects currently on the mesh.
                    foreach (BasicEffect oldEffect in mesh.Effects)
                    {
                        // If we haven't already seen this effect...
                        if (!effectMapping.ContainsKey(oldEffect))
                        {
                            // Make a clone of our replacement effect. We can't just use
                            // it directly, because the same effect might need to be
                            // applied several times to different parts of the model using
                            // a different texture each time, so we need a fresh copy each
                            // time we want to set a different texture into it.
                            Effect newEffect = replacementEffect.Clone();

                            // Copy across the texture from the original effect.
                            newEffect.Parameters["Texture"].SetValue(oldEffect.Texture);
                            newEffect.Parameters["TextureEnabled"].SetValue(oldEffect.TextureEnabled);

                            effectMapping.Add(oldEffect, newEffect);
                        }
                    }

                    // Now that we've found all the effects in use on this mesh,
                    // update it to use our new replacement versions.
                    foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    {
                        meshPart.Effect = effectMapping[meshPart.Effect];
                    }
                }
            }
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
            // TODO: Add your update code here
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            rotAmount += 0.1f;

            if (rotDir == 0)
                rot = Matrix.CreateFromYawPitchRoll(rotAmount, 0, 0);
            else
                rot = Matrix.CreateFromYawPitchRoll(-rotAmount, 0, 0);

            world = rot * Matrix.CreateTranslation(position);

        }

    }
}
