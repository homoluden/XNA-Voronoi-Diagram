using System;
using System.Collections;
using System.IO;
using System.Text;

using UnityEngine;

namespace SteeleSky.Voronoi.Mathematics
{
    public abstract class MathTools
    {
        public static float Dist(float x1, float y1, float x2, float y2)
        {
            return Vector2.Distance(new Vector2(x1, y1), new Vector2(x2, y2)); ;
        }




        public static int ccw(Vector2 P0, Vector2 P1, Vector2 P2, bool PlusOneOnZeroDegrees)
        {
            int dx1, dx2, dy1, dy2;
            dx1 = (int)(Math.Floor(P1.x - P0.x)); dy1 = (int)(Math.Floor(P1.y - P0.y));
            dx2 = (int)(Math.Floor(P2.x - P0.x)); dy2 = (int)(Math.Floor(P2.y - P0.y));
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

            float Kx = P11.x, Ky = P11.y, Mx = P21.x, My = P21.y;
            float Lx = (P12.x - P11.x), Ly = (P12.y - P11.y), Nx = (P22.x - P21.x), Ny = (P22.y - P21.y);
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