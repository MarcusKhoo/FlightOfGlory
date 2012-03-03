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
    public class TextFader : Microsoft.Xna.Framework.DrawableGameComponent
    {
        SpriteBatch spriteBatch;
        SpriteFont font;

        enum State { Visible, Inactive, FadeIn, FadeOut };
        State state = State.Inactive; // the state of the text
        String text = ""; // what the text it
        float timeVisible; // how long it should be visible
        float currentTime; // current timer
        float scale = 1.0f;
        float timeFadeIn = 0.5f, timeFadeOut = 1.0f;
        Color fgColor = Color.Yellow;
        Color shadowCol = Color.DarkOrange;
        Vector2 shadowOffset = new Vector2(2, 2);

        public Color ShadowCol
        {
            get { return shadowCol; }
            set { shadowCol = value; }
        }

        public float Scale
        {
            get { return scale; }
            set { scale = value; }
        }


        public SpriteFont Font
        {
            get { return font; }
            set { font = value; }
        }

        public Color FgColor
        {
            get { return fgColor; }
            set { fgColor = value; }
        }

        public TextFader(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
            game.Services.AddService(typeof(TextFader), this);
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(Game.GraphicsDevice);
            font = Game.Content.Load<SpriteFont>("Fonts/faderFont");
            base.LoadContent();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here
            if (state == State.Inactive) return;
            currentTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (state == State.FadeIn && currentTime >= timeFadeIn)
            {
                state = State.Visible;
                currentTime = 0;
            }
            else if (state == State.Visible && currentTime >= timeVisible)
            {
                state = State.FadeOut;
                currentTime = 0;
            }
            else if (state == State.FadeOut && currentTime >= timeFadeOut)
            {
                state = State.Inactive;
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (state == State.Inactive) return;
            //compute center of screen & text
            Vector2 center = new Vector2(Game.GraphicsDevice.Viewport.Width / 2, Game.GraphicsDevice.Viewport.Height / 2);
            Vector2 origin = font.MeasureString(text) / 2;
            //compute fade colour
            float alpha = 1.0f;
            if (state == State.FadeIn)
            {
                alpha = MathHelper.Clamp(currentTime / timeFadeIn, 0, 1);
            }
            else if (state == State.FadeOut)
            {
                alpha = MathHelper.Clamp(1-(currentTime / timeFadeOut), 0, 1);
            }
            spriteBatch.Begin();

            spriteBatch.DrawString(font, text, center + shadowOffset * scale, shadowCol * alpha, 0.0f, origin, scale, SpriteEffects.None, 0);
            spriteBatch.DrawString(font, text, center, fgColor * alpha, 0.0f, origin, scale, SpriteEffects.None, 0);
            
            spriteBatch.End();

            base.Draw(gameTime);
        }

        public void Show(String txt, float duration)
        {
            text = txt;
            timeVisible = duration;
            state = State.FadeIn; // set to visible
            currentTime = 0;
        }

        public void Show(String txt, float duration, float fadeIn, float fadeOut)
        {
            timeFadeIn = fadeIn; // set fade in
            timeFadeOut = fadeOut; // set fade out
            Show(txt, duration);
        }
    }
}
