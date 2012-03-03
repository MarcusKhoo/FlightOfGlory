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
    class InstructionsMenuScreen : MenuScreen
    {
        #region Fields

        ContentManager content;
        SpriteBatch spriteBatch;
        Texture2D instrucTex;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public InstructionsMenuScreen() : base("Instructions")
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
            if (!instancePreserved)
            {
                if (content == null)
                    content = new ContentManager(ScreenManager.Game.Services, "Content");
            }

            spriteBatch = ScreenManager.SpriteBatch;

            instrucTex = content.Load<Texture2D>("Textures/instructions");

            base.Activate(instancePreserved);
        }

        #endregion

        #region Draw

        public override void Draw(GameTime gameTime)
        {
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            spriteBatch.Begin();

            spriteBatch.Draw(instrucTex, new Vector2(viewport.Width / 2 - instrucTex.Width / 2 + 20.0f, viewport.Height / 2 - 130.0f), null, new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha, TransitionAlpha));

            spriteBatch.End();
            base.Draw(gameTime);
        }
        #endregion

    }
}
