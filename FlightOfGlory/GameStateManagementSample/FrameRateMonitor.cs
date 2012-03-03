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


namespace GameStateManagement
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class FrameRateMonitor : Microsoft.Xna.Framework.DrawableGameComponent
    {
        // Add to the declaration of the class
        private float fps;
        private float updateInterval = 1.0f;
        private float timeSinceLastUpdate = 0.0f;
        private float frameCounter = 0;
        private String details = "";

        public string Details
        {
            get { return details; }
        }
        public float Fps
        {
            get { return fps; }
        }

        SpriteBatch spriteBatch;
        SpriteFont spriteFont;

        public FrameRateMonitor(Game game, bool dontWait)
            : base(game)
        {
            if (dontWait)
            {
                GraphicsDeviceManager graphics = (GraphicsDeviceManager)Game.Services.GetService(typeof(IGraphicsDeviceManager));
                graphics.SynchronizeWithVerticalRetrace = false;
            }
            game.IsFixedTimeStep = true;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteFont = Game.Content.Load<SpriteFont>("Fonts/gamefont");
            spriteBatch = new SpriteBatch(Game.GraphicsDevice);
            base.LoadContent();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            frameCounter++;
            timeSinceLastUpdate += elapsed;
            if (timeSinceLastUpdate > updateInterval)
            {
                fps = frameCounter / timeSinceLastUpdate;
                details = "FPS: " + fps.ToString() + " - GameTime: " + gameTime.TotalGameTime.TotalSeconds.ToString();
                frameCounter = 0;
                timeSinceLastUpdate -= updateInterval;
            }
#if XBOX360
            // System.Diagnostics.Debug.WriteLine(details);
#else
            Game.Window.Title = details;
#endif
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            spriteBatch.DrawString(spriteFont, details, new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y), Color.White);
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
