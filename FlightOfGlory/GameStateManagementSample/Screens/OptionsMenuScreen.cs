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
    class OptionsMenuScreen : MenuScreen
    {
        #region Fields

        ContentManager content;
        SpriteBatch spriteBatch;

        Texture2D texture;
        MenuEntry splitScreenMenuEntry;
        MenuEntry bgmMenuEntry;
        MenuEntry sfxMenuEntry;
        MenuEntry vibrationMenuEntry;

        public enum SplitScreen
        {
            Horizontal,
            Vertical,
        }

        public static SplitScreen currentSplitScreen = SplitScreen.Horizontal;

        public static int bgmVolume = 10;

        public static int sfxVolume = 10;


        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public OptionsMenuScreen()
            : base("Options")
        {
            // Create our menu entries.
            //ungulateMenuEntry = new MenuEntry(string.Empty);
            //languageMenuEntry = new MenuEntry(string.Empty);
            //frobnicateMenuEntry = new MenuEntry(string.Empty);
            //elfMenuEntry = new MenuEntry(string.Empty);

            splitScreenMenuEntry = new MenuEntry(string.Empty);
            bgmMenuEntry = new MenuEntry(string.Empty);
            sfxMenuEntry = new MenuEntry(string.Empty);

            SetMenuEntryText();

            MenuEntry back = new MenuEntry("Back");

            // Hook up menu event handlers.
            //ungulateMenuEntry.Selected += UngulateMenuEntrySelected;
            //languageMenuEntry.Selected += LanguageMenuEntrySelected;
            //frobnicateMenuEntry.Selected += FrobnicateMenuEntrySelected;
            //elfMenuEntry.Selected += ElfMenuEntrySelected;
            splitScreenMenuEntry.Selected += SplitScreenMenuEntrySelected;
            bgmMenuEntry.Selected += BgmMenuEntrySelected;
            sfxMenuEntry.Selected += SfxMenuEntrySelected;
            back.Selected += OnCancel;
            
            // Add entries to the menu.
            //MenuEntries.Add(ungulateMenuEntry);
            //MenuEntries.Add(languageMenuEntry);
            //MenuEntries.Add(frobnicateMenuEntry);
            //MenuEntries.Add(elfMenuEntry);
            MenuEntries.Add(splitScreenMenuEntry);
            MenuEntries.Add(bgmMenuEntry);
            MenuEntries.Add(sfxMenuEntry);
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

            texture = content.Load<Texture2D>("Textures/texture");

            base.Activate(instancePreserved);
        }


        /// <summary>
        /// Fills in the latest values for the options screen menu text.
        /// </summary>
        void SetMenuEntryText()
        {
            //ungulateMenuEntry.Text = "Preferred ungulate: " + currentUngulate;
            //languageMenuEntry.Text = "Language: " + languages[currentLanguage];
            //frobnicateMenuEntry.Text = "Frobnicate: " + (frobnicate ? "on" : "off");
            //elfMenuEntry.Text = "elf: " + elf;
            splitScreenMenuEntry.Text = "Split Screen: " + currentSplitScreen;
            bgmMenuEntry.Text = "BGM Volume: " + bgmVolume;
            sfxMenuEntry.Text = "SFX Volume: " + sfxVolume;
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Event handler for when the Ungulate menu entry is selected.
        /// </summary>
        //void UngulateMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        //{
        //    currentUngulate++;

        //    if (currentUngulate > Ungulate.Llama)
        //        currentUngulate = 0;

        //    SetMenuEntryText();
        //}


        /// <summary>
        /// Event handler for when the Language menu entry is selected.
        /// </summary>
        //void LanguageMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        //{
        //    currentLanguage = (currentLanguage + 1) % languages.Length;

        //    SetMenuEntryText();
        //}


        /// <summary>
        /// Event handler for when the Frobnicate menu entry is selected.
        /// </summary>
        //void FrobnicateMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        //{
        //    frobnicate = !frobnicate;

        //    SetMenuEntryText();
        //}


        /// <summary>
        /// Event handler for when the Elf menu entry is selected.
        /// </summary>
        //void ElfMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        //{
        //    elf++;

        //    SetMenuEntryText();
        //}


        void SplitScreenMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            currentSplitScreen++;

            if (currentSplitScreen > SplitScreen.Vertical)
                currentSplitScreen = 0;

            SetMenuEntryText();
        }

        void BgmMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            bgmVolume++;

            if (bgmVolume > 10)
                bgmVolume = 0;

            ScreenManager.Sound.SetCategoryVolume(sfxVolume, bgmVolume);
            SetMenuEntryText();
        }

        void SfxMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            sfxVolume++;

            if (sfxVolume > 10)
                sfxVolume = 0;

                        ScreenManager.Sound.SetCategoryVolume(sfxVolume, bgmVolume);
            SetMenuEntryText();
        }


        #endregion

        #region Draw

        public override void Draw(GameTime gameTime)
        {
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            spriteBatch.Begin();

            spriteBatch.Draw(texture, new Vector2(viewport.Width / 2 - texture.Width / 2 + 20.0f, viewport.Height / 2 - 160.0f), null, new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha, TransitionAlpha));

            spriteBatch.End();
            base.Draw(gameTime);
        }
        #endregion
    }
}
