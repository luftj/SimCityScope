using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimCityScope
{
    public static class GeometryDrawer
    {
        static GraphicsDevice graphicsDevice;
        static SpriteBatch spriteBatch;
        static Texture2D dummyTexture;


        static BasicEffect basicEffect;


        public static void init(Game1 game)//GraphicsDevice gd)
        {
            //graphicsDevice = gd;
            //spriteBatch = new SpriteBatch(gd);
            graphicsDevice = game.GraphicsDevice;
            spriteBatch = game.spriteBatch;

            dummyTexture = new Texture2D(graphicsDevice, 1, 1);
            dummyTexture.SetData(new Color[] { Color.White });


            basicEffect = new BasicEffect(graphicsDevice);
            basicEffect.VertexColorEnabled = true;
            basicEffect.Projection = Matrix.CreateOrthographicOffCenter
                (0, graphicsDevice.Viewport.Width,     // left, right
                 graphicsDevice.Viewport.Height, 0,    // bottom, top
                0, 1);                    // near, far plane


            graphicsDevice.RasterizerState = RasterizerState.CullNone;
        }

        public static void setViewPort(Viewport viewport)
        {
            basicEffect.Projection = Matrix.CreateOrthographicOffCenter
                (viewport.X, viewport.X + viewport.Width * 3 / 2,     // left, right  // todo: ololol, hacky!
                 viewport.Y + viewport.Height, viewport.Y,    // bottom, top
                 0, 1);                 // near, far plane
        }

        public static void fillRect(int x, int y, int width, int height, Color colour) { fillRect(new Rectangle(x, y, width, height), colour); }
        public static void fillRect(Point pos, int width, int height, Color colour) { fillRect(new Rectangle((int)pos.X, (int)pos.Y, width, height), colour); }
        public static void fillRect(Rectangle rect) { fillRect(rect, Color.White); }
        public static void fillRect(Rectangle rect, Color colour)
        {
            spriteBatch.Draw(dummyTexture, new Rectangle(rect.X, rect.Y, rect.Width, rect.Height), colour);
        }

        public static void drawLine(Vector2 A, Vector2 B, Color colour)
        {
            // todo: implement line styles (thickness, ...)

            Vector3 a = new Vector3(A, 0);
            Vector3 b = new Vector3(B, 0);
            VertexPositionColor[] vertices = { new VertexPositionColor(a, colour),
                                               new VertexPositionColor(b, colour) };

            basicEffect.CurrentTechnique.Passes[0].Apply();
            graphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, vertices, 0, 1);
        }

        public static void drawLineWidth(Vector2 A, Vector2 B, float width, Color colour)
        {
            Vector2 perpendicular = B - A;
            var tmp = perpendicular.Y;
            perpendicular.Y = -perpendicular.X; // right-hand perpendicular
            perpendicular.X = tmp;

            perpendicular.Normalize();
            perpendicular *= width/2f;  // orthogonal offset

            List<Point> vertices = new List<Point>();
            vertices.Add((A + perpendicular).ToPoint());    // fill ACW
            vertices.Add((A - perpendicular).ToPoint());
            vertices.Add((B - perpendicular).ToPoint());
            vertices.Add((B + perpendicular).ToPoint());
            fillPoly(vertices,colour);
        }

        public static void fillTriangleGradient(Vector2 A, Vector2 B, Vector2 C, Color cA, Color cB, Color cC)
        {
            VertexPositionColor[] vertices = new VertexPositionColor[3];

            vertices[0] = new VertexPositionColor(new Vector3(A, 0), cA);
            float det = (B.X - A.X) * -(C.Y - A.Y) - (C.X - A.X) * -(B.Y - A.Y);

            vertices[det < 0 ? 1 : 2] = new VertexPositionColor(new Vector3(B, 0), cB);
            vertices[det > 0 ? 1 : 2] = new VertexPositionColor(new Vector3(C, 0), cC);

            //graphicsDevice.RasterizerState = RasterizerState.CullNone;
            basicEffect.CurrentTechnique.Passes[0].Apply();
            graphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, vertices, 0, 1);
        }

        public static void fillPolyGradient(Vector2 Center, List<Point> poly, Color ccolour, Color[] pcolour)
        {
            if (poly.Count < 3)
                return;

            /*int iterator = 1;
            while (iterator < poly.Count)
            {
                fillTriangleGradient(poly[iterator - 1].toVector2(),Center, poly[iterator].toVector2(), ccolour, pcolour[iterator - 1], pcolour[iterator]);
                ++iterator;
            }
            fillTriangleGradient(poly[poly.Count - 1].toVector2(),Center, poly[0].toVector2(), ccolour, pcolour[poly.Count - 1], pcolour[0]);*/
            Vector2 center = Center;

            List<VertexPositionColor> vertices = new List<VertexPositionColor>();

            int iterator = 1;
            while (iterator < poly.Count)
            {
                vertices.Add(new VertexPositionColor(new Vector3(new Vector2(poly[iterator - 1].X, poly[iterator - 1].Y), 0), pcolour[iterator - 1]));
                vertices.Add(new VertexPositionColor(new Vector3(center, 0), ccolour));
                vertices.Add(new VertexPositionColor(new Vector3(new Vector2(poly[iterator].X, poly[iterator].Y), 0), pcolour[iterator]));


                ++iterator;
            }
            vertices.Add(new VertexPositionColor(new Vector3(new Vector2(poly[poly.Count - 1].X, poly[poly.Count - 1].Y), 0), pcolour[poly.Count - 1]));
            vertices.Add(new VertexPositionColor(new Vector3(center, 0), ccolour));
            vertices.Add(new VertexPositionColor(new Vector3(new Vector2(poly[0].X, poly[0].Y), 0), pcolour[0]));


            graphicsDevice.RasterizerState = RasterizerState.CullNone;
            basicEffect.CurrentTechnique.Passes[0].Apply();
            graphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, vertices.ToArray(), 0, poly.Count);
        }

        public static void fillPoly(List<Point> poly, Color colour)
        {
            if (poly.Count < 3)
                return;

            //Vector2 center = Center * cameraZoom - cameraPosition;

            /*VertexPositionColor[] vertices = new VertexPositionColor[poly.Count * 3];
            for (int i = 1; i < poly.Count; ++i)
            {
                vertices[i * 3 - 3] = (new VertexPositionColor(new Vector3(center, 0), colour));
                vertices[i * 3 - 2] = (new VertexPositionColor(new Vector3(poly[i].toVector2() * cameraZoom - cameraPosition, 0.0f), colour));
                vertices[i * 3 - 1] = (new VertexPositionColor(new Vector3(poly[i - 1].toVector2() * cameraZoom - cameraPosition, 0.0f), colour));
            }
            vertices[vertices.Length - 3] = (new VertexPositionColor(new Vector3(center, 0), colour));
            vertices[vertices.Length - 2] = (new VertexPositionColor(new Vector3(poly[0].toVector2() * cameraZoom - cameraPosition, 0.0f), colour));
            vertices[vertices.Length - 1] = (new VertexPositionColor(new Vector3(poly[poly.Count - 1].toVector2() * cameraZoom - cameraPosition, 0.0f), colour));

            basicEffect.CurrentTechnique.Passes[0].Apply();
            graphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, vertices, 0, poly.Count);*/

            List<VertexPositionColor> vertices = new List<VertexPositionColor>();

            /*vertices.Add(new VertexPositionColor(new Vector3(poly[0].toVector2() * cameraZoom - cameraPosition, 0), colour));
            vertices.Add(new VertexPositionColor(new Vector3(center, 0), colour));
            vertices.Add(new VertexPositionColor(new Vector3(poly[1].toVector2() * cameraZoom - cameraPosition, 0), colour));*/

            int iterator = 0;
            int index;
            while (iterator < poly.Count)
            {
                if (iterator % 2 == 0)
                    index = iterator / 2;
                else
                    index = poly.Count - 1 - iterator / 2;
                vertices.Add(new VertexPositionColor(new Vector3(new Vector2(poly[index].X, poly[index].Y), 0), colour));

                //if(iterator % 2 == 0)
                //vertices.Add(new VertexPositionColor(new Vector3(center, 0), colour));
                /*if (iterator % 2 == 0)
                    vertices.Add(new VertexPositionColor(new Vector3(center, 0), colour));
                else
                    vertices.Add(new VertexPositionColor(new Vector3(poly[iterator].toVector2() * cameraZoom - cameraPosition, 0), colour));*/
                ++iterator;
            }
            /*if (iterator % 2 == 0)
                vertices.Add(new VertexPositionColor(new Vector3(center, 0), colour));
            vertices.Add(new VertexPositionColor(new Vector3(poly[0].toVector2() * cameraZoom - cameraPosition, 0), colour));*/

            /* for( i = 0; i < vertices.count; ++i )
            {
                if( i % 2 == 0 )
                    vertex = i / 2;
                else
                    vertex = vertices.count - 1 - i / 2;
                Push( vertices[ vertex ] );
            }
            */

            //graphicsDevice.RasterizerState = RasterizerState.CullNone;
            basicEffect.CurrentTechnique.Passes[0].Apply();
            graphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleStrip, vertices.ToArray(), 0, vertices.Count - 2);//(vertices.Count-1)/2);
        }
    }
}
