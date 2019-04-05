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
        public TileType type;

        public int value;

        public static int maxVal { get; } = 30;
    }

    class World
    {
        public int size = 20;   // number of tiles in each dimension
        public int tilesize = 30;   // width/height of tiles in px

        public Tile[,] grid;    // the map

        public int population { get; private set; } = 0;
        public int jobs { get; private set; } = 10;
        public int jobDemand { get; private set; }
        int ecoGrowth = 2;
        int maxGrowthPerStep = 5;

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
                growTile(TileType.RES, MathHelper.Min(vacancies,maxGrowthPerStep));
            else if (vacancies < 0)
                jobDemand = growTile(TileType.COMM, MathHelper.Min(-vacancies, maxGrowthPerStep));

            // update traffic
            List<Point> relevantTiles = new List<Point>();
            for (var x = 1; x < size-1; ++x)
                for (var y = 1; y < size-1; ++y)
                {
                    if (grid[x, y].type == TileType.ROAD)    // pick relevant type
                    {
                        if (grid[x - 1, y - 1].value > 0) grid[x, y].value = 1;
                        if (grid[x + 1, y - 1].value > 0) grid[x, y].value = 1;
                        if (grid[x - 1, y + 1].value > 0) grid[x, y].value = 1;
                        if (grid[x + 1, y + 1].value > 0) grid[x, y].value = 1;
                    }
                }
        }

        public int growTile(TileType type, int amount)
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
                var cur = relevantTiles[rng.Next(relevantTiles.Count())];

                // grow
                grid[cur.X, cur.Y].value++;

                if (type == TileType.COMM) jobs++;
                if (type == TileType.RES) population++;

                --amountleft;
            }
            return amountleft;
        }

        public void removeTile(int x, int y)
        {
            if (grid[x, y].type == TileType.RES)
                population -= grid[x, y].value;
            if (grid[x, y].type == TileType.COMM)
                jobs -= grid[x, y].value;
            grid[x, y].value = 0;
            grid[x, y].type = TileType.NONE;
        }

        bool checkVicinity(Point pos, TileType check, int range)
        {
            // todo: proper manhattan distance
            for (var x = MathHelper.Max(pos.X - range, 0); x < MathHelper.Min(pos.X + range +1, size); ++x)
                for (var y = MathHelper.Max(pos.Y - range, 0); y < MathHelper.Min(pos.Y + range+1, size); ++y)
                {
                    if (grid[x, y].type == check) return true;
                }
            return false;
        }
    }
}
