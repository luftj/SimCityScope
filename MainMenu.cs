using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimCityScope
{
    delegate void MenuDelegate();

    struct MenuElement
    {
        public string name { get; }

        public MenuDelegate action { get; }

        public MenuElement(string name, MenuDelegate action)
        {
            this.name = name;
            this.action = action;
        }
    }

    class MainMenu
    {


        int width = 200;
        int height = 100;   // todo: make size depend on elements.count?

        float slidePos;
        int maxSlideSpeed = 100;  // in px/s
        int curSlideSpeed = 0;  // in px/s

        bool alignCenter = true;
        int topMargin = 20;
        int leftMargin = 10;
        int spacing = 30;
        List<MenuElement> elements;

        Game1 game;

        MouseState prevMouse;

        public MainMenu(Game1 game)
        {
            this.game = game;
            slidePos = -height;

            elements = new List<MenuElement>();

            elements.Add(new MenuElement("BACK", startSlide));
        }

        public bool mouseInMenu()
        {
            Rectangle bounds = new Rectangle((game.windowWidth - width) / 2, (int)slidePos,width, height);
            return (bounds.Contains(Mouse.GetState().Position));
        }

        public void startSlide()
        {
            // if sliding down / sitting at bottom
            if (slidePos == 0 || curSlideSpeed > 0)
            {
                // start sliding up
                curSlideSpeed = -maxSlideSpeed;
            }
            // if sliding up / sitting at top
            else if (slidePos == -height || curSlideSpeed < 0)
            {
                // start sliding down
                curSlideSpeed = maxSlideSpeed;
            }

        }

        public void Update(GameTime gameTime)
        {
            slidePos += (float)(curSlideSpeed * gameTime.ElapsedGameTime.TotalSeconds); // move Menu, if sliding

            slidePos = MathHelper.Clamp(slidePos, -height, 0);  // don't slide too far

            if (slidePos == 0 || slidePos == -height) curSlideSpeed = 0;    // stop moving eventually

            var mouse = Mouse.GetState();
            if (mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released)
                selectPosition(mouse.Position);
            prevMouse = mouse;
        }

        void selectPosition(Point pos)
        {
            // find element at pos (mouse or touch)
            int idx = -1;
            Vector2 p = new Vector2((game.windowWidth - width) / 2, (int)slidePos);
            p.X += alignCenter ? width / 2 : leftMargin;
            p.Y += topMargin; // top margin
            for (int i = 0; i < elements.Count; ++i)
            {
                var dims = game.font.MeasureString(elements[i].name);
                Rectangle bounds = new Rectangle((p - (alignCenter ? dims / 2 : Vector2.Zero)).ToPoint(), dims.ToPoint());
                
                if (bounds.Contains(pos))   // found element at pos
                {
                    idx = i;
                    break;
                }
                pos.Y += spacing;
            }

            if (idx == -1) return;  // nothing hit
            elements[idx].action?.Invoke(); // do smth
        }

        public void Draw(GameTime gameTime)
        {
            // draw menu frame
            Point pos = new Point((game.windowWidth - width) / 2, (int)slidePos);
            GeometryDrawer.fillRect(pos, width, height, Color.LightGray);
            // draw menu elements
            pos.Y += topMargin; // top margin
            pos.X += alignCenter ? width / 2 : leftMargin;
            for (int i = 0; i < elements.Count; ++i)
            {
                if (elements[i].name != "")
                    game.spriteBatch.DrawString(game.font, elements[i].name, pos.ToVector2() - (alignCenter ? game.font.MeasureString(elements[i].name) / 2 : Vector2.Zero), Color.White);// factor in element width
                pos.Y += spacing;
            }
        }
    }
}
