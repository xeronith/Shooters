using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Shooters
{
    public struct PlayerData
    {
        public Vector2 Position;
        public bool IsAlive;
        public Color Color;
        public float Angle;
        public float Power;
    }

    public class Game : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        GraphicsDevice device;
        SpriteBatch spriteBatch;

        Texture2D backgroundTexture, foregroundTexture,
            cannonTexture, carriageTexture, rocketTexture,
            smokeTexture, groundTexture;
        int screenWidth, screenHeight;

        SpriteFont font;

        PlayerData[] players;
        int numberOfPlayers = 4;
        float playerScaling;
        int currentPlayer = 0;

        bool rocketFlying = false;
        Vector2 rocketPosition;
        Vector2 rocketDirection;
        float rocketAngle;
        float rocketScaling = .1f;


        List<Vector2> smokeList = new List<Vector2>();
        Random randomizer = new Random();

        int[] terrainContour;

        private void GenerateTerrainContour()
        {
            terrainContour = new int[screenWidth];

            double rand1 = randomizer.NextDouble() + 1;
            double rand2 = randomizer.NextDouble() + 2;
            double rand3 = randomizer.NextDouble() + 3;

            float offset = screenHeight / 2;
            float peakHeight = 100;
            float flatness = 70;

            for (int x = 0; x < screenWidth; x++)
            {
                double height = peakHeight / rand1 * Math.Sin((float)x / flatness * rand1 + rand1);
                height += peakHeight / rand2 * Math.Sin((float)x / flatness * rand2 + rand2);
                height += peakHeight / rand3 * Math.Sin((float)x / flatness * rand3 + rand3);

                height += offset;

                terrainContour[x] = (int)height;
            }
        }

        private void CreateForeground()
        {
            Color[,] groundColors = TextureTo2DArray(groundTexture);

            Color[] foregroundColors = new Color[screenWidth * screenHeight];
            for (int x = 0; x < screenWidth; x++)
            {
                for (int y = 0; y < screenHeight; y++)
                {
                    if (y > terrainContour[x])
                        foregroundColors[x + y * screenWidth] = groundColors[x % groundTexture.Width, y % groundTexture.Height];
                    else
                        foregroundColors[x + y * screenWidth] = Color.Transparent;
                }
            }

            foregroundTexture = new Texture2D(device, screenWidth, screenHeight, false, SurfaceFormat.Color);
            foregroundTexture.SetData(foregroundColors);
        }

        public Game()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        private void SetUpPlayers()
        {
            Color[] playerColors = new Color[10];
            playerColors[0] = Color.Red;
            playerColors[1] = Color.Green;
            playerColors[2] = Color.Blue;
            playerColors[3] = Color.Purple;
            playerColors[4] = Color.Orange;
            playerColors[5] = Color.Indigo;
            playerColors[6] = Color.Yellow;
            playerColors[7] = Color.SaddleBrown;
            playerColors[8] = Color.Tomato;
            playerColors[9] = Color.Turquoise;

            players = new PlayerData[numberOfPlayers];

            for (int i = 0; i < numberOfPlayers; i++)
            {
                players[i].IsAlive = true;
                players[i].Color = playerColors[i];
                players[i].Angle = MathHelper.ToRadians(90);
                players[i].Power = 250;

                players[i].Position = new Vector2();
                players[i].Position.X = screenWidth / (numberOfPlayers + 1) * (i + 1);
                players[i].Position.Y = terrainContour[(int)players[i].Position.X];
            }
        }

        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = 500;
            graphics.PreferredBackBufferHeight = 500;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();
            Window.Title = "Shooters";

            base.Initialize();
        }

        private Color[,] TextureTo2DArray(Texture2D texture)
        {
            Color[] colors1D = new Color[texture.Width * texture.Height];
            texture.GetData(colors1D);
            Color[,] colors2D = new Color[texture.Width, texture.Height];
            for (int x = 0; x < texture.Width; x++)
            {
                for (int y = 0; y < texture.Height; y++)
                {
                    colors2D[x, y] = colors1D[x + y * texture.Width];
                }
            }
            return colors2D;
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            device = graphics.GraphicsDevice;

            backgroundTexture = Content.Load<Texture2D>("background");
            groundTexture = Content.Load<Texture2D>("ground");
            cannonTexture = Content.Load<Texture2D>("cannon");
            carriageTexture = Content.Load<Texture2D>("carriage");
            rocketTexture = Content.Load<Texture2D>("rocket");
            smokeTexture = Content.Load<Texture2D>("smoke");
            font = Content.Load<SpriteFont>("myFont");

            screenWidth = device.PresentationParameters.BackBufferWidth;
            screenHeight = device.PresentationParameters.BackBufferHeight;


            playerScaling = 40.0f / (float)carriageTexture.Width;

            GenerateTerrainContour();
            SetUpPlayers();
            FlattenTerrainBelowPlayers();
            CreateForeground();
        }

        private void FlattenTerrainBelowPlayers()
        {
            foreach (PlayerData player in players)
            {
                if (player.IsAlive)
                {
                    for (int x = 0; x < 40; x++)
                    {
                        terrainContour[(int)player.Position.X + x] = terrainContour[(int)player.Position.X];
                    }
                }
            }
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            ProcessKeyboard();

            UpdateRocket();

            base.Update(gameTime);
        }

        private void UpdateRocket()
        {
            if (rocketFlying)
            {
                Vector2 gravity = new Vector2(0, 1);
                rocketDirection += gravity / 10.0f;
                rocketPosition += rocketDirection;
                rocketAngle = (float)Math.Atan2(rocketDirection.X, -rocketDirection.Y);

                for (int i = 0; i < 5; i++)
                {
                    Vector2 smokePos = rocketPosition;
                    smokePos.X += randomizer.Next(10) - 5;
                    smokePos.Y += randomizer.Next(10) - 5;
                    smokeList.Add(smokePos);
                }
            }
        }

        private void ProcessKeyboard()
        {
            KeyboardState keyboard = Keyboard.GetState();
            if (keyboard.IsKeyDown(Keys.Left))
                players[currentPlayer].Angle -= .05f;
            if (keyboard.IsKeyDown(Keys.Right))
                players[currentPlayer].Angle += .05f;

            if (players[currentPlayer].Angle > MathHelper.PiOver2)
                players[currentPlayer].Angle = -MathHelper.PiOver2;
            if (players[currentPlayer].Angle < -MathHelper.PiOver2)
                players[currentPlayer].Angle = MathHelper.PiOver2;

            if (keyboard.IsKeyDown(Keys.Down))
                players[currentPlayer].Power -= 1;
            if (keyboard.IsKeyDown(Keys.Up))
                players[currentPlayer].Power += 1;
            if (keyboard.IsKeyDown(Keys.PageDown))
                players[currentPlayer].Power -= 20;
            if (keyboard.IsKeyDown(Keys.PageUp))
                players[currentPlayer].Power += 20;

            if (players[currentPlayer].Power > 1000)
                players[currentPlayer].Power = 1000;
            if (players[currentPlayer].Power < 0)
                players[currentPlayer].Power = 0;

            if (keyboard.IsKeyDown(Keys.Space) || keyboard.IsKeyDown(Keys.Enter))
            {
                rocketFlying = true;
                rocketPosition = players[currentPlayer].Position;
                rocketPosition.X += 20;
                rocketPosition.Y -= 10;
                rocketAngle = players[currentPlayer].Angle;
                Vector2 up = new Vector2(0, -1);
                Matrix rotationMatrix = Matrix.CreateRotationZ(rocketAngle);
                rocketDirection = Vector2.Transform(up, rotationMatrix);
                rocketDirection *= players[currentPlayer].Power / 50.0f;
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            DrawScenery();
            DrawPlayers();
            DrawText();
            DrawRocket();
            DrawSmoke();

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawScenery()
        {
            Rectangle screenRectangle = new Rectangle(0, 0, screenWidth, screenHeight);
            spriteBatch.Draw(backgroundTexture, screenRectangle, Color.White);
            spriteBatch.Draw(foregroundTexture, screenRectangle, Color.White);
        }

        private void DrawPlayers()
        {
            foreach (PlayerData player in players)
            {
                if (player.IsAlive)
                {
                    int xPos = (int)player.Position.X;
                    int yPos = (int)player.Position.Y;
                    Vector2 cannonOrigin = new Vector2(11, 50);

                    spriteBatch.Draw(cannonTexture, new Vector2(xPos + 20, yPos - 10), null, player.Color, player.Angle, cannonOrigin, playerScaling, SpriteEffects.None, 1);
                    spriteBatch.Draw(carriageTexture, player.Position, null, player.Color, 0, new Vector2(0, carriageTexture.Height), playerScaling, SpriteEffects.None, 0);
                }
            }
        }

        private void DrawText()
        {
            PlayerData player = players[currentPlayer];
            int currentAngle = (int)MathHelper.ToDegrees(player.Angle);
            spriteBatch.DrawString(font, $"Cannon Angle: {currentAngle}", new Vector2(20, 20), player.Color);
            spriteBatch.DrawString(font, $"Cannon Power: {player.Power}", new Vector2(20, 45), player.Color);
        }

        private void DrawRocket()
        {
            if (rocketFlying)
                spriteBatch.Draw(rocketTexture, rocketPosition, null, players[currentPlayer].Color, rocketAngle, new Vector2(42, 240), rocketScaling, SpriteEffects.None, 1);
        }

        private void DrawSmoke()
        {
            foreach (Vector2 smokePos in smokeList)
                spriteBatch.Draw(smokeTexture, smokePos, null, Color.White, 0, new Vector2(40, 35), .2f, SpriteEffects.None, 1);
        }
    }
}
