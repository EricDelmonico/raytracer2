using Microsoft.Xna.Framework;
using System;

using System.Security.Cryptography;

namespace raytracer2
{
    public class RaytracingRenderer : IRenderer
    {
        private HittableList world;

        public RaytracingRenderer()
        {
            world = new HittableList();
        }

        private Vec3 RayColor(Ray r)
        {
            HitRecord rec = new HitRecord();
            if (world.Hit(r, 0, 100, ref rec))
            {
                return 0.5 * (rec.normal + Vec3.One);
            }

            Vec3 unitDir = r.direction.normalized;
            double t = 0.5 * (unitDir.y + 1.0);
            return (1.0 - t) * Vec3.One + t * new Vec3(0.5, 0.7, 1.0);
        }

        public void RenderTo(IRenderTarget target, int thread, int maxThreads)
        {
            Camera cam = target as Camera;

            world.objects.Add(new Sphere(new Vec3(0, 0, -1), 0.5));
            world.objects.Add(new Sphere(new Vec3(0, -100.5, -1), 100));

            // Loop through pixels and cast rays
            for (int y = 0; y < cam.Height; y++)
            {
                for (int x = 0; x < cam.Width; x++)
                {
                    //for (int i = 0; i < samplesPerPixel; i++) 
                    {
                        double t = RandomOffset();
                        double test = (t + x);
                        double u = test / (target.Width - 1);
                        double v = (y + RandomOffset()) / (target.Height - 1);
                        Ray currentRay = cam.GetRay(u, v);
                        Vec3 color = RayColor(currentRay);
                        cam.AddPixel(x, y, color);// / samplesPerPixel);
                    }
                }
            }
        }

        /// <summary>
        /// Returns a random double
        /// </summary>
        /// <returns></returns>
        private double RandomDouble()
        {
            byte[] bytes = new byte[8];
            RandomNumberGenerator.Fill(bytes);
            long num = BitConverter.ToInt64(bytes);
            if (num == 0) num++;
            return (double)num / long.MaxValue;
        }

        /// <summary>
        /// Returns a random double in [min, max)
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        private double RandomDoubleRange(double min, double max)
        {
            return min + (max - min) * RandomDouble();
        }

        private double RandomOffset()
        {
            var test = RandomDoubleRange(0, 1.0);
            return test;
        }
    }
}
