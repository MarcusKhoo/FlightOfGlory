#region File Description
//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using GameStateManagement;
#endregion

namespace GameStateManagementSample
{
    /// <summary>
    /// This screen implements the actual game logic. It is just a
    /// placeholder to get the idea across: you'll probably want to
    /// put some more interesting gameplay in here!
    /// </summary>
    class GameplayScreen : GameScreen
    {
        #region Fields

        ContentManager Content;
        SpriteFont gameFont;

        TextFader textFader;

        Random randomiser = new Random();

        Model bulletModel, missileModel;

        ParticleSystem missileTrailParticles;
        ParticleSystem explosionParticles;
        ParticleSystem explosionSmokeParticles;
        List<Projectile> missileTrail = new List<Projectile>();

        float currTime, lastExplosion;
        //float pOneVibrateLeft, pOneVibrateRight;
        //float pTwoVibrateLeft, pTwoVibrateRight;
        float boundaryX = 460000;
        float boundaryZ = 460000;
        float boundaryY = 460000;

        List<Bullet> bulletOneList;
        List<Bullet> bulletTwoList;
        List<Missile> missileOneList;
        List<Missile> missileTwoList;
        List<PowerUp> powerUpList;
        List<Building> buildingArray;

        bool pOneWarning, pTwoWarning;
        TimeSpan shipOneWarningCountdown = TimeSpan.FromSeconds(11);
        TimeSpan shipTwoWarningCountdown = TimeSpan.FromSeconds(11);
        TimeSpan gameover_timer = TimeSpan.FromSeconds(2);

        #region SplitScreen

        // We use SpriteBatch to draw a dividing line between our viewports to make it
        // easier to visualize.
        Texture2D blank;

        // Define the viewports that we wish to render to. We will draw two viewports:
        // - The top half of the screen
        // - The bottom half of the screen
        Viewport playerOneViewport;
        Viewport playerTwoViewport;

        // Each viewport will need a different view and projection matrix in
        // order for them to render the scene from different cameras.
        Matrix playerOneView, playerOneProjection;
        Matrix playerTwoView, playerTwoProjection;

        #endregion

        #region ChaseCamera

        Ship shipOne;
        Ship shipTwo;
        ChaseCamera cameraOne;
        ChaseCamera cameraTwo;

        Model playerOneModel;
        Model playerTwoModel;
        Model groundModel;
        Model skyModel;

        bool cameraSpringEnabled = true;

        #endregion

        #region HUD
        HUD pOneHUD;
        HUD pTwoHUD;
        #endregion

        #region CartoonShader

        Random random = new Random();

        Effect postprocessEffect;

        // Overlay texture containing the pencil sketch stroke pattern.
        Texture2D sketchTexture;


        // Randomly offsets the sketch pattern to create a hand-drawn animation effect.
        Vector2 sketchJitter;
        TimeSpan timeToNextJitter;

        RenderTarget2D sceneRenderTarget;
        RenderTarget2D normalDepthRenderTarget;

        // Choose what display settings to use.
        NonPhotoRealisticSettings Settings
        {
            get { return NonPhotoRealisticSettings.PresetSettings[settingsIndex]; }
        }

        int settingsIndex = 0;

        #endregion

        BoundingBox runway01_bounding, runway02_bounding;

        float pauseAlpha;

        InputAction pauseAction;

        #endregion
        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public GameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            pauseAction = new InputAction(
                new Buttons[] { Buttons.Start, Buttons.Back },
                new Keys[] { Keys.Escape },
                true);

            bulletOneList = new List<Bullet>();
            bulletTwoList = new List<Bullet>();
            missileOneList = new List<Missile>();
            missileTwoList = new List<Missile>();
            buildingArray = new List<Building>();
            powerUpList = new List<PowerUp>();
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

                // collision debugging
                DebugShapeRenderer.Initialize(ScreenManager.GraphicsDevice);

                #region SplitScreen

                // Create the texture we'll use to draw our viewport edges.
                blank = new Texture2D(ScreenManager.GraphicsDevice, 1, 1);
                blank.SetData(new[] { Color.White });

                if (OptionsMenuScreen.currentSplitScreen == OptionsMenuScreen.SplitScreen.Horizontal)
                {
                    // Create the viewports
                    playerOneViewport = new Viewport
                    {
                        MinDepth = 0,
                        MaxDepth = 1,
                        X = 0,
                        Y = 0,
                        Width = ScreenManager.GraphicsDevice.Viewport.Width,
                        Height = ScreenManager.GraphicsDevice.Viewport.Height / 2,
                    };
                    playerTwoViewport = new Viewport
                    {
                        MinDepth = 0,
                        MaxDepth = 1,
                        X = 0,
                        Y = ScreenManager.GraphicsDevice.Viewport.Height / 2,
                        Width = ScreenManager.GraphicsDevice.Viewport.Width,
                        Height = ScreenManager.GraphicsDevice.Viewport.Height / 2,
                    };
                }
                else
                {
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
                }

                // Create the view and projection matrix for each of the viewports
                playerOneView = Matrix.CreateLookAt(
                    new Vector3(400f, 900f, 200f),
                    new Vector3(-100f, 0f, 0f),
                    Vector3.Up);
                playerOneProjection = Matrix.CreatePerspectiveFieldOfView(
                    MathHelper.PiOver4, playerOneViewport.AspectRatio, 10f, 5000f);

                playerTwoView = Matrix.CreateLookAt(
                    new Vector3(0f, 800f, 800f),
                    Vector3.Zero,
                    Vector3.Up);
                playerTwoProjection = Matrix.CreatePerspectiveFieldOfView(
                    MathHelper.PiOver4, playerTwoViewport.AspectRatio, 10f, 5000f);

                #endregion

                #region ChaseCamera

                gameFont = Content.Load<SpriteFont>("Fonts/gamefont");
                if (SelectionScreen.playerOneSelect == 1)
                    playerOneModel = Content.Load<Model>("Models/F-16/F-16_Game");
                else
                    playerOneModel = Content.Load<Model>("Models/MiG-29/MiG-29_Game");
                if (SelectionScreen.playerTwoSelect == 1)
                    playerTwoModel = Content.Load<Model>("Models/F-16/F-16_Game");
                else
                    playerTwoModel = Content.Load<Model>("Models/MiG-29/MiG-29_Game");
                groundModel = Content.Load<Model>("Models/Terrain/terrain");
                bulletModel = Content.Load<Model>("Models/Bullet/bullet");
                missileModel = Content.Load<Model>("Models/Missile/missile");
                skyModel = Content.Load<Model>("Models/SkyDome/skydome");

                if (SelectionScreen.playerOneSelect == 1)
                    shipOne = new Ship(ScreenManager.GraphicsDevice, (PlayerIndex)ControllingPlayer, new Vector3(-74000, 0, 400000), new Vector3(0, 0, -1));
                else
                    shipOne = new Ship(ScreenManager.GraphicsDevice, (PlayerIndex)ControllingPlayer, new Vector3(-74000, 0, 400000), new Vector3(0, 0, -1));

                if (SelectionScreen.playerTwoSelect == 1)
                    shipTwo = new Ship(ScreenManager.GraphicsDevice, (PlayerIndex)ScreenManager.ControllingPlayerTwo, new Vector3(18000, 0, -400000), new Vector3(0, 0, 1));
                else
                    shipTwo = new Ship(ScreenManager.GraphicsDevice, (PlayerIndex)ScreenManager.ControllingPlayerTwo, new Vector3(18000, 0, -400000), new Vector3(0, 0, 1));

                // Create the chase camera
                cameraOne = new ChaseCamera();
                cameraTwo = new ChaseCamera();

                // Set the camera offsets
                cameraOne.DesiredPositionOffset = new Vector3(0.0f, 2000.0f, 3500.0f);
                cameraOne.LookAtOffset = new Vector3(0.0f, 150.0f, 0.0f);
                cameraTwo.DesiredPositionOffset = new Vector3(0.0f, 2000.0f, 3500.0f);
                cameraTwo.LookAtOffset = new Vector3(0.0f, 150.0f, 0.0f);

                // Set camera perspective
                cameraOne.NearPlaneDistance = 1.0f;
                cameraOne.FarPlaneDistance = 1550000.0f;
                cameraTwo.NearPlaneDistance = 1.0f;
                cameraTwo.FarPlaneDistance = 1550000.0f;

                // Set the camera aspect ratio
                // This must be done after the class to base.Initalize() which will
                // initialize the graphics device.
                cameraOne.AspectRatio = (float)playerOneViewport.Width / playerOneViewport.Height;
                cameraTwo.AspectRatio = (float)playerTwoViewport.Width / playerTwoViewport.Height;

                // Perform an inital reset on the camera so that it starts at the resting
                // position. If we don't do this, the camera will start at the origin and
                // race across the world to get behind the chased object.
                // This is performed here because the aspect ratio is needed by Reset.
                UpdateCameraChaseTarget();
                cameraOne.Reset();
                cameraTwo.Reset();

                #endregion

                #region HUD
                pOneHUD = new HUD(ScreenManager.Game, playerOneViewport, PlayerIndex.One);
                pTwoHUD = new HUD(ScreenManager.Game, playerTwoViewport, PlayerIndex.Two);
                #endregion

                #region Particles

                explosionParticles = new ParticleSystem(ScreenManager.Game, Content, "Particles/ExplosionSettings");
                explosionSmokeParticles = new ParticleSystem(ScreenManager.Game, Content, "Particles/ExplosionSmokeSettings");
                missileTrailParticles = new ParticleSystem(ScreenManager.Game, Content, "Particles/ProjectileTrailSettings");

                explosionSmokeParticles.DrawOrder = 200;
                missileTrailParticles.DrawOrder = 300;
                explosionParticles.DrawOrder = 400;

                explosionParticles.Visible = false;
                explosionSmokeParticles.Visible = false;
                missileTrailParticles.Visible = false;

                ScreenManager.Game.Components.Add(missileTrailParticles);
                ScreenManager.Game.Components.Add(explosionParticles);
                ScreenManager.Game.Components.Add(explosionSmokeParticles);

                #endregion

                #region CartoonShader

                postprocessEffect = Content.Load<Effect>("CartoonShader/PostprocessEffect");
                sketchTexture = Content.Load<Texture2D>("CartoonShader/SketchTexture");
                Effect cartoonEffect = Content.Load<Effect>("CartoonShader/CartoonEffect");

                ChangeEffectUsedByModel(playerOneModel, cartoonEffect);
                ChangeEffectUsedByModel(playerTwoModel, cartoonEffect);

                // Create two custom rendertargets.
                PresentationParameters pp = ScreenManager.GraphicsDevice.PresentationParameters;

                sceneRenderTarget = new RenderTarget2D(ScreenManager.GraphicsDevice,
                                                       pp.BackBufferWidth, pp.BackBufferHeight, false,
                                                       pp.BackBufferFormat, pp.DepthStencilFormat);

                normalDepthRenderTarget = new RenderTarget2D(ScreenManager.GraphicsDevice,
                                                             pp.BackBufferWidth, pp.BackBufferHeight, false,
                                                             pp.BackBufferFormat, pp.DepthStencilFormat);

                #endregion

                // Initalise the motor
                //pOneVibrateRight = 0;
                //pOneVibrateLeft = 0;
                //pTwoVibrateRight = 0;
                //pTwoVibrateLeft = 0;

                // Initalise player's warning when out of battlezone
                pOneWarning = false;
                pTwoWarning = false;

                for (int i = 0; i < 100; i++)
                {
                    Building building = new Building(ScreenManager.Game);
                    buildingArray.Add(building);
                }

                for (int i = 0; i < 20; i++)
                {
                    PowerUp powerup = new PowerUp(ScreenManager.Game);
                    powerUpList.Add(powerup);
                }

                int width_Z = 170000;
                int width_X = 45000;

                runway01_bounding = new BoundingBox(new Vector3(-74000 - width_X, 0, 400000 - width_Z),
                   new Vector3(-74000 + width_X, 10000, 400000 + width_Z));

                runway02_bounding = new BoundingBox(new Vector3(18000 - width_X, 0, -400000 - width_Z),
                    new Vector3(18000 + width_X, 10000, -400000 + width_Z));

                //remove buildings if collide with runway
                for (int i = buildingArray.Count - 1; i >= 0; i--)
                {
                    if (buildingArray[i].boundingBox.Intersects(runway01_bounding) ||
                        buildingArray[i].boundingBox.Intersects(runway02_bounding))
                    {
                        buildingArray.RemoveAt(i);
                    }
                }

                //remove buildings when collide with each other.
                for (int i = buildingArray.Count - 1; i >= 1; i--)
                {
                    if (buildingArray[i].boundingBox.Intersects(buildingArray[i - 1].boundingBox))
                    {
                        buildingArray.RemoveAt(i);
                    }
                }

                //re-position powerups when collide with buildings
                for (int i = powerUpList.Count - 1; i >= 0; i--)
                {
                    for (int k = buildingArray.Count - 1; k >= 0; k--)
                    {
                        while(powerUpList[i].boundingSphere.Intersects(buildingArray[k].boundingBox))
                        {
                            powerUpList[i].position = new Vector3(randomiser.Next(-460000, 500000),
                                                                randomiser.Next(5000, 450000),
                                                                randomiser.Next(-460000, 500000));
                        }
                    }
                }

                //re-position powerups when collide with runways
                for (int i = powerUpList.Count - 1; i >= 0; i--)
                {
                    while (powerUpList[i].boundingSphere.Intersects(runway01_bounding) ||
                        powerUpList[i].boundingSphere.Intersects(runway02_bounding))
                    {
                        powerUpList[i].position = new Vector3(randomiser.Next(-460000, 500000),
                                                            randomiser.Next(5000, 450000),
                                                            randomiser.Next(-460000, 500000));
                    }
                }

                textFader = (TextFader)ScreenManager.Game.Services.GetService(typeof(TextFader));

                // once the load has finished, we use ResetElapsedTime to tell the game's
                // timing mechanism that we have just finished a very long frame, and that
                // it should not try to catch up.
                ScreenManager.Game.ResetElapsedTime();
            }
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
        /// Unload graphics content used by the game.
        /// </summary>
        public override void Unload()
        {
            if (sceneRenderTarget != null)
            {
                sceneRenderTarget.Dispose();
                sceneRenderTarget = null;
            }

            if (normalDepthRenderTarget != null)
            {
                normalDepthRenderTarget.Dispose();
                normalDepthRenderTarget = null;
            }
            Content.Unload();
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);

            ScreenManager.Sound.Update();

            // Gradually fade in or out depending on whether we are covered by the pause screen.
            if (coveredByOtherScreen)
            {
                pauseAlpha = Math.Min(pauseAlpha + 1f / 32, 1);
                ScreenManager.Sound.Pause();
            }
            else
            {
                pauseAlpha = Math.Max(pauseAlpha - 1f / 32, 0);
                ScreenManager.Sound.Resume();
            }

            if (IsActive)
            {
                currTime = (float)gameTime.TotalGameTime.TotalMilliseconds;

                ScreenManager.Sound.PlayEngine(shipOne.Position, shipTwo.Position);
                ScreenManager.Sound.PlayEngine(shipTwo.Position, shipOne.Position);

                if (shipOne.mach > 0)
                    ScreenManager.Sound.PlayAfterburner(shipOne.Position, shipTwo.Position);

                if (shipTwo.mach > 0)
                    ScreenManager.Sound.PlayAfterburner(shipTwo.Position, shipOne.Position);

                // Update the ship
                UpdateShip(gameTime, shipOne);
                UpdateShip(gameTime, shipTwo);

                CheckBoundaries(gameTime);

                // Update the camera to chase the new target
                UpdateCameraChaseTarget();

                // The chase camera's update behavior is the springs, but we can
                // use the Reset method to have a locked, spring-less camera
                cameraOne.Update(gameTime);
                cameraTwo.Update(gameTime);

                // Update bullets and missiles
                //UpdateAmmo(gameTime, shipOne, bulletOneList, missileOneList);
                //UpdateAmmo(gameTime, shipTwo, bulletTwoList, missileTwoList);
                pOneHUD.Update(gameTime, shipOne.Direction, shipOne.life, shipOne.leftBulletAmt, shipOne.leftMissileAmt, shipOne.rightBulletAmt, shipOne.rightMissileAmt, pOneWarning, shipOneWarningCountdown, shipOne.mach);
                pTwoHUD.Update(gameTime, shipTwo.Direction, shipTwo.life, shipTwo.leftBulletAmt, shipTwo.leftMissileAmt, shipTwo.rightBulletAmt, shipTwo.rightMissileAmt, pTwoWarning, shipTwoWarningCountdown, shipTwo.mach);
                UpdateMissileTrail(missileOneList);

                for (int i = 0; i < powerUpList.Count; i++)
                {
                    powerUpList[i].Update(gameTime);
                    if (powerUpList[i].boundingSphere.Intersects(shipOne.boundingSphere))
                    {
                        
                        shipOne.leftBulletAmt = shipOne.maxBullets;
                        shipOne.rightBulletAmt = shipOne.maxBullets;

                        ScreenManager.Sound.PlayWeaponToggle(shipOne.Position, shipTwo.Position);
                        powerUpList.RemoveAt(i);
                        break;
                    }
                    if (powerUpList[i].boundingSphere.Intersects(shipTwo.boundingSphere))
                    {
                        shipTwo.leftBulletAmt = shipTwo.maxBullets;
                        shipTwo.rightBulletAmt = shipTwo.maxBullets;

                        ScreenManager.Sound.PlayWeaponToggle(shipTwo.Position, shipOne.Position);
                        powerUpList.RemoveAt(i);
                        break;
                    }
                }

                if (powerUpList.Count < 10)
                {
                    PowerUp powerup = new PowerUp(ScreenManager.Game);

                    for (int i = buildingArray.Count - 1; i >= 0; i--)
                    {
                        while (powerup.boundingSphere.Intersects(buildingArray[i].boundingBox))
                        {
                            powerup.position = new Vector3(randomiser.Next(-460000, 500000),
                                                                randomiser.Next(5000, 450000),
                                                                randomiser.Next(-460000, 500000));
                        }
                    }

                    powerUpList.Add(powerup);
                }

                CheckGameOver(gameTime);

                #region CartoonShader
                // Update the sketch overlay texture jitter animation.
                if (Settings.SketchJitterSpeed > 0)
                {
                    timeToNextJitter -= gameTime.ElapsedGameTime;

                    if (timeToNextJitter <= TimeSpan.Zero)
                    {
                        sketchJitter.X = (float)random.NextDouble();
                        sketchJitter.Y = (float)random.NextDouble();

                        timeToNextJitter += TimeSpan.FromSeconds(Settings.SketchJitterSpeed);
                    }
                }
                #endregion

            }
            else
            {
#if XBOX360
                GamePad.SetVibration((PlayerIndex)ControllingPlayer, 0, 0);
                GamePad.SetVibration((PlayerIndex)ScreenManager.ControllingPlayerTwo, 0, 0);
#else

#endif
            }
        }

        private void UpdateShip(GameTime gameTime, Ship ship)
        {
            Ship enemyShip;
            if (ship == shipOne)
                enemyShip = shipTwo;
            else
                enemyShip = shipOne;

            if (ship.life > 0)
                ship.Update(gameTime);

            ship.UpdateAmmo(gameTime, ScreenManager, explosionParticles, enemyShip, buildingArray);
            ship.UpdateVibration(gameTime);
        }

        //private void CheckGameOver(GameTime gameTime, Ship ship)
        private void CheckGameOver(GameTime gameTime)
        {
            for (int i = 0; i < buildingArray.Count; i++)
            {
                if (shipOne.boundingSphere.Intersects(buildingArray[i].boundingBox))
                {
                    shipOne.life = 0;
                    break;
                }
            }

            for (int i = 0; i < buildingArray.Count; i++)
            {
                if (shipTwo.boundingSphere.Intersects(buildingArray[i].boundingBox))
                {
                    shipTwo.life = 0;
                    break;
                }
            }

            if ((shipOne.onAir && shipOne.Position.Y <= 0) ||
                (pOneWarning && shipOneWarningCountdown <= TimeSpan.Zero) ||
                (!shipOne.onAir && shipOne.Position.Z < 230000))
                shipOne.life = 0;

            if ((shipTwo.onAir && shipTwo.Position.Y <= 0) ||
                (pTwoWarning && shipTwoWarningCountdown <= TimeSpan.Zero) ||
                (!shipTwo.onAir && shipTwo.Position.Z > -230000))
                shipTwo.life = 0;

            //if (ship.life <= 0)
            //{
            //    ship.life = 0;
            //    ScreenManager.Sound.PlayJetExplosion(ship.Position, ship.Position);
            //    ScreenManager.Sound.StopAfterburner();
            //    if (currTime - lastExplosion > 1000)
            //    {
            //        explosionParticles.AddParticle(shipOne.Position, new Vector3(0, 0, 0));
            //        lastExplosion = (float)gameTime.TotalGameTime.TotalMilliseconds;
            //    }
            //}

            if (shipOne.life <= 0)
            {
                shipOne.life = 0;
                ScreenManager.Sound.PlayJetExplosion(shipOne.Position, shipTwo.Position);
                ScreenManager.Sound.StopAfterburner();
                if (currTime - lastExplosion > 1000)
                {
                    explosionParticles.AddParticle(shipOne.Position, new Vector3(0, 0, 0));
                    lastExplosion = (float)gameTime.TotalGameTime.TotalMilliseconds;
                }
                //pOneVibrateLeft = 1.0f;
                //pOneVibrateRight = 1.0f;

                shipOne.vibrationLeft = shipOne.vibrationRight = 1.0f;

                if (shipTwo.life > 0)
                    textFader.Show("Player 2 wins!", 2.0f, 0.0f, 0.5f);
                gameover_timer -= gameTime.ElapsedGameTime;
            }

            if (shipTwo.life <= 0)
            {
                shipTwo.life = 0;
                ScreenManager.Sound.PlayJetExplosion(shipTwo.Position, shipTwo.Position);
                ScreenManager.Sound.StopAfterburner();
                if (currTime - lastExplosion > 1000)
                {
                    explosionParticles.AddParticle(shipOne.Position, new Vector3(0, 0, 0));
                    lastExplosion = (float)gameTime.TotalGameTime.TotalMilliseconds;
                }
                //pTwoVibrateLeft = 1.0f;
                //pTwoVibrateRight = 1.0f;

                shipTwo.vibrationLeft = shipTwo.vibrationRight = 1.0f;

                if (shipOne.life > 0)
                    textFader.Show("Player 1 wins!", 2.0f, 0.0f, 0.5f);
                gameover_timer -= gameTime.ElapsedGameTime;
            }

            if (gameover_timer <= TimeSpan.Zero)
                GameOver();
        }

        private void GameOver()
        {
            pauseAlpha = 2.0f;
            ScreenManager.Sound.StopSFX();
            ScreenManager.AddScreen(new GameOverScreen(), ControllingPlayer);
        }

        private void CheckBoundaries(GameTime gameTime)
        {
            if (shipOne.Position.Z < -boundaryZ ||
                shipOne.Position.Z > boundaryZ ||
                shipOne.Position.X < -boundaryX ||
                shipOne.Position.X > boundaryX ||
                shipOne.Position.Y > boundaryY)
            {
                shipOneWarningCountdown -= gameTime.ElapsedGameTime;
                pOneWarning = true;
            }
            else
            {
                shipOneWarningCountdown = TimeSpan.FromSeconds(10);
                pOneWarning = false;
            }

            if (shipTwo.Position.Z < -boundaryZ ||
                shipTwo.Position.Z > boundaryZ ||
                shipTwo.Position.X < -boundaryX ||
                shipTwo.Position.X > boundaryX ||
                shipTwo.Position.Y > boundaryY)
            {
                shipTwoWarningCountdown -= gameTime.ElapsedGameTime;
                pTwoWarning = true;
            }
            else
            {
                shipTwoWarningCountdown = TimeSpan.FromSeconds(10);
                pTwoWarning = false;
            }
        }

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

            currTime = (float)gameTime.TotalGameTime.TotalMilliseconds;

            KeyboardState currentKeyboardState = input.CurrentKeyboardStates[playerIndex];
            KeyboardState lastKeyboardState = input.LastKeyboardStates[playerIndex];

            GamePadState currentGamePadState = input.CurrentGamePadStates[playerIndex];
            GamePadState lastGamePadState = input.LastGamePadStates[playerIndex];

            // The game pauses either if the user presses the pause button, or if
            // they unplug the active gamepad. This requires us to keep track of
            // whether a gamepad was ever plugged in, because we don't want to pause
            // on PC if they are playing with a keyboard and have no gamepad at all!
            bool gamePadDisconnected = !currentGamePadState.IsConnected && input.GamePadWasConnected[playerIndex];

            PlayerIndex player;
            if ((pauseAction.Evaluate(input, null, out player) || gamePadDisconnected && gameover_timer != TimeSpan.Zero))
            {
                ScreenManager.AddScreen(new PauseMenuScreen(), null);
            }
            else
            {
                // Switch to the next settings preset?
                if ((currentGamePadState.Buttons.Y == ButtonState.Pressed &&
                     lastGamePadState.Buttons.Y != ButtonState.Pressed))
                {
                    if (settingsIndex != 0)
                        settingsIndex = 0;
                    else if(settingsIndex == 0)
                        settingsIndex = 3;
                }

                HandlePlayerInput(gameTime, input, (PlayerIndex)ControllingPlayer);
                HandlePlayerInput(gameTime, input, (PlayerIndex)ScreenManager.ControllingPlayerTwo);
            }
        }

        private void HandlePlayerInput(GameTime gameTime, InputState input, PlayerIndex playerIndex)
        {
            Ship ship, enemyShip;
            if (playerIndex == ControllingPlayer)
            {
                ship = shipOne;
                enemyShip = shipTwo;
            }
            else
            {
                ship = shipTwo;
                enemyShip = shipOne;
            }

            PlayerIndex dummy;

#if XBOX360
            if (input.IsButtonPressed(Buttons.A, playerIndex, out dummy) || input.IsKeyPressed(Keys.F, playerIndex, out dummy))
            {
                ship.FireBullet(ScreenManager, enemyShip);
            }

            if (input.IsButtonPressed(Buttons.B, playerIndex, out dummy) || input.IsKeyPressed(Keys.G, playerIndex, out dummy))
            {
                ship.FireMissile(ScreenManager, enemyShip);
            }
#else

#endif


        }

        /// <summary>
        /// Update the values to be chased by the camera
        /// </summary>
        private void UpdateCameraChaseTarget()
        {
            cameraOne.ChasePosition = shipOne.Position;
            cameraOne.ChaseDirection = shipOne.Direction;
            cameraOne.Up = shipOne.Up;
            cameraTwo.ChasePosition = shipTwo.Position;
            cameraTwo.ChaseDirection = shipTwo.Direction;
            cameraTwo.Up = shipTwo.Up;
        }

        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // This game has a blue background. Why? Because!
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 0, 0);

            if (Settings.EnableEdgeDetect)
            {
                ScreenManager.GraphicsDevice.SetRenderTarget(normalDepthRenderTarget);

                ScreenManager.GraphicsDevice.Clear(Color.Black);
                // Draw our scene with all of our viewports and their respective view/projection matrices.
                DrawScene(gameTime, playerOneViewport, playerOneView, playerOneProjection, cameraOne, "NormalDepth");
                DrawScene(gameTime, playerTwoViewport, playerTwoView, playerTwoProjection, cameraTwo, "NormalDepth");
            }

            if (Settings.EnableEdgeDetect || Settings.EnableSketch)
                ScreenManager.GraphicsDevice.SetRenderTarget(sceneRenderTarget);
            else
                ScreenManager.GraphicsDevice.SetRenderTarget(null);

            ScreenManager.GraphicsDevice.Clear(Color.CornflowerBlue);

            string effectTechniqueName;

            if (Settings.EnableToonShading)
                effectTechniqueName = "Toon";
            else
                effectTechniqueName = "Lambert";
            DrawScene(gameTime, playerOneViewport, playerOneView, playerOneProjection, cameraOne, effectTechniqueName);
            DrawScene(gameTime, playerTwoViewport, playerTwoView, playerTwoProjection, cameraTwo, effectTechniqueName);

            // Run the postprocessing filter over the scene that we just rendered.
            if (Settings.EnableEdgeDetect || Settings.EnableSketch)
            {
                ScreenManager.GraphicsDevice.SetRenderTarget(null);

                ApplyPostprocess();
            }

            // Now we'll draw the viewport edges on top so we can visualize the viewports more easily.
            DrawViewportEdges(playerOneViewport);
            DrawViewportEdges(playerTwoViewport);

            pOneHUD.Draw(gameTime, ScreenManager.SpriteBatch, shipOne.Position, shipTwo.Position, gameFont);
            pTwoHUD.Draw(gameTime, ScreenManager.SpriteBatch, shipTwo.Position, shipOne.Position, gameFont);

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0 || pauseAlpha > 0)
            {
                float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, pauseAlpha / 2);

                ScreenManager.FadeBackBufferToBlack(alpha);
            }
        }

        /// <summary>
        /// Simple model drawing method. The interesting part here is that
        /// the view and projection matrices are tak    en from the camera object.
        /// </summary>        
        private void DrawModel(Model model, Matrix world, ChaseCamera camera)
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
                    //effect.DirectionalLight0.SpecularColor = Color.White.ToVector3();
                    effect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(-1, -1.5f, 0));
                    effect.DirectionalLight1.Enabled = true;
                    effect.DirectionalLight1.DiffuseColor = Color.LightGray.ToVector3();
                    effect.DirectionalLight1.Direction = Vector3.Normalize(new Vector3(1, -1.5f, -1));
                    effect.DirectionalLight2.Enabled = true;
                    effect.DirectionalLight2.DiffuseColor = Color.Orange.ToVector3();
                    effect.DirectionalLight2.Direction = Vector3.Normalize(new Vector3(0, -1.5f, -1));

                    effect.SpecularColor = Vector3.Zero;
                    effect.FogColor = Color.LightGoldenrodYellow.ToVector3();
                    effect.FogEnabled = true;
                    effect.FogStart = 100000.0f;
                    effect.FogEnd = 1000000.0f;

                    // Use the matrices provided by the chase camera
                    effect.View = camera.View;
                    effect.Projection = camera.Projection;
                }
                mesh.Draw();
            }
        }

        void DrawModel(Model model, Matrix world, ChaseCamera camera,
                       string effectTechniqueName)
        {
            // Look up the bone transform matrices.
            Matrix[] transforms = new Matrix[model.Bones.Count];

            model.CopyAbsoluteBoneTransformsTo(transforms);

            // Draw the model.
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    // Specify which effect technique to use.
                    effect.CurrentTechnique = effect.Techniques[effectTechniqueName];

                    Matrix localWorld = transforms[mesh.ParentBone.Index] * world;

                    effect.Parameters["World"].SetValue(localWorld);
                    effect.Parameters["View"].SetValue(camera.View);
                    effect.Parameters["Projection"].SetValue(camera.Projection);
                }

                mesh.Draw();
            }
        }

        /// <summary>
        /// DrawScene is our main rendering method. By rendering the entire scene inside of this method,
        /// we enable ourselves to be able to render the scene using any viewport we may want.
        /// </summary>
        private void DrawScene(GameTime gameTime, Viewport viewport, Matrix view, Matrix projection, ChaseCamera camera, string technique)
        {
            // Set our viewport. We store the old viewport so we can restore it when we're done in case
            // we want to render to the full viewport at some point.
            Viewport oldViewport = ScreenManager.GraphicsDevice.Viewport;
            ScreenManager.GraphicsDevice.Viewport = viewport;
            ScreenManager.GraphicsDevice.BlendState = BlendState.Opaque;
            ScreenManager.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            ScreenManager.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            // Here we'd want to draw our entire scene. For this sample, that's just the tank.
            //tank.Draw(Matrix.Identity, view, projection);
            if (shipOne.life > 0)
            {
                DrawModel(playerOneModel, shipOne.World, camera, technique);
            }
            if (shipTwo.life > 0)
            {
                DrawModel(playerTwoModel, shipTwo.World, camera, technique);
            }
            DrawModel(groundModel, Matrix.Identity, camera);

            DrawModel(skyModel, Matrix.Identity, camera);

            //DrawOverlayText();

            DrawAmmo(shipOne, camera);
            DrawAmmo(shipTwo, camera);

            //for (int i = 0; i < bulletOneList.Count; i++)
            //{
            //    DrawModel(bulletModel, bulletOneList[i].World, camera);
            //    DebugShapeRenderer.AddBoundingSphere(bulletOneList[i].bulletSphere, Color.Yellow);
            //}

            //for (int i = 0; i < missileOneList.Count; i++)
            //{
            //    DrawModel(missileModel, missileOneList[i].World, camera);
            //    DebugShapeRenderer.AddBoundingSphere(missileOneList[i].missileSphere, Color.Yellow);
            //}

            //for (int i = 0; i < bulletTwoList.Count; i++)
            //{
            //    DrawModel(bulletModel, bulletTwoList[i].World, camera);
            //    DebugShapeRenderer.AddBoundingSphere(bulletTwoList[i].bulletSphere, Color.Yellow);
            //}

            //for (int i = 0; i < missileTwoList.Count; i++)
            //{
            //    DrawModel(missileModel, missileTwoList[i].World, camera);
            //    DebugShapeRenderer.AddBoundingSphere(missileTwoList[i].missileSphere, Color.Yellow);
            //}

            for (int i = 0; i < buildingArray.Count; i++)
            {
                DrawModel(buildingArray[i].model, buildingArray[i].World, camera, technique);
                DebugShapeRenderer.AddBoundingBox(buildingArray[i].boundingBox, Color.Yellow);
            }

            for (int i = 0; i < powerUpList.Count; i++)
            {
                DrawModel(powerUpList[i].model, powerUpList[i].World, camera, technique);
                DebugShapeRenderer.AddBoundingSphere(powerUpList[i].boundingSphere, Color.Yellow);
            }

            DebugShapeRenderer.AddBoundingSphere(shipOne.boundingSphere, Color.Yellow);
            DebugShapeRenderer.AddBoundingSphere(shipTwo.boundingSphere, Color.Yellow);

            DebugShapeRenderer.AddBoundingBox(runway01_bounding, Color.Yellow);
            DebugShapeRenderer.AddBoundingBox(runway02_bounding, Color.Yellow);

            DebugShapeRenderer.Draw(gameTime, camera.View, camera.Projection);

            missileTrailParticles.SetCamera(camera.View, camera.Projection);
            explosionParticles.SetCamera(camera.View, camera.Projection);
            explosionSmokeParticles.SetCamera(camera.View, camera.Projection);

            missileTrailParticles.Draw(gameTime);
            explosionParticles.Draw(gameTime);
            explosionSmokeParticles.Draw(gameTime);

            // Now that we're done, set our old viewport back on the device
            ScreenManager.GraphicsDevice.Viewport = oldViewport;
        }

        private void DrawAmmo(Ship ship, ChaseCamera camera)
        {
            // Draw bullets
            for (int i = 0; i < ship.bullets.Count; i++)
            {
                DrawModel(bulletModel, ship.bullets[i].World, camera);
                DebugShapeRenderer.AddBoundingSphere(ship.bullets[i].bulletSphere, Color.Yellow);
            }

            // Draw missiles
            for (int i = 0; i < ship.missiles.Count; i++)
            {
                DrawModel(missileModel, ship.missiles[i].World, camera);
                DebugShapeRenderer.AddBoundingSphere(ship.missiles[i].missileSphere, Color.Yellow);
            }
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

        /// <summary>
        /// Helper applies the edge detection and pencil sketch postprocess effect.
        /// </summary>
        void ApplyPostprocess()
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            EffectParameterCollection parameters = postprocessEffect.Parameters;
            string effectTechniqueName;

            // Set effect parameters controlling the pencil sketch effect.
            if (Settings.EnableSketch)
            {
                parameters["SketchThreshold"].SetValue(Settings.SketchThreshold);
                parameters["SketchBrightness"].SetValue(Settings.SketchBrightness);
                parameters["SketchJitter"].SetValue(sketchJitter);
                parameters["SketchTexture"].SetValue(sketchTexture);
            }

            // Set effect parameters controlling the edge detection effect.
            if (Settings.EnableEdgeDetect)
            {
                Vector2 resolution = new Vector2(sceneRenderTarget.Width,
                                                 sceneRenderTarget.Height);

                Texture2D normalDepthTexture = normalDepthRenderTarget;

                parameters["EdgeWidth"].SetValue(Settings.EdgeWidth);
                parameters["EdgeIntensity"].SetValue(Settings.EdgeIntensity);
                parameters["ScreenResolution"].SetValue(resolution);
                parameters["NormalDepthTexture"].SetValue(normalDepthTexture);

                // Choose which effect technique to use.
                if (Settings.EnableSketch)
                {
                    if (Settings.SketchInColor)
                        effectTechniqueName = "EdgeDetectColorSketch";
                    else
                        effectTechniqueName = "EdgeDetectMonoSketch";
                }
                else
                    effectTechniqueName = "EdgeDetect";
            }
            else
            {
                // If edge detection is off, just pick one of the sketch techniques.
                if (Settings.SketchInColor)
                    effectTechniqueName = "ColorSketch";
                else
                    effectTechniqueName = "MonoSketch";
            }

            // Activate the appropriate effect technique.
            postprocessEffect.CurrentTechnique = postprocessEffect.Techniques[effectTechniqueName];

            // Draw a fullscreen sprite to apply the postprocessing effect.
            spriteBatch.Begin(0, BlendState.Opaque, null, null, null, postprocessEffect);
            spriteBatch.Draw(sceneRenderTarget, Vector2.Zero, Color.White);
            spriteBatch.End();
        }

        private void UpdateMissileTrail(List<Missile> missile)
        {
            if (missile.Count >= 0)
            {
                for (int i = 0; i < missile.Count; i++)
                {
                    missileTrail.Add(new Projectile(explosionParticles, explosionSmokeParticles, missileTrailParticles));
                    missileTrail[i].position = missile[i].position;
                    missileTrail[i].velocity = missile[i].velocity;
                }
            }
        }

        private void AddExplosionParticle(Vector3 position, Vector3 velocity)
        {
            Random r = new Random();
            position *= (float)r.NextDouble();
            for (int i = 0; i < 10; i++)
            {
                explosionParticles.AddParticle(position, velocity);
            }
        }

        #endregion
    }
}
