using System;
using System.Collections;
using System.IO;
using System.Text;

using Microsoft.Xna.Framework;

namespace SteeleSky.Voronoi.Mathematics
{
    public abstract class MathTools
    {
        /// <summary>
        /// One static Random instance for use in the entire application
        /// </summary>
        public static readonly Random Rng = new Random((int)DateTime.Now.Ticks);
        public static float Dist(float x1, float y1, float x2, float y2)
        {
            return Vector2.Distance(new Vector2(x1, y1), new Vector2(x2, y2)); ;
        }




        public static int ccw(Vector2 P0, Vector2 P1, Vector2 P2, bool PlusOneOnZeroDegrees)
        {
            int dx1, dx2, dy1, dy2;
            dx1 = (int)(Math.Floor(P1.X - P0.X)); dy1 = (int)(Math.Floor(P1.Y - P0.Y));
            dx2 = (int)(Math.Floor(P2.X - P0.X)); dy2 = (int)(Math.Floor(P2.Y - P0.Y));
            if (dx1 * dy2 > dy1 * dx2) return +1;
            if (dx1 * dy2 < dy1 * dx2) return -1;
            if ((dx1 * dx2 < 0) || (dy1 * dy2 < 0)) return -1;
            if ((dx1 * dx1 + dy1 * dy1) < (dx2 * dx2 + dy2 * dy2) && PlusOneOnZeroDegrees)
                return +1;
            return 0;
        }


        public static bool intersect(Vector2 P11, Vector2 P12, Vector2 P21, Vector2 P22)
        {
            return ccw(P11, P12, P21, true) * ccw(P11, P12, P22, true) <= 0
                && ccw(P21, P22, P11, true) * ccw(P21, P22, P12, true) <= 0;
        }

        public static Vector2 IntersectionPoint(Vector2 P11, Vector2 P12, Vector2 P21, Vector2 P22)
        {

            float Kx = P11.X, Ky = P11.Y, Mx = P21.X, My = P21.Y;
            float Lx = (P12.X - P11.X), Ly = (P12.Y - P11.Y), Nx = (P22.X - P21.X), Ny = (P22.Y - P21.Y);
            float a = float.NaN, b = float.NaN;
            if (Lx == 0)
            {
                if (Nx == 0)
                    throw new Exception("No intersect!");
                b = (Kx - Mx) / Nx;
            }
            else if (Ly == 0)
            {
                if (Ny == 0)
                    throw new Exception("No intersect!");
                b = (Ky - My) / Ny;
            }
            else if (Nx == 0)
            {
                if (Lx == 0)
                    throw new Exception("No intersect!");
                a = (Mx - Kx) / Lx;
            }
            else if (Ny == 0)
            {
                if (Ly == 0)
                    throw new Exception("No intersect!");
                a = (My - Ky) / Ly;
            }
            else
            {
                b = (Ky + Mx * Ly / Lx - Kx * Ly / Lx - My) / (Ny - Nx * Ly / Lx);
            }
            if (!float.IsNaN(a))
            {
                return new Vector2((float)(Kx + a * Lx), (float)(Ky + a * Ly));
            }
            if (!float.IsNaN(b))
            {
                return new Vector2((float)(Mx + b * Nx), (float)(My + b * Ny));
            }
            throw new Exception("Error in IntersectionPoint");
        }
    }
}