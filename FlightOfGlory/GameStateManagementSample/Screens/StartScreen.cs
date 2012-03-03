#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using GameStateManagement;
#endregion

namespace GameStateManagementSample
{
    class StartScreen : GameScreen
    {
        Texture2D texture;
        SpriteFont gameFont;
        SpriteBatch spriteBatch;

        public override void Activate(bool instancePreserved)
        {
            spriteBatch = ScreenManager.SpriteBatch;

            gameFont = ScreenManager.Game.Content.Load<SpriteFont>("Fonts/gamefont");
            //texture = ScreenManager.Game.Content.Load<Texture2D>("Textures/...");
            
            base.Activate(instancePreserved);
        }

        public override void HandleInput(GameTime gameTime, InputState input)
        {
            bool pressStart = false;

            PlayerIndex controllingPlayer = PlayerIndex.One;

            for (PlayerIndex index = PlayerIndex.One; index <= PlayerIndex.Four; index++)
            {
                if (GamePad.GetState(index).Buttons.Start == ButtonState.Pressed)
                {
                    controllingPlayer = index;
                    pressStart = true;
                    break;
                }
            }

            if (pressStart)
            {
                LoadingScreen.Load(ScreenManager, false, null, new BackgroundScreen(),
                                                           new MainMenuScreen());
            }
#if XBOX360

#else
            for (PlayerIndex index = PlayerIndex.One; index <= PlayerIndex.Four; index++)
            {
                if (Keyboard.GetState(index).IsKeyDown(Keys.Enter))
                {
                    controllingPlayer = index;
                    pressStart = true;
                    break;
                }
            }

            if (pressStart)
            {
                LoadingScreen.Load(ScreenManager, false, null, new BackgroundScreen(),
                                                           new MainMenuScreen());
            }
#endif

            base.HandleInput(gameTime, input);
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            //spriteBatch.Draw(texture, Vector2.Zero, Color.White);
            spriteBatch.DrawString(gameFont, "Press Start", new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2, ScreenManager.GraphicsDevice.Viewport.Height / 2), Color.White,
                0,new Vector2(gameFont.MeasureString("Press Start").X/2,gameFont.MeasureString("Press Start").Y/2), 2.0f, SpriteEffects.None, 0);

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
