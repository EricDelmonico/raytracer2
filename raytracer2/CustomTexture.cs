using System;
using System.Collections.Generic;
using System.Text;

namespace raytracer2
{
    /// <summary>
    /// Allows for custom textures without the limitations/unneeded extras/hidden elements of MonoGame's texturing
    /// </summary>
    public abstract class CustomTexture
    {
        public abstract Vec3 Value(double u, double v, Vec3 p);
    }

    public class SolidColor : CustomTexture
    {
        private Vec3 colorValue;

        public SolidColor() : this(Vec3.One) { }

        public SolidColor(Vec3 color)
        {
            colorValue = color;
        }

        public override Vec3 Value(double u, double v, Vec3 p)
        {
            return colorValue;
        }
    }

    public class CheckerTexture : CustomTexture
    {
        private CustomTexture odd;
        private CustomTexture even;

        public CheckerTexture(CustomTexture odd, CustomTexture even)
        {
            this.odd = odd;
            this.even = even;
        }

        public CheckerTexture(Vec3 color1, Vec3 color2 ) : this(new SolidColor(color1), new SolidColor(color2)) { }

        public override Vec3 Value(double u, double v, Vec3 p)
        {
            double sins = Math.Sin(10 * p.x) * Math.Sin(10 * p.y) * Math.Sin(10 * p.z);
            if (sins < 0) 
                return odd.Value(u, v, p);
            else 
                return even.Value(u, v, p);
        }
    }

    public class ImageTexture : CustomTexture
    {
        private Vec3[,] data;
        private int width;
        private int height;

        public ImageTexture(string filename)
        {
            using (var bmp = new System.Drawing.Bitmap(filename))
            {
                // Lay the data out in x, y format for ease
                data = new Vec3[bmp.Width, bmp.Height];
                width = bmp.Width;
                height = bmp.Height;
                for (int x = 0; x < bmp.Width; x++)
                {
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        var px = bmp.GetPixel(x, y);
                        Vec3 color = new Vec3(px.R / 255.0, px.G / 255.0, px.B / 255.0);
                        data[x, y] = color;
                    }
                }
            }
        }

        public override Vec3 Value(double u, double v, Vec3 p)
        {
            u = Math.Clamp(u, 0.0, 1.0);
            v = 1.0 - Math.Clamp(v, 0.0, 1.0); // Flip v

            int x = (int)(u * (width - 1));
            int y = (int)(v * (height - 1));

            return data[x, y];
        }
    }
}
