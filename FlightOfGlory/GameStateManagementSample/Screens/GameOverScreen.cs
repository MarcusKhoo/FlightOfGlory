#region File Description
//-----------------------------------------------------------------------------
// PauseMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace GameStateManagementSample
{
    /// <summary>
    /// The pause menu comes up over the top of the game,
    /// giving the player options to resume or quit.
    /// </summary>
    class GameOverScreen : MenuScreen
    {
        #region Fields


        SpriteBatch spriteBatch;
        SpriteFont gameFont;
        MenuEntry tryAgainMenuEntry;
        MenuEntry quitGameMenuEntry;


        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public GameOverScreen() : base("Game Over")
        {
            // Create our menu entries.
            tryAgainMenuEntry = new MenuEntry("Retry");
            quitGameMenuEntry = new MenuEntry("Quit Game");

            // Hook up menu event handlers.
            tryAgainMenuEntry.Selected += tryAgainMenuEntrySelected;
            quitGameMenuEntry.Selected += QuitGameMenuEntrySelected;

            // Add entries to the menu.
            MenuEntries.Add(tryAgainMenuEntry);
            MenuEntries.Add(quitGameMenuEntry);
        }


        public override void Activate(bool instancePreserved)
        {
            ContentManager content = ScreenManager.Game.Content;
            spriteBatch = ScreenManager.SpriteBatch;
            gameFont = ScreenManager.Font;

            ScreenManager.Sound.PlayBG_02();

            base.Activate(instancePreserved);
        }


        public override void Unload()
        {
            ScreenManager.Sound.StopBG_02();
            base.Unload();
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Event handler for when the Quit Game menu entry is selected.
        /// </summary>
        void QuitGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            const string message = "Are you sure you want to quit this game?";

            MessageBoxScreen confirmQuitMessageBox = new MessageBoxScreen(message);

            confirmQuitMessageBox.Accepted += ConfirmQuitMessageBoxAccepted;

            ScreenManager.AddScreen(confirmQuitMessageBox, null);
        }


        void tryAgainMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            LoadingScreen.Load(ScreenManager, true, ControllingPlayer, new GameplayScreen());
        }


        /// <summary>
        /// Event handler for when the user selects ok on the "are you sure
        /// you want to quit" message box. This uses the loading screen to
        /// transition from the game back to the main menu screen.
        /// </summary>
        void ConfirmQuitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
        {
            LoadingScreen.Load(ScreenManager, false, null, new BackgroundScreen(),
                                                           new MainMenuScreen());
        }


        #endregion
    }
}
