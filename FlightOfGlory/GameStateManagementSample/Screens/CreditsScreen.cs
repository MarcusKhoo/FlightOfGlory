#region File Description
//-----------------------------------------------------------------------------
// OptionsMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace GameStateManagementSample
{
    /// <summary>
    /// The options screen is brought up over the top of the main menu
    /// screen, and gives the user a chance to configure the game
    /// in various hopefully useful ways.
    /// </summary>
    class CreditsScreen : MenuScreen
    {
        #region Fields

        ContentManager content;
        SpriteBatch spriteBatch;

        SpriteFont font;
        Vector2 origin;

        Vector2 position;
        string text;
        Texture2D background;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public CreditsScreen()
            : base("Credits")
        {
            // Create our menu entries.
            MenuEntry back = new MenuEntry("Back");

            // Hook up menu event handlers.
            back.Selected += OnCancel;

            // Add entries to the menu.
            MenuEntries.Add(back);
        }


        ///<summary>
        ///Load Content
        ///</summary>
        public override void Activate(bool instancePreserved)
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            spriteBatch = ScreenManager.SpriteBatch;

            font = content.Load<SpriteFont>("Fonts/creditsfont");


            origin = new Vector2(0, font.LineSpacing / 2);
            position = new Vector2(130, ScreenManager.GraphicsDevice.Viewport.Height / 2 - 160.0f);
            text =
                "Done by\n" +
                "--------\n" +
                "Marcus Khoo Lian Kai\n" +
                "Ong Jin Wen\n" +
                "Aliena Li Jia Yan\n" +
                "Clarence Lam Ching Kai\n\n" +
                "Music taken from:\n" +
                "soundlabs\n" +
                "sounddogs.com\n" +
                "Plane model taken from:\n" +
                "http://3dlenta.com/en/aircraft.html?page=shop.product_details&" + "\n" +
                "         flypage=flypage.tpl&category_id=16&product_id=318" + "\n" +
                "Border picture taken from:\n" +
                "http://www.flickr.com/photos/fabulousminge/4247563183/";

            background = content.Load<Texture2D>("Textures/border");

            base.Activate(instancePreserved);
        }


        #endregion

        #region Draw


        public override void Draw(GameTime gameTime)
        {
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            spriteBatch.Begin();
            
            
            spriteBatch.Draw(background, new Rectangle(0, 0, ScreenManager.GraphicsDevice.Viewport.Width, ScreenManager.GraphicsDevice.Viewport.Height),Color.White);

            spriteBatch.DrawString(font, text, position, Color.White, 0,
                                   origin, 1, SpriteEffects.None, 0);
            spriteBatch.End();
            base.Draw(gameTime);
        }


        #endregion
    }
}
