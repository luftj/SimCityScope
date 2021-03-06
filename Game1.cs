﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SimCityScope
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        #region STATE
        public World world;
        #endregion

        #region TIME
        bool running = true;
        int timeStep = 0; // total elapsed time steps
        float speed = 0.5f; // seconds per time step
        double stepTimer = 0.0f;
        #endregion

        #region VIEW
        GraphicsDeviceManager graphics;
        public SpriteBatch spriteBatch;

        public Dictionary<string, Texture2D> sprites { get; private set; }
        public SpriteFont font;
        public int windowWidth { get; private set; } = 1000;
        public int windowHeight { get; private set; } = 1000;
        #endregion

        #region CONROL
        KeyboardState prevKB;
        MouseState prevMS;
        Vector2 camOffset;
        Interface UI;
        Point? dragStart = null;

        MainMenu menu;
        #endregion

        string debugtext = "";
        string debugtextpersist = "";

        List<List<Vector2>> polys;        

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = windowWidth;
            graphics.PreferredBackBufferHeight = windowHeight;
            Mouse.WindowHandle = Window.Handle;
            
            Content.RootDirectory = "Content";

            sprites = new Dictionary<string, Texture2D>();


            polys = new List<List<Vector2>>();
        }

        void toggleFullscreen()
        {
            if(graphics.IsFullScreen)
            {
                Window.IsBorderless = false;
                windowWidth = 800;
                windowHeight = 600;
            }
            else
            {
                Window.IsBorderless = true; // otherwise mouse pos can be 20px (titlebar) off!
                windowWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                windowHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            }

            graphics.PreferredBackBufferWidth = windowWidth;
            graphics.PreferredBackBufferHeight = windowHeight;
            graphics.ToggleFullScreen();
            graphics.ApplyChanges();
            GeometryDrawer.init(this);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = windowWidth;
            graphics.PreferredBackBufferHeight = windowHeight;
            graphics.ApplyChanges();

            this.IsMouseVisible = true;

            TouchPanel.EnabledGestures = GestureType.FreeDrag | GestureType.Tap | GestureType.Pinch | GestureType.DragComplete | GestureType.Flick;

            world = new World(100);
            world.bankAccount = 2000;

            camOffset = new Vector2(-190, -120);
            UI = new Interface(this);

            menu = new MainMenu(this);
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

            #region contentmanager
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
            #endregion


            world.costs = XmlHandler.getCosts();

            polys = new GEOhandler().makePolys();
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
        /// Performs zoning (setting TileType) under a rectangular area.
        /// </summary>
        /// <param name="A">first corner of box selection in screen coordinates</param>
        /// <param name="B">second corner</param>
        /// <param name="type"></param>
        void boxZone(Point A, Point B, TileType type)
        {
            var startpos = screenToWorld(A);

            var xmin = MathHelper.Min(startpos.Value.X, B.X);
            var xmax = MathHelper.Max(startpos.Value.X, B.X);
            var ymin = MathHelper.Min(startpos.Value.Y, B.Y);
            var ymax = MathHelper.Max(startpos.Value.Y, B.Y);

            for (int x = (int)xmin; x <= xmax; ++x)
            {
                for (int y = (int)ymin; y <= ymax; ++y)
                {
                    //if (world.grid[x, y].type == TileType.NONE)
                    //    world.grid[x, y].type = type;
                    world.setTile(x, y, type);
                }
            }
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


            menu.Update(gameTime);

            debugtext = "";

            debugtext += Window.ClientBounds + "\n";

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
            
            TileType newTile = TileType.NONE;
            switch (UI.state)
            {
                case InterfaceState.REMOVE: newTile = TileType.NONE; break;
                case InterfaceState.COMM:   newTile = TileType.COMM; break;
                case InterfaceState.RES:    newTile = TileType.RES; break;
                case InterfaceState.ROAD:   newTile = TileType.ROAD; break;
            }
            debugtext += "tilestate: " + newTile.ToString() +"\n";

            #region touch_input
            // touch interaction
            // get any gestures that are ready.
            bool dragHandled = false;
            bool wantflick = false;
            while (TouchPanel.IsGestureAvailable)
            {
                GestureSample gs = TouchPanel.ReadGesture();
                var pos = screenToWorld(gs.Position.ToPoint());
                var delta = gs.Delta;
                switch (gs.GestureType)
                {
                    case GestureType.Tap:
                        // single tap
                        // get interface click
                        UI.selectInterface(gs.Position.ToPoint());

                        if (pos == null) break;
                        //world.grid[(int)pos.Value.X, (int)pos.Value.Y].type = newTile;
                        world.setTile((int)pos.Value.X, (int)pos.Value.Y, newTile);
                        debugtext += "tap\n";
                        break;
                    case GestureType.DragComplete:
                        // move the poem screen vertically by the drag delta
                        // amount.
                        if (pos == null) break;
                        if (UI.state == InterfaceState.ROAD || UI.state == InterfaceState.REMOVE) break;
                        boxZone(pos.Value.ToPoint(), (pos.Value.ToPoint() - delta.ToPoint()), newTile);
                        debugtext += "dragcomplete\n";
                        break;

                    case GestureType.Pinch:
                        // add velocity to the poem screen (only interested in
                        // changes to Y velocity).
                        var delta2 = gs.Delta2;
                        if (delta2 == null) break;
                        var move = (delta + delta2) / 2;  // average of 2-finger-drag
                        camOffset += move;
                        debugtext += "pinch\n";
                        dragHandled = true;
                        break;
                    case GestureType.FreeDrag:
                        // happens multiple times _during_ a drag. 
                        // use for roads + removal
                        if (pos == null) break;
                        if (UI.state == InterfaceState.ROAD)
                        {
                            //world.grid[(int)pos.Value.X, (int)pos.Value.Y].type = newTile;
                            world.setTile((int)pos.Value.X, (int)pos.Value.Y, newTile);

                        }
                        else if (UI.state == InterfaceState.REMOVE)
                        {
                            world.removeTile((int)pos.Value.X, (int)pos.Value.Y);
                        }
                        break;
                    case GestureType.Flick:
                        wantflick = true;
                        var delta22 = gs.Delta2;
                        debugtextpersist = "flick: " + delta + ", " + delta22;
                        break;
                    default:
                        // other gesture happened
                        debugtext += "Unhandled gesture: " + gs.GestureType.ToString() + "\n";
                        break;
                }
            }

            if (!dragHandled && wantflick)
            {
                menu.startSlide();
            }
            #endregion

            #region mouse_input
            // mouse interaction
            MouseState mouse = Mouse.GetState();
            debugtext += mouse.Position.ToString() + "\n";
            // mouse drag ended
            if (mouse.LeftButton == ButtonState.Released && prevMS.LeftButton == ButtonState.Pressed)
            {
                var pos = screenToWorld(mouse.Position);
                if (pos != null)
                {
                    if (dragStart != null && UI.state != InterfaceState.ROAD) // fill rect
                        boxZone(dragStart.Value, pos.Value.ToPoint(), newTile);
                    else // fill single tile
                        if(!menu.mouseInMenu())
                        world.setTile((int)pos.Value.X, (int)pos.Value.Y, newTile);
                        //world.grid[(int)pos?.X, (int)pos?.Y].type = newTile;
                }
                dragStart = null;
            }

            if (mouse.LeftButton == ButtonState.Pressed && !menu.mouseInMenu())
            {
                var pos = screenToWorld(Mouse.GetState().Position);

                if ( UI.state == InterfaceState.ROAD || UI.state == InterfaceState.REMOVE)
                    if (pos != null)
                    {
                        switch (UI.state)
                        {
                            case InterfaceState.REMOVE:
                                world.removeTile((int)pos?.X, (int)pos?.Y);
                                break;
                            case InterfaceState.ROAD:
                                //world.grid[(int)pos?.X, (int)pos?.Y].type = TileType.ROAD;
                                world.setTile((int)pos.Value.X, (int)pos.Value.Y, TileType.ROAD);
                                break;
                        }

                    }
                
                if (prevMS.LeftButton == ButtonState.Released)
                {
                    // get interface click
                    UI.selectInterface(mouse.Position);

                    // mouse drag
                    if (pos != null)
                        dragStart = mouse.Position;
                    else
                        dragStart = null;
                }
            }

            prevMS = Mouse.GetState();
            #endregion
            
            #region keyboard_input
            // move view
            if (Keyboard.GetState().IsKeyDown(Keys.W)) camOffset.Y++;
            if (Keyboard.GetState().IsKeyDown(Keys.A)) camOffset.X++;
            if (Keyboard.GetState().IsKeyDown(Keys.S)) camOffset.Y--;
            if (Keyboard.GetState().IsKeyDown(Keys.D)) camOffset.X--;
            if (Keyboard.GetState().IsKeyDown(Keys.Space) && prevKB.IsKeyUp(Keys.Space)) running ^= true; // toggle simulation state
            if (Keyboard.GetState().IsKeyDown(Keys.M) && prevKB.IsKeyUp(Keys.M)) toggleFullscreen(); // toggle simulation state
            if (Keyboard.GetState().IsKeyDown(Keys.Enter) && prevKB.IsKeyUp(Keys.Enter)) menu.startSlide(); // toggle simulation state
            prevKB = Keyboard.GetState();
            #endregion

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
                GeometryDrawer.drawLine(camOffset + new Vector2(x * world.tilesize+1, 0), camOffset + new Vector2(x, world.size) * world.tilesize + Vector2.UnitX, Color.Black);   // vertical lines
                GeometryDrawer.drawLine(camOffset + new Vector2(0, x * world.tilesize+1), camOffset + new Vector2(world.size, x) * world.tilesize+Vector2.UnitY, Color.Black);   // horizontal lines
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
            for (int i = 0; i < UI.interfaceElements.Count; ++i)
            {
                if (UI.interfaceElements[i].name != "")
                    spriteBatch.Draw(sprites[UI.interfaceElements[i].name], pos, Color.White);
                //spriteBatch.DrawString(font, interfaceEls[i].name, pos + Vector2.UnitY * interfaceWidth / 2.0f, Color.White);
                pos.Y += InterfaceElement.height;
                //GeometryDrawer.drawLine(pos, pos + Vector2.UnitX * (interfaceWidth+10), Color.Red);
            }

            // todo: draw stats, info, plots (passive UI)
            // treasury
            spriteBatch.DrawString(font, world.bankAccount + "$", new Vector2(10,10), Color.White);
            // draw residential-commercial demand chart
            pos.Y += 20;
            pos.X += InterfaceElement.width / 2;
            var vacancies = world.jobs - world.population;
            GeometryDrawer.fillRect(new Rectangle(pos.ToPoint(), new Point(vacancies, 10)), Color.Green); // draw res demand
            spriteBatch.DrawString(font, "R", pos, Color.White);
            pos.Y += 20;
            GeometryDrawer.fillRect(new Rectangle(new Point((int)pos.X - vacancies, (int)pos.Y), new Point(-vacancies, 10)), Color.Blue); // draw comemrcial demand
            spriteBatch.DrawString(font, "C", pos, Color.White);


            // draw cursor/selector
            if (dragStart != null && (UI.state == InterfaceState.COMM || UI.state == InterfaceState.RES))
            {
                var xmin = MathHelper.Min(dragStart.Value.X, Mouse.GetState().Position.X);
                var xmax = MathHelper.Max(dragStart.Value.X, Mouse.GetState().Position.X);
                var ymin = MathHelper.Min(dragStart.Value.Y, Mouse.GetState().Position.Y);
                var ymax = MathHelper.Max(dragStart.Value.Y, Mouse.GetState().Position.Y);
                GeometryDrawer.fillRect(new Rectangle(xmin, ymin, xmax - xmin, ymax - ymin), Color.White * 0.5f);
            }

            // draw debug output
            spriteBatch.DrawString(font, debugtext, Vector2.UnitY * (windowHeight - font.MeasureString(debugtext).Y), Color.White);
            spriteBatch.DrawString(font, debugtextpersist, new Vector2(400,windowHeight - font.MeasureString(debugtext).Y), Color.White);

            foreach(var poly in polys)
                for (int p = 0; p < poly.Count - 1; ++p)
                    GeometryDrawer.drawLine(worldToScreen(poly[p]),worldToScreen(poly[p + 1]), Color.White);

            menu.Draw(gameTime);
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

        Vector2 worldToScreen(Vector2 worldPos)
        {
            return worldPos + camOffset;
        }
    }
}
