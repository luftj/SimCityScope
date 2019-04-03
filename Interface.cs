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
    }
}
