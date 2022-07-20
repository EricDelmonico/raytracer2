using Microsoft.Xna.Framework;
using System;

using System.Security.Cryptography;

namespace raytracer2
{
    public enum Worlds
    {
        CornellBox,
        World1
    }

    public class RaytracingRenderer : IRenderer
    {
        private HittableList world1;
        private HittableList cornellBox;
        public HittableList CurrentWorld { get; private set; }

        private int maxDepth = 50;

        public int SamplesPerPass { get; set; }

        private Vec3 backgroundColor = Vec3.Zero;

        public RaytracingRenderer(int samplesPerPass)
        {
            #region World 1
            world1 = new HittableList();
            // New diffuse light with an intensity of 10
            Material emissionTest = new DiffuseLight(100);
            CustomTexture image = new ImageTexture(@"C:\Users\silen\Downloads\rip keytar.png");
            world1.objects.Add(new Sphere(new Vec3(0, 2, -1), 0.5, emissionTest));
            world1.objects.Add(new Sphere(new Vec3(1.25, 0, -1), 0.5, new Metal(new Vec3(.8, .6, 0.2), 0.01)));
            world1.objects.Add(new Sphere(new Vec3(-1.25, 0, -1), 0.5, new Dielectric(1.5)));

            // World "floor"
            CustomTexture checker = new CheckerTexture(new Vec3(0.2, 0.3, 0.1), new Vec3(0.9, 0.9, 0.9));
            world1.objects.Add(new Sphere(new Vec3(0, -1000.5, -1), 1000, new Lambertian(checker)));

            #endregion

            #region World 2 -- Cornell Box
            cornellBox = new HittableList();

            var red = new Lambertian(new Vec3(0.65, 0.05, 0.05));
            var white = new Lambertian(new Vec3(0.9, 0.9, 0.9));
            var green = new Lambertian(new Vec3(0.12, 0.45, 0.15));
            var light = new DiffuseLight(image, 2);

            cornellBox.Add(new YZRect(0, 555, 0, 555, 555, green));
            cornellBox.Add(new YZRect(0, 555, 0, 555, 0, red));
            //cornellBox.Add(new XZRect(213, 343, 227, 332, 554, light));
            cornellBox.Add(new XZRect(0, 555, 0, 555, 0, light));
            cornellBox.Add(new XZRect(0, 555, 0, 555, 555, white));
            cornellBox.Add(new XYRect(0, 555, 0, 555, 555, white));

            // Boxes
            Hittable box1 = new Box(new Vec3(0, 0, 0), new Vec3(165, 330, 165), white);
            box1 = new RotateY(box1, 15);
            box1 = new Translate(box1, new Vec3(265, 0, 295));
            cornellBox.Add(box1);

            Hittable box2 = new Box(new Vec3(0, 0, 0), new Vec3(165, 165, 165), white);
            box2 = new RotateY(box2, -18);
            box2 = new Translate(box2, new Vec3(130, 0, 65));
            cornellBox.Add(box2);

            #endregion

            SamplesPerPass = samplesPerPass;
        }

        private Vec3 RayColor(Ray r, int depth)
        {
            HitRecord rec = new HitRecord();
            if (depth <= 0)
                return Vec3.Zero;

            if (!CurrentWorld.Hit(r, 0.01, double.PositiveInfinity, ref rec))
                return backgroundColor;

            Ray scattered;
            Vec3 atten;
            Vec3 emittedColor = rec.material.ColorEmitted(rec.u, rec.v, rec.p);

            if (!rec.material.Scatter(r, ref rec, out atten, out scattered))
                return emittedColor;

            return emittedColor + atten * RayColor(scattered, depth - 1);
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

        public void SetWorld(string world)
        {
            switch (Enum.Parse(typeof(Worlds), world))
            {
                case Worlds.World1:
                    if (CurrentWorld == world1) return;

                    CurrentWorld = world1;
                    break;
                case Worlds.CornellBox:
                    if (CurrentWorld == cornellBox) return;

                    CurrentWorld = cornellBox;
                    break;
            }
        }
    }
}
