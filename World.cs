using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimCityScope
{
    enum TileType
    {
        NONE = 0,
        ROAD,
        COMM,
        RES,
    }

    struct Tile
    {
        public bool active;
        public TileType type;

        public int value;

        public static int maxVal = 30;
    }

    class World
    {
        public int size = 20;   // number of tiles in each dimension
        public int tilesize = 30;   // width/height of tiles in px

        public Tile[,] grid;    // the map

        public int population { get; private set; } = 0;
        public int jobs { get; private set; } = 10;
        int ecoGrowth = 2;

        public World(int size)
        {
            grid = new Tile[size, size];
            this.size = size;

        }

        public void updateStep()
        {
            // update totals
            growTile(TileType.COMM, ecoGrowth);

            // growth scenarios
            var vacancies = jobs - population;  // todo: either one has to come from somewhere...
            if (vacancies > 0)
                growTile(TileType.RES, vacancies);
            else if (vacancies < 0)
                growTile(TileType.COMM, -vacancies);
            else
                return;
        }

        public void growTile(TileType type, int amount)
        {
            // find tiles of this type
            List<Point> relevantTiles = new List<Point>();
            for (var x = 0; x < size; ++x)
                for (var y = 0; y < size; ++y)
                {
                    if (grid[x, y].type == type)    // pick relevant type
                    {
                        var p = new Point(x, y);
                        // check tiles for validity/quality
                        if (grid[x, y].value >= Tile.maxVal) continue; // max size reached
                        if (checkVicinity(p, TileType.ROAD, 3))  // only when roads in range
                        {
                            relevantTiles.Add(p);
                        }
                    }
                }

            int amountleft = amount;
            Random rng = new Random();

            while (relevantTiles.Count > 0 && amountleft > 0)
            {
                // pick at random
                var cur = relevantTiles[rng.Next(relevantTiles.Count() - 1)];

                // grow
                grid[cur.X, cur.Y].value++;

                if (type == TileType.COMM) jobs++;
                if (type == TileType.RES) population++;

                --amountleft;
            }
        }

        bool checkVicinity(Point pos, TileType check, int range)
        {
            for (var x = MathHelper.Max(pos.X - range, 0); x < MathHelper.Min(pos.X + range, size); ++x)
                for (var y = MathHelper.Max(pos.Y - range, 0); y < MathHelper.Min(pos.Y + range, size); ++y)
                {
                    if (grid[x, y].type == check) return true;
                }
            return false;
        }
    }
}
