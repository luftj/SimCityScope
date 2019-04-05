using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimCityScope
{
    delegate void InterfaceDelegate();

    class InterfaceElement
    {
        public static int width = 50;
        public static int height = 50;
        public string name { get; }
        public InterfaceDelegate action;

        public InterfaceElement(string name, InterfaceDelegate action)
        {
            this.name = name;
            this.action = action;
        }
    }

    enum InterfaceState
    {
        REMOVE,
        ROAD,
        COMM,
        RES,

        NUM_INTERFACESTATE,
        NONE =0
    }

    class Interface
    {
        public List<InterfaceElement> interfaceElements;
        public InterfaceState state;
        Game1 game;

        public Interface(Game1 game)
        {
            this.game=game;
            interfaceElements = new List<InterfaceElement>();
            interfaceElements.Add(new InterfaceElement("", null));
            interfaceElements.Add(new InterfaceElement("remove", delegate () { state = InterfaceState.REMOVE; }));
            interfaceElements.Add(new InterfaceElement("road", delegate () { state = InterfaceState.ROAD; }));
            interfaceElements.Add(new InterfaceElement("commercial", delegate () { state = InterfaceState.COMM; }));
            interfaceElements.Add(new InterfaceElement("residential", delegate () { state = InterfaceState.RES; }));
        }

        public void selectInterface(Point pos)
        {
            if (pos.X < InterfaceElement.width)
            {
                int idx = pos.Y / InterfaceElement.height;
                if (idx < interfaceElements.Count && idx >= 0)
                {
                    InterfaceElement selected = interfaceElements[idx];
                    selected.action?.Invoke();  // change interface state
                }
            }
        }

        public void Draw(GameTime gameTime)
        {
            // draw interface
            GeometryDrawer.fillRect(Point.Zero, InterfaceElement.width, game.windowHeight, Color.Gray);
            Vector2 pos = Vector2.Zero;
            for (int i = 0; i < interfaceElements.Count; ++i)
            {
                if (interfaceElements[i].name != "")
                    game.spriteBatch.Draw(game.sprites[interfaceElements[i].name], pos, Color.White);
                //spriteBatch.DrawString(font, interfaceEls[i].name, pos + Vector2.UnitY * interfaceWidth / 2.0f, Color.White);
                pos.Y += InterfaceElement.height;
                //GeometryDrawer.drawLine(pos, pos + Vector2.UnitX * (interfaceWidth+10), Color.Red);
            }
        }
    }
}
