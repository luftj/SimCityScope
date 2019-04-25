using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BAMCIS.GeoJSON;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace SimCityScope
{
    class GEOhandler
    {
        string filepath = @"SiedlungsflaechenGrasbrook.json";
        
        Vector2 offset = new Vector2(565700, 5932400);
        float mapzoom = 1.5f;
        
        public List<List<Vector2>> makePolys()
        {
            var polys = new List<List<Vector2>>();
            string text;
            var fileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
            {
                text = streamReader.ReadToEnd();
            }
            GeoJson data = JsonConvert.DeserializeObject<GeoJson>(text);
            FeatureCollection Col = FeatureCollection.FromJson(text);
            foreach (var feature in Col.Features)
            {
                var p = feature.Geometry;

                List<Polygon> c = new List<Polygon>((p as MultiPolygon).Coordinates);
                foreach (var poly in c)
                {
                    List<Vector2> polypoints = new List<Vector2>();
                    foreach (var x in poly.Coordinates)
                    {
                        foreach (var pos in x.Coordinates)
                        {
                            System.Console.WriteLine(pos);
                            var v = (new Vector2((float)pos.Longitude, (float)pos.Latitude) - offset) / mapzoom;
                            v.Y *= -1.0f;
                            polypoints.Add(v);
                        }
                    }
                    polys.Add(polypoints);
                }
            }
            return polys;
        }
    }
}
