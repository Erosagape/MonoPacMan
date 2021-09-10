using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace MyPacMan
{
    public class MonoPacman : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public MonoPacman()
        {
            // Pac Man 2 is somewhat resolution-independent, but runs best at 720x640.
            graphics = new GraphicsDeviceManager(this);
            //graphics.PreferredBackBufferHeight = 720;
            //graphics.PreferredBackBufferWidth = 640;
            //graphics.ApplyChanges();

            // Pac Man 2 always updates 1000 times per second. Framerate may vary.
            IsFixedTimeStep = true;
            TargetElapsedTime = TimeSpan.FromMilliseconds(1);

            // The menu needs to be added in the Components list in the constructor.
            // Otherwise its Initialize() method is not called and everything blows up.
            Components.Add(new Menu(this, null));

            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Services.AddService(typeof(SpriteBatch), spriteBatch);
            Services.AddService(typeof(GraphicsDeviceManager), graphics);
            base.Initialize();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // The components are automatically updated by XNA. The only relevant
            // task here is to update the AudioEngine.
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // We always clear to black, so we do this here.
            GraphicsDevice.Clear(Color.Black);

            base.Draw(gameTime);
        }

    }
}
