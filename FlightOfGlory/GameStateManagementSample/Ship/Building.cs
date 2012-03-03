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
    public class Building
    {
        #region Fields

        ContentManager content;

        public Vector3 position;
        public Vector3 up;
        public Vector3 right;
        public Vector3 direction;
        public BoundingBox boundingBox;

        private Matrix world;

        public Model model;

        static private Random randomiser = new Random();

        private Game Game;

        #endregion
        public Building(Game game)
        {
            this.Game = game;
            // TODO: Construct any child components here

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
                0.0f,
                randomiser.Next(min, max));

            int modelRandom = randomiser.Next(10);
            int random = randomiser.Next(10);

            switch (modelRandom)
            {
                case 1:
                case 2:
                    {
                        model = content.Load<Model>("Models/Buildings/building03");
                        boundingBox = new BoundingBox(new Vector3(position.X - 23000, position.Y, position.Z - 6000),
        new Vector3(position.X + 11000, position.Y + 10000, position.Z + 6000));
                        break;
                    }
                case 3:
                case 4:
                case 5:
                    {
                        if (random <= 5)
                        {
                            model = content.Load<Model>("Models/Buildings/Building02");
                            boundingBox = new BoundingBox(new Vector3(position.X - 10000, position.Y, position.Z - 10000),
                                new Vector3(position.X + 10000, position.Y + 34500, position.Z + 10000));

                        }
                        else
                        {
                            model = content.Load<Model>("Models/Buildings/building01");

                            boundingBox = new BoundingBox(new Vector3(position.X - 10000, position.Y, position.Z - 10000),
        new Vector3(position.X + 10000, position.Y + 65000, position.Z + 10000));
                        }
                        break;
                    }
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                    {
                        if (random <= 2)
                        {
                            model = content.Load<Model>("Models/Buildings/building04");
                            boundingBox = new BoundingBox(new Vector3(position.X - 10000, position.Y, position.Z - 10000),
                                new Vector3(position.X + 10000, position.Y + 50000, position.Z + 10000));
                        }
                        else
                        {
                            model = content.Load<Model>("Models/Buildings/building05");
                            boundingBox = new BoundingBox(new Vector3(position.X - 16000, position.Y, position.Z - 10000),
                                new Vector3(position.X + 16000, position.Y + 35000, position.Z + 10000));
                        }
                        break;
                    }
                default:
                    {
                        model = content.Load<Model>("Models/Buildings/Building02");
                        boundingBox = new BoundingBox(new Vector3(position.X - 10000, position.Y, position.Z - 10000),
         new Vector3(position.X + 10000, position.Y + 34500, position.Z + 10000));
                        break;
                    }
            }

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


        }

    }
}
