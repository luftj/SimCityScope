using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

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
        Vector2 camOffset;

        SpriteFont font;

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
            camOffset = new Vector2(20, 20);

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
            font = Content.Load<SpriteFont>("testfont");
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

            // mouse interaction
            if (Mouse.GetState().LeftButton == ButtonState.Pressed && prevMS.LeftButton==ButtonState.Released)
            {
                var pos = screenToWorld(Mouse.GetState().Position);
                if (pos != null)
                    world.grid[(int)pos?.X, (int)pos?.Y].active ^= true;
            }

            // touch interaction
            foreach(var touch in TouchPanel.GetState())
            {
                // You're looking for when they finish a drag, so only check
                // released touches.
                if (touch.State != TouchLocationState.Released)
                    continue;

                TouchLocation prevLoc;

                // Sometimes TryGetPreviousLocation can fail. Bail out early if this happened
                // or if the last state didn't move
                if (!touch.TryGetPreviousLocation(out prevLoc) || prevLoc.State != TouchLocationState.Moved)
                    continue;

                var pos = screenToWorld(touch.Position.ToPoint());
                if (pos != null)
                    world.grid[(int)pos?.X, (int)pos?.Y].active ^= true;

                // get your delta
                //var delta = touch.Position - prevLoc.Position;

                // Usually you don't want to do something if the user drags 1 pixel.
                //if (delta.LengthSquared() < YOUR_DRAG_TOLERANCE)
                //    continue;

                //if (delta.X < 0 || delta.Y < 0)
                //    return new RotateLeftCommand(_gameController);

                //if (delta.X > 0 || delta.Y > 0)
                //    return new RotateRightCommand(_gameController);
            }

            // move view
            if (Keyboard.GetState().IsKeyDown(Keys.W)) camOffset.Y++;
            if (Keyboard.GetState().IsKeyDown(Keys.A)) camOffset.X++;
            if (Keyboard.GetState().IsKeyDown(Keys.S)) camOffset.Y--;
            if (Keyboard.GetState().IsKeyDown(Keys.D)) camOffset.X--;

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
            for (int x = 0; x < world.size + 1; ++x)
            {
                GeometryDrawer.drawLine(camOffset + new Vector2(x * world.tilesize, 0), camOffset + new Vector2(x, world.size) * world.tilesize, Color.Black);   // vertical lines
                GeometryDrawer.drawLine(camOffset + new Vector2(0, x * world.tilesize), camOffset + new Vector2(world.size, x) * world.tilesize, Color.Black);   // horizontal lines
            }
            // draw tiles
            for (int x = 0; x < world.size; ++x)
            {
                for (int y = 0; y < world.size; ++y)
                {
                    if(world.grid[x,y].active)
                    {
                        Vector2 a = camOffset + new Vector2(x, y) * world.tilesize;
                        GeometryDrawer.fillRect(a.ToPoint(), world.tilesize, world.tilesize, Color.White);
                    }
                }
            }

            // todo: draw interface

            // draw debug output
            spriteBatch.DrawString(font, TouchPanel.GetState().Count.ToString(), Vector2.One, Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        Vector2? screenToWorld(Point screenPos)
        {
            int x = (screenPos.X - (int)camOffset.X) / world.tilesize;
            int y = (screenPos.Y - (int)camOffset.Y) / world.tilesize;
            if (x >= 0 && x < world.size && y >= 0 && y < world.size)
                return new Vector2(x, y);
            else return null;
        }
    }
}
