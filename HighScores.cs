using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;

namespace MyPacMan
{
    public class HighScores : DrawableGameComponent
    {
        List<string> scores;
        SpriteFont scoreFont;
        SpriteFont itemFont;
        Texture2D selectionArrow;
        SpriteBatch spriteBatch;
        GraphicsDeviceManager graphics;
        KeyboardState oldState;
        public HighScores(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            scores = new List<string>(10);
            const string fileName = "highscores.txt";
            if (File.Exists(fileName))
            {
                scores = File.ReadAllLines(fileName).ToList<string>();
                scores.Sort((a, b) => Convert.ToInt32(a).CompareTo(Convert.ToInt32(b)));
                scores.Reverse();
            }
            scoreFont = Game.Content.Load<SpriteFont>("Score");
            itemFont = Game.Content.Load<SpriteFont>("MenuItem");
            selectionArrow = Game.Content.Load<Texture2D>("sprites/Selection");
            spriteBatch = (SpriteBatch)Game.Services.GetService(typeof(SpriteBatch));
            graphics = (GraphicsDeviceManager)Game.Services.GetService(typeof(GraphicsDeviceManager));
            oldState = Keyboard.GetState();
            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here
            if (Keyboard.GetState().GetPressedKeys().Length > 0 && oldState.GetPressedKeys().Length == 0)
            {
                Game.Components.Remove(this);
                Game.Components.Add(new Menu(Game, null));
            }
            oldState = Keyboard.GetState();
            base.Update(gameTime);
        }

        /// <summary>
        /// Allows the component to draw itself
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            Vector2 position = new Vector2(graphics.PreferredBackBufferWidth / 2 - 150, graphics.PreferredBackBufferHeight / 2 - 200);
            spriteBatch.Begin();
            for (int i = 0; i < 10; i++)
            {
                spriteBatch.DrawString(scoreFont, (i + 1).ToString() + ".", new Vector2(position.X, position.Y + (30 * i)), Color.White);
                if (i < scores.Count)
                {
                    spriteBatch.DrawString(scoreFont, scores[i], new Vector2(position.X + 50, position.Y + (30 * i)), Color.White);
                }
            }

            Vector2 itemPosition;
            itemPosition.X = (graphics.PreferredBackBufferWidth / 2) - 100;
            itemPosition.Y = (graphics.PreferredBackBufferHeight / 2) + 200;
            spriteBatch.Draw(selectionArrow, new Vector2(itemPosition.X - 50, itemPosition.Y), Color.White);
            spriteBatch.DrawString(itemFont, "Return", itemPosition, Color.Yellow);

            spriteBatch.End();


        }
    }
}
