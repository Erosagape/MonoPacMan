using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MyPacMan
{
    public class Menu : DrawableGameComponent
    {
        GameLoop gameLoop;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont menuItem;
        string[] items;
        int selection;
        bool gameStart;
        Texture2D title;
        Texture2D selectionArrow;

        KeyboardState oldState;
        public Menu(Game game, GameLoop gameLoop)
            : base(game)
        {
            this.gameLoop = gameLoop;
            gameStart = (gameLoop == null);
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            spriteBatch = (SpriteBatch)Game.Services.GetService(typeof(SpriteBatch));
            graphics = (GraphicsDeviceManager)Game.Services.GetService(typeof(GraphicsDeviceManager));
            selection = 0;
            if (gameLoop == null)
            {
                items = new string[] { "New Game", "High Scores", "Quit" };
            }
            else
            {
                items = new string[] { "Resume", "Quit Game" };
            }
            menuItem = Game.Content.Load<SpriteFont>("MenuItem");
            title = Game.Content.Load<Texture2D>("sprites/Title");
            selectionArrow = Game.Content.Load<Texture2D>("sprites/Selection");
            oldState = Keyboard.GetState();
            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            // Wonder why we test for this condition? Just replace gameStart by true and
            // try running the game. The answer should be instantaneous.
            if (gameStart)
            {
                gameStart = false;
            }

            KeyboardState newState = Keyboard.GetState();

            // Get keys pressed now that weren't pressed before
            var newPressedKeys = from k in newState.GetPressedKeys()
                                 where !(oldState.GetPressedKeys().Contains(k))
                                 select k;

            // Scroll through menu items
            if (newPressedKeys.Contains(Keys.Down))
            {
                selection++;
                selection %= items.Length;
            }
            else if (newPressedKeys.Contains(Keys.Up))
            {
                selection--;
                selection = (selection < 0 ? items.Length - 1 : selection);
            }
            else if (newPressedKeys.Contains(Keys.Enter))
            {
                menuAction();
            }

            // Update keyboard state for next update
            oldState = newState;


        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            // The menu is a main component, so it is responsible for initializing the sprite batch each frame
            spriteBatch.Begin();

            // Draw title
            spriteBatch.Draw(title, new Vector2((graphics.PreferredBackBufferWidth / 2) - (title.Width / 2), 75), Color.White);

            // Draw items
            Vector2 itemPosition;
            itemPosition.X = (graphics.PreferredBackBufferWidth / 2) - 100;
            for (int i = 0; i < items.Length; i++)
            {

                itemPosition.Y = (graphics.PreferredBackBufferHeight / 2) - 60 + (60 * i);
                if (i == selection)
                {
                    spriteBatch.Draw(selectionArrow, new Vector2(itemPosition.X - 50, itemPosition.Y), Color.White);
                }
                spriteBatch.DrawString(menuItem, items[i], itemPosition, Color.Yellow);
            }

            spriteBatch.End();
        }

        void menuAction()
        {
            Game.Components.Remove(this);
            switch (items[selection])
            {
                case ("Resume"):
                    Game.Components.Add(gameLoop);
                    break;
                case ("New Game"):
                    Game.Components.Add(new GameLoop(Game));
                    break;
                case ("High Scores"):
                    Game.Components.Add(new HighScores(Game));
                    break;
                case ("Quit"):
                    Game.Exit();
                    break;
                case ("Quit Game"):
                    Game.Components.Add(new Menu(Game, null));
                    SaveHighScore(gameLoop.Score);
                    break;
                default:
                    throw new ArgumentException("\"" + items[selection] + "\" is not a valid case");

            }
        }

        /// <summary>
        /// Keep a history of the best 10 scores
        /// </summary>
        /// <param name="highScore">New score to save, might make it inside the list, might not.</param>
        public static void SaveHighScore(int highScore)
        {
            const string fileName = "highscores.txt";
            if (!File.Exists(fileName))
            {
                File.WriteAllLines(fileName, new string[] { highScore.ToString() });
            }
            else
            {
                List<string> contents = File.ReadAllLines(fileName).ToList<string>();
                contents.Add(highScore.ToString());
                if (contents.Count >= 10)
                {
                    contents.Sort((a, b) => Convert.ToInt32(a).CompareTo(Convert.ToInt32(b)));
                    while (contents.Count > 10)
                    {
                        contents.RemoveAt(0);
                    }
                }
                File.WriteAllLines(fileName, contents.ToArray());
            }
        }
    }
}
