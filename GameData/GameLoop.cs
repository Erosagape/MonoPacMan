using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace MyPacMan
{
    public class GameLoop : DrawableGameComponent
    {
        Dictionary<string, Texture2D> bonus;
        Texture2D xlife;
        Texture2D board;
        Texture2D boardFlash;
        Texture2D crump;
        Texture2D ppill;
        SpriteFont scoreFont;
        SpriteFont scoreEventFont;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        List<Ghost> ghosts;
        Player player;
        TimeSpan lockTimer;
        DateTime eventTimer;
        int bonusSpawned;
        bool bonusPresent;
        DateTime bonusSpawnedTime;
        Dictionary<string, int> bonusEaten;
        bool playerDied;
        bool paChomp;
        int xlives;
        int score;
        int eatenGhosts;
        List<ScoreEvent> scoreEvents;
        public int Score
        {
            get { return score; }
            private set
            {
                if ((value / 1000) > (score / 10000))
                {
                    xlives++;
                }
                score = value;
            }
        }
        private TimeSpan LockTimer
        {
            get { return lockTimer; }
            set { eventTimer = DateTime.Now; lockTimer = value; }
        }
        public GameLoop(Game game) :base(game)
        {

        }
        public override void Initialize()
        {
            Grid.Reset();
            Constants.Level = 1;
            spriteBatch = (SpriteBatch)Game.Services.GetService(typeof(SpriteBatch));
            graphics = (GraphicsDeviceManager)Game.Services.GetService(typeof(GraphicsDeviceManager));

            scoreFont = Game.Content.Load<SpriteFont>("Score");
            scoreEventFont = Game.Content.Load<SpriteFont>("ScoreEvent");
            xlife = Game.Content.Load<Texture2D>("sprites/ExtraLife");
            ppill = Game.Content.Load<Texture2D>("sprites/PowerPill");
            crump = Game.Content.Load<Texture2D>("sprites/Crump");
            board = Game.Content.Load<Texture2D>("sprites/Board");
            boardFlash = Game.Content.Load<Texture2D>("sprites/BoardFlash");
            bonusEaten = new Dictionary<string, int>();
            bonus = new Dictionary<string, Texture2D>(9);
            bonus.Add("Apple", Game.Content.Load<Texture2D>("bonus/Apple"));
            bonus.Add("Banana", Game.Content.Load<Texture2D>("bonus/Banana"));
            bonus.Add("Bell", Game.Content.Load<Texture2D>("bonus/Bell"));
            bonus.Add("Cherry", Game.Content.Load<Texture2D>("bonus/Cherry"));
            bonus.Add("Key", Game.Content.Load<Texture2D>("bonus/Key"));
            bonus.Add("Orange", Game.Content.Load<Texture2D>("bonus/Orange"));
            bonus.Add("Pear", Game.Content.Load<Texture2D>("bonus/Pear"));
            bonus.Add("Pretzel", Game.Content.Load<Texture2D>("bonus/Pretzel"));
            bonus.Add("Strawberry", Game.Content.Load<Texture2D>("bonus/Strawberry"));

            scoreEvents = new List<ScoreEvent>(5);
            bonusPresent = false;
            bonusSpawned = 0;
            eatenGhosts = 0;
            Score = 0;
            xlives = 2;
            paChomp = true;
            playerDied = false;
            player = new Player(Game);
            ghosts = new List<Ghost> { new Ghost(Game, player, Ghosts.Blinky), new Ghost(Game, player, Ghosts.Clyde),
                                        new Ghost(Game, player, Ghosts.Inky), new Ghost(Game, player, Ghosts.Pinky)};
            ghosts[2].SetBlinky(ghosts[0]); // Oh, dirty hack. Inky needs this for his AI.
            LockTimer = TimeSpan.FromMilliseconds(4500);

            base.Initialize();
        }
        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {

            // Some events (death, new level, etc.) lock the game for a few moments.
            if (DateTime.Now - eventTimer < LockTimer)
            {
                ghosts.ForEach(i => i.LockTimer(gameTime));
                // Also we need to do the same thing for our own timer concerning bonuses
                bonusSpawnedTime += gameTime.ElapsedGameTime;
                return;
            }

            // Remove special events older than 5 seconds
            scoreEvents.RemoveAll(i => DateTime.Now - i.When > TimeSpan.FromSeconds(5));

            // If the player had died, spawn a new one or end game.
            if (playerDied)
            {
                // extra lives are decremented here, at the same time the pac man is spawned; this makes those 
                // events seem linked.
                xlives--;
                //xlives++; // Give infinite lives to the evil developer;
                if (xlives >= 0)
                {
                    playerDied = false;
                    player = new Player(Game);
                    ghosts.ForEach(i => i.Reset(false, player));
                    scoreEvents.Clear();
                }
                else
                { // The game is over
                    Menu.SaveHighScore(Score);
                    Game.Components.Add(new Menu(Game, null));
                    Game.Components.Remove(this);
                    return;
                }
            }

            // When all crumps have been eaten, wait a few seconds and then spawn a new level
            if (noCrumpsLeft())
            {
                if (Constants.Level < 21)
                {
                    bonusSpawned = 0;
                    Grid.Reset();
                    player = new Player(Game);
                    ghosts.ForEach(i => i.Reset(true, player));
                    LockTimer = TimeSpan.FromSeconds(2);
                    Constants.Level++;
                    return;
                }
                else
                { // Game over, you win.
                    Menu.SaveHighScore(Score);
                    Game.Components.Add(new Menu(Game, null));
                    Game.Components.Remove(this);
                    return;
                }
            }

            Keys[] inputKeys = Keyboard.GetState().GetPressedKeys();
            // The user may escape to the main menu with the escape key
            if (inputKeys.Contains(Keys.Escape))
            {
                Game.Components.Add(new Menu(Game, this));
                Game.Components.Remove(this);
                // if the player subsequently quits the game, since we'll re-initialize GhostSoundManager in
                // Initialize() if the player wants to start a new game.
                return;
            }

            // Eat crumps and power pills.
            if (player.Position.DeltaPixel == Point.Zero)
            {
                Point playerTile = player.Position.Tile;
                if (Grid.TileGrid[playerTile.X, playerTile.Y].HasCrump)
                {
                    paChomp = !paChomp;
                    Score += 10;
                    Grid.TileGrid[playerTile.X, playerTile.Y].HasCrump = false;
                    if (Grid.TileGrid[playerTile.X, playerTile.Y].HasPowerPill)
                    {
                        Score += 40;
                        eatenGhosts = 0;
                        for (int i = 0; i < ghosts.Count; i++)
                        {
                            if (ghosts[i].State == GhostState.Attack || ghosts[i].State == GhostState.Scatter ||
                                ghosts[i].State == GhostState.Blue)
                            {
                                ghosts[i].State = GhostState.Blue;
                            }
                        }
                        Grid.TileGrid[playerTile.X, playerTile.Y].HasPowerPill = false;
                    }

                    // If that was the last crump, lock the game for a while
                    if (noCrumpsLeft())
                    {
                        LockTimer = TimeSpan.FromSeconds(2);
                        return;
                    }
                }
            }

            // Eat bonuses
            if (bonusPresent && player.Position.Tile.Y == 17 &&
                ((player.Position.Tile.X == 13 && player.Position.DeltaPixel.X == 8) ||
                  (player.Position.Tile.X == 14 && player.Position.DeltaPixel.X == -8)))
            {
                LockTimer = TimeSpan.FromSeconds(1.5);
                Score += Constants.BonusScores();
                scoreEvents.Add(new ScoreEvent(player.Position, DateTime.Now, Constants.BonusScores()));
                bonusPresent = false;
                if (bonusEaten.ContainsKey(Constants.BonusSprite()))
                {
                    bonusEaten[Constants.BonusSprite()]++;
                }
                else
                {
                    bonusEaten.Add(Constants.BonusSprite(), 1);
                }
            }

            // Remove bonus if time's up
            if (bonusPresent && ((DateTime.Now - bonusSpawnedTime) > TimeSpan.FromSeconds(10)))
            {
                bonusPresent = false;
            }

            // Detect collision between ghosts and the player
            foreach (Ghost ghost in ghosts)
            {
                Rectangle playerArea = new Rectangle((player.Position.Tile.X * 16) + player.Position.DeltaPixel.X,
                                                     (player.Position.Tile.Y * 16) + player.Position.DeltaPixel.Y,
                                                      26,
                                                      26);
                Rectangle ghostArea = new Rectangle((ghost.Position.Tile.X * 16) + ghost.Position.DeltaPixel.X,
                                                    (ghost.Position.Tile.Y * 16) + ghost.Position.DeltaPixel.Y,
                                                    22,
                                                    22);
                if (!Rectangle.Intersect(playerArea, ghostArea).IsEmpty)
                {
                    // If collision detected, either kill the ghost or kill the pac man, depending on state.

                    if (ghost.State == GhostState.Blue)
                    {
                        ghost.State = GhostState.Dead;
                        eatenGhosts++;
                        int bonus = (int)(100 * Math.Pow(2, eatenGhosts));
                        Score += bonus;
                        scoreEvents.Add(new ScoreEvent(ghost.Position, DateTime.Now, bonus));
                        LockTimer = TimeSpan.FromMilliseconds(900);
                        return;
                    }
                    else if (ghost.State != GhostState.Dead)
                    {
                        KillPacMan();
                        return;
                    }
                    // Otherwise ( = the ghost is dead), don't do anything special.
                }
            }

            // Periodically spawn a fruit, when the player isn't on the spawn location
            // otherwise we get an infinite fruit spawning bug
            if ((Grid.NumCrumps == 180 || Grid.NumCrumps == 80) && bonusSpawned < 2 &&
                !(player.Position.Tile.Y == 17 &&
                    ((player.Position.Tile.X == 13 && player.Position.DeltaPixel.X == 8) ||
                    (player.Position.Tile.X == 14 && player.Position.DeltaPixel.X == -8))))
            {
                bonusPresent = true;
                bonusSpawned++;
                bonusSpawnedTime = DateTime.Now;

            }

            // Now is the time to move player based on inputs and ghosts based on AI
            // If we have returned earlier in the method, they stay in place
            player.Update(gameTime);
            ghosts.ForEach(i => i.Update(gameTime));

            base.Update(gameTime);
        }


        /// <summary>
        /// Nice to have for debug purposes. We might want the level to end early.
        /// </summary>
        /// <returns>Whether there are no crumps left on the board.</returns>
        bool noCrumpsLeft()
        {
            return Grid.NumCrumps == 0;
        }


        /// <summary>
        /// AAAARRRGH
        /// </summary>
        void KillPacMan()
        {
            player.State = State.Dying;
            LockTimer = TimeSpan.FromMilliseconds(1811);
            playerDied = true;
            bonusPresent = false;
            bonusSpawned = 0;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            // The GameLoop is a main component, so it is responsible for initializing the sprite batch each frame
            spriteBatch.Begin();

            Vector2 boardPosition = new Vector2(
                (graphics.PreferredBackBufferWidth / 2) - (board.Width / 2),
                (graphics.PreferredBackBufferHeight / 2) - (board.Height / 2)
                );

            // When all crumps have been eaten, flash until new level is spawned
            // Draw the player and nothing else, just end the spritebatch and return.
            if (noCrumpsLeft())
            {
                spriteBatch.Draw(((DateTime.Now.Second * 1000 + DateTime.Now.Millisecond) / 350) % 2 == 0 ? board : boardFlash, boardPosition, Color.White);
                player.Draw(gameTime, boardPosition);
                spriteBatch.End();
                return;
            }
            // Otherwise...

            // Draw the board
            spriteBatch.Draw(board, boardPosition, Color.White);

            // Draw crumps and power pills
            Tile[,] tiles = Grid.TileGrid;
            for (int j = 0; j < Grid.Height; j++)
            {
                for (int i = 0; i < Grid.Width; i++)
                {
                    if (tiles[i, j].HasPowerPill)
                    {
                        spriteBatch.Draw(ppill, new Vector2(
                            boardPosition.X + 3 + (i * 16),
                            boardPosition.Y + 3 + (j * 16)),
                            Color.White);
                    }
                    else if (tiles[i, j].HasCrump)
                    {
                        spriteBatch.Draw(crump, new Vector2(
                            boardPosition.X + 5 + (i * 16),
                            boardPosition.Y + 5 + (j * 16)),
                            Color.White);
                    }
                }
            }

            // Draw extra lives; no more than 20 though
            for (int i = 0; i < xlives && i < 20; i++)
            {
                spriteBatch.Draw(xlife, new Vector2(boardPosition.X + 10 + (20 * i), board.Height + boardPosition.Y + 10), Color.White);
            }

            // Draw current score
            spriteBatch.DrawString(scoreFont, "SCORE", new Vector2(30, 50), Color.White);
            spriteBatch.DrawString(scoreFont, Score.ToString(), new Vector2(30, 30), Color.White);

            // Draw current level
            spriteBatch.DrawString(scoreFont, "LEVEL", new Vector2(100, 50), Color.White);
            spriteBatch.DrawString(scoreFont, Constants.Level.ToString(), new Vector2(100, 30), Color.White);

            // Draw a bonus fruit if any
            if (bonusPresent)
            {
                spriteBatch.Draw(bonus[Constants.BonusSprite()], new Vector2(boardPosition.X + (13 * 16) + 2, boardPosition.Y + (17 * 16) - 8), Color.White);
            }

            // Draw captured bonus fruits at the bottom of the screen
            int k = 0;
            foreach (KeyValuePair<string, int> kvp in bonusEaten)
            {
                for (int i = 0; i < kvp.Value; i++)
                {
                    spriteBatch.Draw(bonus[kvp.Key], new Vector2(boardPosition.X + 10 + (22 * (k + i)), board.Height + boardPosition.Y + 22), Color.White);
                }
                k += kvp.Value;
            }

            // Draw ghosts
            ghosts.ForEach(i => i.Draw(gameTime, boardPosition));

            // Draw player
            player.Draw(gameTime, boardPosition);

            // Draw special scores (as when a ghost or fruit has been eaten)
            foreach (ScoreEvent se in scoreEvents)
            {
                spriteBatch.DrawString(scoreEventFont, se.Score.ToString(), new Vector2(boardPosition.X + (se.Position.Tile.X * 16) + se.Position.DeltaPixel.X + 4,
                                                                                           boardPosition.Y + (se.Position.Tile.Y * 16) + se.Position.DeltaPixel.Y + 4), Color.White);
            }

            // Draw GET READY ! at level start
            if (player.State == State.Start)
            {
                spriteBatch.DrawString(scoreFont, "GET READY!", new Vector2(boardPosition.X + (board.Width / 2) - 58, boardPosition.Y + 273), Color.Yellow);
            }

            // Display number of crumps (for debug)
            //spriteBatch.DrawString(scoreFont, "Crumps left :" + Grid.NumCrumps.ToString(), Vector2.Zero, Color.White);

            spriteBatch.End();
        }
    }
}
