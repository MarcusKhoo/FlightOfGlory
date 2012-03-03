#region Using Statements
using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GameStateManagement;
#endregion

namespace GameStateManagementSample
{
    /// <summary>
    /// This screen implements the actual game logic. It is just a
    /// placeholder to get the idea across: you'll probably want to
    /// put some more interesting gameplay in here!
    /// </summary>
    class SelectionScreen : MenuScreen
    {
        #region Fields

        #region SplitScreen

        // We use SpriteBatch to draw a dividing line between our viewports to make it easier to visualize.
        Texture2D blank;

        // Define the viewports that we wish to render to. We will draw two viewports:
        Viewport playerOneViewport;
        Viewport playerTwoViewport;

        // Each viewport will need a different view and projection matrix in order for them to render the scene from different cameras.
        Matrix playerOneView, playerOneProjection;
        Matrix playerTwoView, playerTwoProjection;

        #endregion

        ContentManager Content;

        Model f16Model;
        Model mig29Model;

        Vector3 playerOnePos;
        Vector3 playerTwoPos;

        public static int playerOneSelect;
        public static int playerTwoSelect;

        float playerOneYaw;
        float playerTwoYaw;

        bool playerOneAccept;
        bool playerTwoAccept;

        SpriteBatch spriteBatch;

        Texture2D titleTexture;
        Texture2D selectionTexture;
        Texture2D pressATexture;
        Texture2D pressStartTexture;
        Texture2D readyTexture;

        Texture2D controllerOne;
        Texture2D controllerTwo;
        Texture2D controllerThree;
        Texture2D controllerFour;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public SelectionScreen()
            : base("")
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }


        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void Activate(bool instancePreserved)
        {
            if (!instancePreserved)
            {
                if (Content == null)
                    Content = new ContentManager(ScreenManager.Game.Services, "Content");

                spriteBatch = ScreenManager.SpriteBatch;

                f16Model = Content.Load<Model>("Models/F-16/F-16");
                mig29Model = Content.Load<Model>("Models/MiG-29/MiG-29");

                titleTexture = Content.Load<Texture2D>("Textures/selection_title");
                selectionTexture = Content.Load<Texture2D>("Textures/selection");
                pressATexture = Content.Load<Texture2D>("Textures/pressA");
                pressStartTexture = Content.Load<Texture2D>("Textures/pressStart");
                readyTexture = Content.Load<Texture2D>("Textures/readyTexture");

                controllerOne = Content.Load<Texture2D>("Textures/Controller/xboxControllerOne");
                controllerTwo = Content.Load<Texture2D>("Textures/Controller/xboxControllerTwo");
                controllerThree = Content.Load<Texture2D>("Textures/Controller/xboxControllerThree");
                controllerFour = Content.Load<Texture2D>("Textures/Controller/xboxControllerFour");

                playerOnePos = new Vector3(5000f, -2000f, 0f);
                playerTwoPos = new Vector3(-5000f, -2000f, 0f);

                playerOneSelect = 1;
                playerTwoSelect = 2;

                playerOneYaw = 0;
                playerTwoYaw = 0;

                playerOneAccept = false;
                playerTwoAccept = false;

                #region SplitScreen

                // Create the texture we'll use to draw our viewport edges.
                blank = new Texture2D(ScreenManager.GraphicsDevice, 1, 1);
                blank.SetData(new[] { Color.White });

                // Create the viewports
                playerOneViewport = new Viewport
                {
                    MinDepth = 0,
                    MaxDepth = 1,
                    X = 0,
                    Y = 0,
                    Width = ScreenManager.GraphicsDevice.Viewport.Width / 2,
                    Height = ScreenManager.GraphicsDevice.Viewport.Height,
                };
                playerTwoViewport = new Viewport
                {
                    MinDepth = 0,
                    MaxDepth = 1,
                    X = ScreenManager.GraphicsDevice.Viewport.Width / 2,
                    Y = 0,
                    Width = ScreenManager.GraphicsDevice.Viewport.Width / 2,
                    Height = ScreenManager.GraphicsDevice.Viewport.Height,
                };

                // Create the view and projection matrix for each of the viewports
                playerOneView = Matrix.CreateLookAt(
                    Vector3.Zero,
                    playerOnePos,
                    Vector3.Up);
                playerOneProjection = Matrix.CreatePerspectiveFieldOfView(
                    MathHelper.PiOver4, playerOneViewport.AspectRatio, 10f, 10000f);

                playerTwoView = Matrix.CreateLookAt(
                    Vector3.Zero,
                    playerTwoPos,
                    Vector3.Up);
                playerTwoProjection = Matrix.CreatePerspectiveFieldOfView(
                    MathHelper.PiOver4, playerTwoViewport.AspectRatio, 10f, 10000f);

                #endregion

                ScreenManager.Sound.PlayBG_02();

                // once the load has finished, we use ResetElapsedTime to tell the game's
                // timing mechanism that we have just finished a very long frame, and that
                // it should not try to catch up.
                ScreenManager.Game.ResetElapsedTime();
            }
        }


        public override void Deactivate()
        {
            base.Deactivate();
        }


        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void Unload()
        {
            ScreenManager.Sound.StopBG_02();
            Content.Unload();
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(GameTime gameTime, InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            // Look up inputs for the active player profile.
            int playerIndex = (int)ControllingPlayer.Value;

            GamePadState playerOneCurrentGamePadState = input.CurrentGamePadStates[playerIndex];
            GamePadState playerOneLastGamePadState = input.LastGamePadStates[playerIndex];

            if (!playerOneAccept)
            {
                if (playerOneCurrentGamePadState.DPad.Left == ButtonState.Released &&
                    playerOneLastGamePadState.DPad.Left == ButtonState.Pressed)
                {
                    playerOneSelect--;
                }
                else if (playerOneCurrentGamePadState.DPad.Right == ButtonState.Released &&
                    playerOneLastGamePadState.DPad.Right == ButtonState.Pressed)
                {
                    playerOneSelect++;
                }

                if (playerOneCurrentGamePadState.Buttons.A == ButtonState.Pressed)
                {
                    playerOneAccept = true;
                }

                SelectLimit(ref playerOneSelect);

                playerOneYaw -= playerOneCurrentGamePadState.ThumbSticks.Left.X / 10;

                if (playerOneCurrentGamePadState.Buttons.B == ButtonState.Released && playerOneLastGamePadState.Buttons.B == ButtonState.Pressed)
                {
                    LoadingScreen.Load(ScreenManager, false, null, new BackgroundScreen(), new MainMenuScreen());
                }
            }
            else
            {
                if (playerOneCurrentGamePadState.Buttons.B == ButtonState.Released && playerOneLastGamePadState.Buttons.B == ButtonState.Pressed)
                {
                    playerOneAccept = false;
                }
            }

            if (ScreenManager.ControllingPlayerTwo == -1)
                for (int i = 0; i < 4; i++)
                {
                    if (i != (int)ControllingPlayer)
                    {
                        if (input.CurrentGamePadStates[i].Buttons.Start == ButtonState.Pressed)
                        {
                            ScreenManager.ControllingPlayerTwo = i;
                        }
                    }
                }
            else
            {
                GamePadState playerTwoCurrentGamePadState = input.CurrentGamePadStates[ScreenManager.ControllingPlayerTwo];
                GamePadState playerTwoLastGamePadState = input.LastGamePadStates[ScreenManager.ControllingPlayerTwo];

                if (!playerTwoAccept)
                {
                    if (playerTwoCurrentGamePadState.DPad.Left == ButtonState.Released &&
                        playerTwoLastGamePadState.DPad.Left == ButtonState.Pressed)
                    {
                        playerTwoSelect--;
                    }
                    else if (playerTwoCurrentGamePadState.DPad.Right == ButtonState.Released &&
                        playerTwoLastGamePadState.DPad.Right == ButtonState.Pressed)
                    {
                        playerTwoSelect++;
                    }

                    SelectLimit(ref playerTwoSelect);

                    playerTwoYaw -= playerTwoCurrentGamePadState.ThumbSticks.Left.X / 10;

                    if (playerTwoCurrentGamePadState.Buttons.A == ButtonState.Pressed)
                    {
                        playerTwoAccept = true;
                    }

                    if (playerTwoCurrentGamePadState.Buttons.B == ButtonState.Released && playerTwoLastGamePadState.Buttons.B == ButtonState.Pressed)
                    {
                        LoadingScreen.Load(ScreenManager, false, null, new BackgroundScreen(), new MainMenuScreen());
                    }
                }

                if (playerTwoCurrentGamePadState.Buttons.B == ButtonState.Released && playerTwoLastGamePadState.Buttons.B == ButtonState.Pressed)
                {
                    playerTwoAccept = false;
                }
            }

            if (playerOneAccept && playerTwoAccept)
            {
                LoadingScreen.Load(ScreenManager, true, ControllingPlayer, new GameplayScreen());

            }
#if XBOX360

#else
            if(Keyboard.GetState().IsKeyDown(Keys.Enter))
                LoadingScreen.Load(ScreenManager, true, ControllingPlayer, new GameplayScreen());
#endif
        }

        private void SelectLimit(ref int select)
        {
            if (select > 2)
            {
                select = 1;
            }
            else if (select < 1)
            {
                select = 2;
            }
        }


        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // This game has a blue background. Why? Because!
            //ScreenManager.GraphicsDevice.Clear(ClearOptions.Target, Color.CornflowerBlue, 0, 0);
            spriteBatch.Begin();
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            Rectangle fullscreen = new Rectangle(0, 0, viewport.Width, viewport.Height);
            spriteBatch.Draw(selectionTexture, fullscreen, new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha, TransitionAlpha));
            spriteBatch.Draw(titleTexture, new Vector2(viewport.Width / 2 - titleTexture.Width / 2, viewport.Height * 0.15f), new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha, TransitionAlpha));
            spriteBatch.End();

            // Now we'll draw the viewport edges on top so we can visualize the viewports more easily.
            //DrawViewportEdges(playerOneViewport);
            //DrawViewportEdges(playerTwoViewport);

            // Draw our scene with all of our viewports and their respective view/projection matrices.
            DrawScene(gameTime, playerOneViewport, playerOneView, playerOneProjection);
            DrawScene(gameTime, playerTwoViewport, playerTwoView, playerTwoProjection);

            DrawOverlay();
        }

        private void DrawOverlay()
        {
            spriteBatch.Begin();

            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;

            Texture2D texture = ChooseTexture((int)ControllingPlayer); float scale = 0.5f;
            Vector2 position = new Vector2(viewport.Width / 4 - texture.Width * scale / 2, viewport.Height * 0.15f);

            spriteBatch.Draw(texture, position, null, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);

            if (playerOneAccept)
                spriteBatch.Draw(readyTexture, new Vector2(viewport.Width * 0.2f, viewport.Height / 2 + 220.0f), Color.White);
            else
                spriteBatch.Draw(pressATexture, new Vector2(viewport.Width * 0.2f, viewport.Height / 2 + 220.0f), Color.White);

            if (ScreenManager.ControllingPlayerTwo == -1)
                spriteBatch.Draw(pressStartTexture, new Vector2(viewport.Width * 0.6f, viewport.Height / 2 - 150.0f), Color.White);
            else
            {
                position = new Vector2(viewport.Width * 3 / 4 - texture.Width * scale / 2, viewport.Height * 0.15f);
                texture = ChooseTexture(ScreenManager.ControllingPlayerTwo);
                spriteBatch.Draw(texture, position, null, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
            }
            if (playerTwoAccept)
                spriteBatch.Draw(readyTexture, new Vector2(viewport.Width * 0.7f, viewport.Height / 2 + 220.0f), Color.White);
            else
                spriteBatch.Draw(pressATexture, new Vector2(viewport.Width * 0.7f, viewport.Height / 2 + 220.0f), Color.White);

            spriteBatch.End();
        }

        private Texture2D ChooseTexture(int index)
        {
            Texture2D texture = controllerOne;
            switch (index)
            {
                case 1:
                    texture = controllerTwo;
                    break;
                case 2:
                    texture = controllerThree;
                    break;
                case 3:
                    texture = controllerFour;
                    break;
            }
            return texture;
        }

        /// <summary>
        /// Simple model drawing method. The interesting part here is that
        /// the view and projection matrices are tak    en from the camera object.
        /// </summary>        
        private void DrawModel(Model model, Matrix world, Matrix view, Matrix projection)
        {
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.World = transforms[mesh.ParentBone.Index] * world;
                    effect.DirectionalLight0.Enabled = true;
                    effect.DirectionalLight0.DiffuseColor = Color.White.ToVector3();
                    effect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(-1, -1.5f, 0));
                    effect.DirectionalLight1.Enabled = true;
                    effect.DirectionalLight1.DiffuseColor = Color.White.ToVector3();
                    effect.DirectionalLight1.Direction = Vector3.Normalize(new Vector3(1, -1.5f, -1));
                    effect.DirectionalLight2.Enabled = true;
                    effect.DirectionalLight2.DiffuseColor = Color.White.ToVector3();
                    effect.DirectionalLight2.Direction = Vector3.Normalize(new Vector3(0, -1.5f, -1));

                    effect.Projection = projection;
                    effect.View = view;
                }
                mesh.Draw();
            }
        }

        private Model SelectModel(int select)
        {
            Model model = null;
            switch (select)
            {
                case 1:
                    model = f16Model;
                    break;
                case 2:
                    model = mig29Model;
                    break;
            }
            return model;
        }

        /// <summary>
        /// DrawScene is our main rendering method. By rendering the entire scene inside of this method,
        /// we enable ourselves to be able to render the scene using any viewport we may want.
        /// </summary>
        private void DrawScene(GameTime gameTime, Viewport viewport, Matrix view, Matrix projection)
        {
            // Set our viewport. We store the old viewport so we can restore it when we're done in case
            // we want to render to the full viewport at some point.
            Viewport oldViewport = ScreenManager.GraphicsDevice.Viewport;
            ScreenManager.GraphicsDevice.Viewport = viewport;
            ScreenManager.GraphicsDevice.BlendState = BlendState.Opaque;
            ScreenManager.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            ScreenManager.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            // Here we'd want to draw our entire scene. For this sample, that's just the tank.
            Matrix playerOneWorld = Matrix.Identity;
            Matrix playerOneRotation = Matrix.CreateFromYawPitchRoll(playerOneYaw, 0, 0);
            playerOneWorld *= playerOneRotation;
            playerOneWorld.Translation = playerOnePos;
            Model playerOneModel = SelectModel(playerOneSelect);
            DrawModel(playerOneModel, playerOneWorld, view, projection);

            Matrix playerTwoWorld = Matrix.Identity;
            Matrix playerTwoRotation = Matrix.CreateFromYawPitchRoll(playerTwoYaw, 0, 0);
            playerTwoWorld *= playerTwoRotation;
            playerTwoWorld.Translation = playerTwoPos;
            Model playerTwoModel = SelectModel(playerTwoSelect);
            DrawModel(playerTwoModel, playerTwoWorld, view, projection);

            // Now that we're done, set our old viewport back on the device
            ScreenManager.GraphicsDevice.Viewport = oldViewport;
        }

        /// <summary>
        /// A helper to draw the edges of a viewport.
        /// </summary>
        private void DrawViewportEdges(Viewport viewport)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            const int edgeWidth = 2;

            // We now compute four rectangles that make up our edges
            Rectangle topEdge = new Rectangle(
                viewport.X - edgeWidth / 2,
                viewport.Y - edgeWidth / 2,
                viewport.Width + edgeWidth,
                edgeWidth);
            Rectangle bottomEdge = new Rectangle(
                viewport.X - edgeWidth / 2,
                viewport.Y + viewport.Height - edgeWidth / 2,
                viewport.Width + edgeWidth,
                edgeWidth);
            Rectangle leftEdge = new Rectangle(
                viewport.X - edgeWidth / 2,
                viewport.Y - edgeWidth / 2,
                edgeWidth,
                viewport.Height + edgeWidth);
            Rectangle rightEdge = new Rectangle(
                viewport.X + viewport.Width - edgeWidth / 2,
                viewport.Y - edgeWidth / 2,
                edgeWidth,
                viewport.Height + edgeWidth);

            // We just use SpriteBatch to draw the four rectangles
            spriteBatch.Begin();
            spriteBatch.Draw(blank, topEdge, Color.Black);
            spriteBatch.Draw(blank, bottomEdge, Color.Black);
            spriteBatch.Draw(blank, leftEdge, Color.Black);
            spriteBatch.Draw(blank, rightEdge, Color.Black);
            spriteBatch.End();
        }


        #endregion
    }
}
