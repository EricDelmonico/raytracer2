using Microsoft.Xna.Framework;
using System;

using System.Security.Cryptography;

namespace raytracer2
{
    public class RaytracingRenderer : IRenderer
    {
        private HittableList world;
        private int maxDepth = 50;

        public int SamplesPerPass { get; set; }

        public RaytracingRenderer(int samplesPerPass)
        {
            world = new HittableList();
            world.objects.Add(new Sphere(new Vec3(0, 0, -1), 0.5));
            world.objects.Add(new Sphere(new Vec3(0, -100.5, -1), 100));

            SamplesPerPass = samplesPerPass;
        }

        private Vec3 RayColor(Ray r, int depth)
        {
            HitRecord rec = new HitRecord();
            if (depth <= 0)
                return Vec3.Zero;

            if (world.Hit(r, 0.01, double.PositiveInfinity, ref rec))
            {
                Vec3 target = rec.p + rec.normal + Vec3.RandomInUnitSphere();
                return 0.5 * RayColor(new Ray(rec.p, target - rec.p), depth - 1);
            }

            Vec3 unitDir = r.direction.normalized;
            double t = 0.5 * (unitDir.y + 1.0);
            return (1.0 - t) * Vec3.One + t * new Vec3(0.5, 0.7, 1.0);
        }

        public void RenderTo(IRenderTarget target, int row)
        {
            Camera cam = target as Camera;

            // Loop through pixels and cast rays
            int y = row;
            for (int x = 0; x < cam.Width; x++)
            {
                for (int i = 0; i < SamplesPerPass; i++) 
                {
                    double u = (x + RayHitHelpers.RandomDouble()) / (target.Width - 1);
                    double v = (y + RayHitHelpers.RandomDouble()) / (target.Height - 1);
                    Ray currentRay = cam.GetRay(u, v);
                    Vec3 color = RayColor(currentRay, maxDepth);
                    cam.AddPixel(x, y, color);
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
