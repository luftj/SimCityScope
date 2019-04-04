using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System.Collections.Generic;

namespace SimCityScope
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        World world;

        #region TIME
        bool running = true;
        int timeStep = 0; // total elapsed time steps
        float speed = 2.0f; // seconds per time step
        double stepTimer = 0.0f;
        #endregion

        #region VIEW
        GraphicsDeviceManager graphics;
        public SpriteBatch spriteBatch;

        Dictionary<string, Texture2D> sprites;
        SpriteFont font;
        int windowWidth = 800;
        int windowHeight = 800;
        #endregion

        #region CONROL
        KeyboardState prevKB;
        MouseState prevMS;
        Vector2 camOffset;
        List<InterfaceElement> interfaceElements;
        InterfaceState state;

        Point? dragStart = null;
        #endregion

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = windowWidth;
            graphics.PreferredBackBufferHeight = windowHeight;
            Content.RootDirectory = "Content";

            sprites = new Dictionary<string, Texture2D>();

            interfaceElements = new List<InterfaceElement>();
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
            camOffset = new Vector2(90, 20);

            interfaceElements.Add(new InterfaceElement("", null));
            interfaceElements.Add(new InterfaceElement("remove", delegate () { state = InterfaceState.REMOVE; }));
            interfaceElements.Add(new InterfaceElement("road", delegate () { state = InterfaceState.ROAD; }));
            interfaceElements.Add(new InterfaceElement("commercial", delegate () { state = InterfaceState.COMM; }));
            interfaceElements.Add(new InterfaceElement("residential", delegate () { state = InterfaceState.RES; }));

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


            // use this.Content to load game content here
            font = Content.Load<SpriteFont>("testfont");
            sprites["remove"] = Content.Load<Texture2D>("bulldozer");
            InterfaceElement.width = sprites["remove"].Width;
            InterfaceElement.height = sprites["remove"].Height;
            sprites["road"] = Content.Load<Texture2D>("road");
            sprites["residential"] = Content.Load<Texture2D>("residential");
            sprites["commercial"] = Content.Load<Texture2D>("commercial");
            sprites["road_NS"] = Content.Load<Texture2D>("roads/NS");
            sprites["road_EW"] = Content.Load<Texture2D>("roads/EW");
            sprites["road_NE"] = Content.Load<Texture2D>("roads/NE");
            sprites["road_ES"] = Content.Load<Texture2D>("roads/ES");
            sprites["road_SW"] = Content.Load<Texture2D>("roads/SW");
            sprites["road_NW"] = Content.Load<Texture2D>("roads/NW");
            sprites["road_NES"] = Content.Load<Texture2D>("roads/NES");
            sprites["road_ESW"] = Content.Load<Texture2D>("roads/ESW");
            sprites["road_NSW"] = Content.Load<Texture2D>("roads/NSW");
            sprites["road_NEW"] = Content.Load<Texture2D>("roads/NEW");
            sprites["road_NESW"] = Content.Load<Texture2D>("roads/NESW");


            sprites["res_1"] = Content.Load<Texture2D>("residential_high/residential_high_1");
            sprites["res_2"] = Content.Load<Texture2D>("residential_high/residential_high_2");
            sprites["res_3"] = Content.Load<Texture2D>("residential_high/residential_high_3");
            sprites["res_4"] = Content.Load<Texture2D>("residential_high/residential_high_4");
            sprites["res_5"] = Content.Load<Texture2D>("residential_high/residential_high_5");
            sprites["comm_1"] = Content.Load<Texture2D>("commercial_high/000");
            sprites["comm_2"] = Content.Load<Texture2D>("commercial_high/001");
            sprites["comm_3"] = Content.Load<Texture2D>("commercial_high/002");
            sprites["comm_4"] = Content.Load<Texture2D>("commercial_high/003");
            sprites["comm_5"] = Content.Load<Texture2D>("commercial_high/004");

            sprites["cars_hor_0"] = Content.Load<Texture2D>("traffic/cars_hor");
            sprites["cars_hor_1"] = Content.Load<Texture2D>("traffic/cars_hor2");
            sprites["cars_ver_0"] = Content.Load<Texture2D>("traffic/cars_ver");
            sprites["cars_ver_1"] = Content.Load<Texture2D>("traffic/cars_ver2");
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

            // simulation steps
            if (running)
            {
                stepTimer += gameTime.ElapsedGameTime.TotalSeconds;
                if (stepTimer >= speed)
                {
                    timeStep++;
                    stepTimer -= speed;

                    world.updateStep(); // the magic happens here
                }
            }

            MouseState mouse = Mouse.GetState();
            // mouse interaction
            if (mouse.LeftButton == ButtonState.Pressed && prevMS.LeftButton == ButtonState.Released)
            {
                var pos = screenToWorld(Mouse.GetState().Position);
                if (pos != null)
                    dragStart = mouse.Position;
                else
                    dragStart = null;
            }

            if (mouse.LeftButton == ButtonState.Released && prevMS.LeftButton == ButtonState.Pressed)
            {
                TileType newTile = TileType.NONE;
                switch (state)
                {
                    case InterfaceState.REMOVE:
                        newTile = TileType.NONE;
                        break;
                    case InterfaceState.COMM:
                        newTile = TileType.COMM;
                        break;
                    case InterfaceState.RES:
                        newTile = TileType.RES;
                        break;
                }
                var pos = screenToWorld(mouse.Position);
                if (pos != null)
                {
                    if (dragStart != null)
                    {
                        // fill rect
                        var startpos = screenToWorld(dragStart.Value);

                        var xmin = MathHelper.Min(startpos.Value.X, pos.Value.X);
                        var xmax = MathHelper.Max(startpos.Value.X, pos.Value.X);
                        var ymin = MathHelper.Min(startpos.Value.Y, pos.Value.Y);
                        var ymax = MathHelper.Max(startpos.Value.Y, pos.Value.Y);

                        for (int x = (int)xmin; x <= xmax; ++x)
                        {
                            for (int y = (int)ymin; y <= ymax; ++y)
                            {
                                if (world.grid[x, y].type == TileType.NONE)
                                    world.grid[x, y].type = newTile;
                            }
                        }
                    }
                    else
                    {
                        // fill single tile

                        world.grid[(int)pos?.X, (int)pos?.Y].type = newTile;
                    }
                }
                dragStart = null;
            }

            if (mouse.LeftButton == ButtonState.Pressed && (state == InterfaceState.ROAD || prevMS.LeftButton == ButtonState.Released))
            {
                // get interface click
                if (mouse.Position.X < InterfaceElement.width)
                {
                    int idx = mouse.Position.Y / InterfaceElement.height;
                    if (idx < interfaceElements.Count && idx >= 0)
                    {
                        InterfaceElement selected = interfaceElements[idx];
                        selected.action?.Invoke();
                    }
                }

                var pos = screenToWorld(Mouse.GetState().Position);
                if (pos != null)
                {
                    switch (state)
                    {
                        case InterfaceState.REMOVE:
                            world.removeTile((int)pos?.X, (int)pos?.Y);
                            break;
                        case InterfaceState.ROAD:
                            world.grid[(int)pos?.X, (int)pos?.Y].type = TileType.ROAD;
                            break;
                    }

                }
            }

            // touch interaction
            foreach (var touch in TouchPanel.GetState())
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
            if (Keyboard.GetState().IsKeyDown(Keys.Space) && prevKB.IsKeyUp(Keys.Space)) running ^= true; // toggle simulation state

            prevMS = Mouse.GetState();
            prevKB = Keyboard.GetState();
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(176, 144, 112));

            spriteBatch.Begin(blendState: BlendState.AlphaBlend);
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
                    Vector2 a = camOffset + new Vector2(x, y) * world.tilesize;
                    Color newCol = Color.Transparent;
                    switch (world.grid[x, y].type)
                    {
                        case TileType.ROAD:
                            newCol = Color.LightGray;
                            break;
                        case TileType.COMM:
                            newCol = Color.Blue;
                            break;
                        case TileType.RES:
                            newCol = Color.Green;
                            break;
                    }
                    if (world.grid[x, y].type == TileType.ROAD)
                    {
                        string tile = "road_" + adjDir(x, y, TileType.ROAD);
                        spriteBatch.Draw(sprites[tile], a, Color.White);
                        if (world.grid[x, y].value > 0)
                        {
                            if (tile == "road_NS")
                                spriteBatch.Draw(sprites["cars_ver_" + timeStep % 2], a, Color.White);
                            if (tile == "road_EW")
                                spriteBatch.Draw(sprites["cars_hor_" + timeStep % 2], a, Color.White);
                        }
                    }
                    else
                    {
                        if (world.grid[x, y].value > 0)
                        {
                            var step = Tile.maxVal / 5;
                            var lvl = world.grid[x, y].value / step + 1;
                            if (lvl > 5) lvl = 5;

                            if (world.grid[x, y].type == TileType.RES)
                                spriteBatch.Draw(texture: sprites["res_" + lvl], position: a, scale: new Vector2((float)world.tilesize / 24.0f), color: Color.White, origin: new Vector2(0, 8));

                            else if (world.grid[x, y].type == TileType.COMM)
                                spriteBatch.Draw(texture: sprites["comm_" + lvl], position: a, scale: new Vector2((float)world.tilesize / 24.0f), color: Color.White, origin: new Vector2(0, 8));

                            else
                                spriteBatch.DrawString(font, world.grid[x, y].value.ToString(), a, Color.White);
                        }
                        else
                            GeometryDrawer.fillRect(a.ToPoint(), world.tilesize, world.tilesize, newCol);
                    }
                }
            }

            // draw interface
            GeometryDrawer.fillRect(Point.Zero, InterfaceElement.width, windowHeight, Color.Gray);
            Vector2 pos = Vector2.Zero;
            for (int i = 0; i < interfaceElements.Count; ++i)
            {
                if (interfaceElements[i].name != "")
                    spriteBatch.Draw(sprites[interfaceElements[i].name], pos, Color.White);
                //spriteBatch.DrawString(font, interfaceEls[i].name, pos + Vector2.UnitY * interfaceWidth / 2.0f, Color.White);
                pos.Y += InterfaceElement.height;
                //GeometryDrawer.drawLine(pos, pos + Vector2.UnitX * (interfaceWidth+10), Color.Red);
            }

            // draw cursor/selector
            if (dragStart != null && (state == InterfaceState.COMM || state == InterfaceState.RES))
            {
                var xmin = MathHelper.Min(dragStart.Value.X, Mouse.GetState().Position.X);
                var xmax = MathHelper.Max(dragStart.Value.X, Mouse.GetState().Position.X);
                var ymin = MathHelper.Min(dragStart.Value.Y, Mouse.GetState().Position.Y);
                var ymax = MathHelper.Max(dragStart.Value.Y, Mouse.GetState().Position.Y);
                GeometryDrawer.fillRect(new Rectangle(xmin, ymin, xmax - xmin, ymax - ymin), Color.White * 0.5f);
            }

            // draw debug output
            spriteBatch.DrawString(font, timeStep.ToString(), Vector2.One, Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        string adjDir(int x, int y, TileType type)
        {
            string ret = "";
            if (y > 0 && world.grid[x, y - 1].type == type) ret += "N";
            if (x < world.size - 1 && world.grid[x + 1, y].type == type) ret += "E";
            if (y < world.size - 1 && world.grid[x, y + 1].type == type) ret += "S";
            if (x > 0 && world.grid[x - 1, y].type == type) ret += "W";

            if (ret == "" || ret == "N" || ret == "S") ret = "NS";
            if (ret == "E" || ret == "W") ret = "EW";
            return ret;
        }

        Vector2? screenToWorld(Point screenPos)
        {
            if (screenPos.X < InterfaceElement.width) return null;
            int x = (screenPos.X - (int)camOffset.X) / world.tilesize;
            int y = (screenPos.Y - (int)camOffset.Y) / world.tilesize;
            if (x >= 0 && x < world.size && y >= 0 && y < world.size)
                return new Vector2(x, y);
            else return null;
        }
    }
}
