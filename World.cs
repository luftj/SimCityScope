using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimCityScope
{
    enum TileType
    {
        NONE= 0,
        ROAD,
        COMM,
        RES,
    }

    struct Tile
    {
        public bool active;
        public TileType type;
    }

    class World
    {
        public int size = 20;

        public int tilesize = 30;

        public Tile[,] grid;

        public World(int size)
        {
            grid = new Tile[size, size];
            this.size = size;
            
        }
    }
}
