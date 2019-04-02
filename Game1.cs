using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SimCityScope
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        public SpriteBatch spriteBatch;

        World world;

        MouseState prevMS;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 800;
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
            this.IsMouseVisible = true;

            world = new World(20);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            GeometryDrawer.init(this);


            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            if (Mouse.GetState().LeftButton == ButtonState.Pressed && prevMS.LeftButton==ButtonState.Released)
            {
                int x = (Mouse.GetState().X - 20) / world.tilesize;
                int y = (Mouse.GetState().Y - 20) / world.tilesize;
                if(x>=0 && x<world.size && y>=0 && y<world.size)
                    world.grid[x, y].active ^= true;
            }

            prevMS = Mouse.GetState();
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            // draw grid
            Vector2 start = new Vector2(20, 20);
            for (int x = 0; x < world.size + 1; ++x)
            {
                GeometryDrawer.drawLine(start + new Vector2(x * world.tilesize, 0), start + new Vector2(x, world.size) * world.tilesize, Color.Black);   // vertical lines
                GeometryDrawer.drawLine(start + new Vector2(0, x * world.tilesize), start + new Vector2(world.size, x) * world.tilesize, Color.Black);   // horizontal lines
            }
            // TODO: draw tiles
            for (int x = 0; x < world.size; ++x)
            {
                for (int y = 0; y < world.size; ++y)
                {
                    if(world.grid[x,y].active)
                    {
                        Vector2 a = start + new Vector2(x, y) * world.tilesize;
                        GeometryDrawer.fillRect(a.ToPoint(), world.tilesize, world.tilesize, Color.White);
                    }
                }
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
