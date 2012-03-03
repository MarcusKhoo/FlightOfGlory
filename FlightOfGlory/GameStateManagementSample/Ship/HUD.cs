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
    public class HUD : Microsoft.Xna.Framework.DrawableGameComponent
    {
        Texture2D missile;
        Texture2D bullet;
        Texture2D healthIcon;
        Texture2D selection;
        Texture2D currLWeapon;
        Texture2D currRWeapon;
        float percentageHealth;
        int currLState;
        int currRState;
        Color[] border;
        Texture2D border_Tex;
        Radar radar;
        float rad;
        Vector2 radarOffset;
        Vector2 hudOffset;
        Rectangle safeArea;
        float _leftBullet, _rightBullet;
        float _leftMissile, _rightMissile;
        bool _warning;
        TimeSpan _countdown;
        string lifeText;
        string ammoLText;
        string ammoRText;
        float warningAlpha;
        string machText;
        string displayName;
        float _mach;

        public HUD(Game game, Viewport viewport, PlayerIndex player)
            : base(game)
        {
            safeArea = viewport.TitleSafeArea;
            missile = game.Content.Load<Texture2D>("Textures/HUD/missile");
            bullet = game.Content.Load<Texture2D>("Textures/HUD/bullet");
            healthIcon = game.Content.Load<Texture2D>("Textures/HUD/plane");
            currLState = 1;
            currRState = 1;
            border = new Color[4];
            border[0] = border[1] = border[2] = border[3] = new Color(255, 0, 0);
            border_Tex = new Texture2D(game.GraphicsDevice, 2, 2);
            border_Tex.SetData(border);
            border[0] = border[1] = border[2] = border[3] = new Color(128, 128, 128);
            selection = new Texture2D(game.GraphicsDevice, 2, 2);
            selection.SetData(border);
            toggleLWeapon();
            toggleRWeapon();
            radar = new Radar(game.Content, "Textures/Radar/redDotSmall", "Textures/Radar/yellowDotSmall", "Textures/Radar/blackDotLarge");
            toggleViewport();
            lifeText = ammoLText = ammoRText = machText = "";
            warningAlpha = 1.0f;

            //gamer profile
            displayName = DisplayName(player);
        }

        public void toggleLWeapon()
        {
            currLState += 1;
            currLState %= 2;
            switch (currLState)
            {
                case 0:
                    currLWeapon = bullet;
                    break;
                case 1:
                    currLWeapon = missile;
                    break;
            }
        }

        public void toggleRWeapon()
        {
            currRState += 1;
            currRState %= 2;
            switch (currRState)
            {
                case 0:
                    currRWeapon = bullet;
                    break;
                case 1:
                    currRWeapon = missile;
                    break;
            }
        }

        public void toggleViewport()
        {
            if (OptionsMenuScreen.currentSplitScreen == OptionsMenuScreen.SplitScreen.Horizontal)
            {
                hudOffset = new Vector2(safeArea.Width * 0.15f, safeArea.Height * 0.3f);
                radarOffset = new Vector2(safeArea.Width * -0.15f, safeArea.Height * 0.2f);
            }
            else
            {
                hudOffset = new Vector2(safeArea.Width * 0.15f, safeArea.Height * 0.13f);
                radarOffset = new Vector2(safeArea.Width * -0.15f, safeArea.Height * 0.13f);
            }
            radar.Pos = new Vector2(safeArea.Right, safeArea.Top) + radarOffset;
        }

        public void Update(GameTime gameTime, Vector3 direction, float currentHealth, float l_bullet, float l_missile, float r_bullet, float r_missile, bool warning, TimeSpan countdown, float mach)
        {
            if (currentHealth < 0)
                currentHealth = 0;
            _leftBullet = l_bullet;
            _leftMissile = l_missile;
            _rightBullet = r_bullet;
            _rightMissile = r_missile;
            _warning = warning;
            _countdown = countdown;
            _mach = (float)Math.Round(mach, 2);
            machText = "Mach: " + _mach;
            percentageHealth = currentHealth / 100.0f;
            rad = (float)Math.Atan2(direction.X, direction.Z);
            lifeText = "" + currentHealth + "%";

            if (_countdown < TimeSpan.Zero)
                _countdown = TimeSpan.Zero;

            switch (currLState)
            {
                case 0:
                    ammoLText = "" + (int)_leftBullet;
                    break;
                case 1:
                    ammoLText = "" + (int)_leftMissile;
                    break;
            }
            switch (currRState)
            {
                case 0:
                    ammoRText = "" + (int)_rightBullet;
                    break;
                case 1:
                    ammoRText = "" + (int)_rightMissile;
                    break;
            }
            if (warning)
            {
                warningAlpha -= 0.1f;

                if (warningAlpha <= 0)
                    warningAlpha = 1.0f;
            }
            else
            {
                warningAlpha = 1.0f;
            }
            base.Update(gameTime);
        }


        string GetGamerTag(PlayerIndex player)
        {
            SignedInGamer gamer = Gamer.SignedInGamers[player];

            if (gamer == null)
                return null;
            else
                return gamer.Gamertag;
        }

        string DisplayName(PlayerIndex player)
        {
            if (GetGamerTag(player) != null)
                return GetGamerTag(player);
            else
                return "Player " + player.ToString();
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Vector3 selfPos, Vector3 enemyPos, SpriteFont gameFont)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(selection,
                new Rectangle((int)(safeArea.Left + hudOffset.X + 60), (int)(safeArea.Top + hudOffset.Y), 250, 190),
                null,
                new Color(255, 255, 255, 127), 0,
                new Vector2((int)(selection.Width / 2.0f), (int)(selection.Height / 2.0f)),
                SpriteEffects.None, 0);
            spriteBatch.Draw(healthIcon,
                new Rectangle((int)(safeArea.Left + hudOffset.X), (int)(safeArea.Top + hudOffset.Y), 80, 120),
                null,
                new Color((int)(255 * (1 - percentageHealth)), (int)(255 * percentageHealth), 0),
                0,
                new Vector2((int)(healthIcon.Width / 2.0f), (int)(healthIcon.Height / 2.0f)),
                SpriteEffects.None, 0);
            spriteBatch.Draw(bullet,
                new Rectangle((int)(safeArea.Left + hudOffset.X) + 60, (int)(safeArea.Top + hudOffset.Y + 40), 8, 60),
                null,
                Color.White, 0,
                new Vector2((int)(bullet.Width / 2.0f), (int)(bullet.Height / 2.0f)),
                SpriteEffects.None, 0);
            spriteBatch.Draw(missile,
                new Rectangle((int)(safeArea.Left + hudOffset.X) + 60, (int)(safeArea.Top + hudOffset.Y - 40), 8, 60),
                null,
                Color.White, 0,
                new Vector2((int)(missile.Width / 2.0f), (int)(missile.Height / 2.0f)),
                SpriteEffects.None, 0);
            radar.Draw(spriteBatch, rad, selfPos, ref enemyPos);
            spriteBatch.DrawString(gameFont, displayName,
                new Vector2((int)(safeArea.Left + hudOffset.X + 90), (int)(safeArea.Top + hudOffset.Y)),
                Color.Black, 0,
                new Vector2((int)(gameFont.MeasureString(lifeText).X / 2.0f), (int)(gameFont.MeasureString(lifeText).Y / 2.0f)),
                1, SpriteEffects.None, 1);
            spriteBatch.DrawString(gameFont, lifeText,
                new Vector2((int)(safeArea.Left + hudOffset.X), (int)(safeArea.Top + hudOffset.Y)),
                Color.Black, 0,
                new Vector2((int)(gameFont.MeasureString(lifeText).X / 2.0f), (int)(gameFont.MeasureString(lifeText).Y / 2.0f)),
                1, SpriteEffects.None, 1);
            spriteBatch.DrawString(gameFont, "" + (_leftBullet + _rightBullet),
                new Vector2((int)(safeArea.Left + hudOffset.X + 120), (int)(safeArea.Top + hudOffset.Y + 40)),
                Color.Black, 0,
                new Vector2((int)(gameFont.MeasureString("" + _leftBullet).X / 2.0f), (int)(gameFont.MeasureString("" + _leftBullet).Y / 2.0f)),
                1, SpriteEffects.None, 1);
            spriteBatch.DrawString(gameFont, "" + (_leftMissile + _rightMissile),
                new Vector2((int)(safeArea.Left + hudOffset.X + 120), (int)(safeArea.Top + hudOffset.Y - 40)),
                Color.Black, 0,
                new Vector2((int)(gameFont.MeasureString("" + _leftMissile).X / 2.0f), (int)(gameFont.MeasureString("" + _leftMissile).Y / 2.0f)),
                1, SpriteEffects.None, 1);
            spriteBatch.DrawString(gameFont, machText,
                new Vector2((int)(safeArea.Left + hudOffset.X), (int)(safeArea.Top + hudOffset.Y + 120)),
                new Color((int)(255 * _mach / 10), 0, 0), 0,
                new Vector2((int)(gameFont.MeasureString(machText).X / 2.0f), (int)(gameFont.MeasureString(machText).Y / 2.0f)),
                2, SpriteEffects.None, 1);
            if (_warning)
            {
                spriteBatch.DrawString(gameFont, "WARNING",
                    new Vector2((int)(safeArea.Left + safeArea.Width / 2.0f), (int)(safeArea.Top + safeArea.Height * 0.3f)),
                    new Color(1, 0, 0, warningAlpha), 0,
                    new Vector2((int)(gameFont.MeasureString("WARNING").X / 2.0f), (int)(gameFont.MeasureString("WARNING").Y / 2.0f)),
                    1.5f, SpriteEffects.None, 1);
                spriteBatch.DrawString(gameFont, "RETURN TO THE BATTLEFIELD!!!",
                    new Vector2((int)(safeArea.Left + safeArea.Width / 2.0f), (int)(safeArea.Top + safeArea.Height * 0.3f + 32)),
                    new Color(1, 0, 0, warningAlpha), 0,
                    new Vector2((int)(gameFont.MeasureString("RETURN TO THE BATTLEFIELD!!!").X / 2.0f), (int)(gameFont.MeasureString("RETURN TO THE BATTLEFIELD!!!").Y / 2.0f)),
                    1.2f, SpriteEffects.None, 1);
                spriteBatch.DrawString(gameFont,
                    "" + (int)_countdown.TotalSeconds,
                    new Vector2((int)(safeArea.Left + safeArea.Width / 2.0f), (int)(safeArea.Top + safeArea.Height * 0.3f + 96)),
                    new Color(1, 0, 0, warningAlpha), 0,
                    new Vector2(gameFont.MeasureString("" + (int)(_countdown.TotalSeconds / 2.0f)).X, gameFont.MeasureString("" + (int)(_countdown.TotalSeconds / 2.0)).Y),
                    2.0f, SpriteEffects.None, 1);
            }
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
