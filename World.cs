using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimCityScope
{
    public enum TileType
    {
        NONE = 0,
        ROAD,
        COMM,
        RES,
    }

    public struct Tile
    {
        public TileType type;

        public int value;

        public static int maxVal { get; } = 30;
    }

    public class World
    {
        public int size;   // number of tiles in each dimension
        public int tilesize = 30;   // width/height of tiles in px

        public Tile[,] grid;    // the map

        public int population { get; private set; } = 0;
        public int jobs { get; private set; } = 10;
        public int jobDemand { get; private set; }
        int ecoGrowth = 2;
        int maxGrowthPerStep = 5;

        public Dictionary<string, int> costs;
        public int bankAccount;

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
                growTile(TileType.RES, MathHelper.Min(vacancies, maxGrowthPerStep));
            else if (vacancies < 0)
                jobDemand = growTile(TileType.COMM, MathHelper.Min(-vacancies, maxGrowthPerStep));

            // update traffic
            List<Point> relevantTiles = new List<Point>();
            for (var x = 1; x < size - 1; ++x)
                for (var y = 1; y < size - 1; ++y)
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

        public void setTile(int x, int y, TileType type)
        {
            if (grid[x, y].type == TileType.NONE || (type == TileType.ROAD && grid[x, y].value == 0 && grid[x,y].type != type))
            {
                grid[x, y].type = type;

                switch (type)
                {
                    case TileType.COMM:
                    case TileType.RES:
                        bankAccount -= costs["cost_zoning"];
                        break;
                    case TileType.ROAD:
                        bankAccount -= costs["cost_road"];
                        break;
                }
            }
        }

        public void removeTile(int x, int y)
        {
            int cost = 0;
            if (grid[x, y].type == TileType.RES)
            {
                population -= grid[x, y].value;
                cost = (grid[x, y].value > 0) ? costs["cost_remove_building"] : costs["cost_remove_zone"];
            }
            else if (grid[x, y].type == TileType.COMM)
            {
                jobs -= grid[x, y].value;
                cost = (grid[x, y].value > 0) ? costs["cost_remove_building"] : costs["cost_remove_zone"];
            }
            else if(grid[x, y].type == TileType.ROAD)
                cost = costs["cost_remove_road"];
            grid[x, y].value = 0;
            grid[x, y].type = TileType.NONE;
            bankAccount -= cost;
        }

        bool checkVicinity(Point pos, TileType check, int range)
        {
            // todo: proper manhattan distance
            for (var x = MathHelper.Max(pos.X - range, 0); x < MathHelper.Min(pos.X + range + 1, size); ++x)
                for (var y = MathHelper.Max(pos.Y - range, 0); y < MathHelper.Min(pos.Y + range + 1, size); ++y)
                {
                    if (grid[x, y].type == check) return true;
                }
            return false;
        }

        public void eraseAll()
        {
            grid = new Tile[size, size];
            population = 0;
            jobs = 10;
        }
    }
}
