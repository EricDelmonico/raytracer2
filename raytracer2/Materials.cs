using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Text;

namespace raytracer2
{
    /// <summary>
    /// Abstract class for materials to define scattering behavior
    /// </summary>
    public abstract class Material
    {
        public virtual Vec3 ColorEmitted(double u, double v, Vec3 p) => Vec3.Zero;
        public abstract bool Scatter(Ray r, ref HitRecord hit, out Vec3 atten, out Ray scattered);
    }

    public class Lambertian : Material
    {
        public CustomTexture albedo;

        public Lambertian(Vec3 color)
        {
            albedo = new SolidColor(color);
        }

        public Lambertian(CustomTexture albedo)
        {
            this.albedo = albedo;
        }

        public override bool Scatter(Ray r, ref HitRecord hit, out Vec3 atten, out Ray scattered)
        {
            Vec3 scatterDir = hit.normal + Vec3.Random().normalized;

            if (scatterDir.NearZero())
                scatterDir = hit.normal;

            scattered = new Ray(hit.p, scatterDir);
            atten = albedo.Value(hit.u, hit.v, hit.p);
            return true;
        }
    }

    public class Metal : Material
    {
        public Vec3 albedo;
        public double fuzz;

        public Metal(Vec3 color, double fuzz = 1)
        {
            albedo = color;
            this.fuzz = Math.Clamp(fuzz, 0.0, 1.0);
        }

        public override bool Scatter(Ray r, ref HitRecord hit, out Vec3 atten, out Ray scattered)
        {
            Vec3 reflected = Vec3.Reflect(r.direction.normalized, hit.normal);
            scattered = new Ray(hit.p, reflected + fuzz * Vec3.RandomInUnitSphere());
            atten = albedo;
            return Vec3.Dot(scattered.direction.normalized, hit.normal) > 0;
        }
    }

    public class Dielectric : Material
    {
        public double iOfRefraction;

        public Dielectric(double iOfRefraction)
        {
            this.iOfRefraction = iOfRefraction;
        }

        public override bool Scatter(Ray r, ref HitRecord hit, out Vec3 atten, out Ray scattered)
        {
            atten = Vec3.One;
            double refractionRatio = hit.frontFace ? (1.0 / iOfRefraction) : iOfRefraction;

            Vec3 unitDir = r.direction.normalized;
            double cosTheta = Math.Min(Vec3.Dot(-unitDir, hit.normal), 1.0);
            double sinTheta = Math.Sqrt(1.0 - cosTheta * cosTheta);

            bool cannotRefract = refractionRatio * sinTheta > 1.0;
            Vec3 direction;

            if (cannotRefract || Reflectance(cosTheta, refractionRatio) > RayHitHelpers.RandomDouble()) direction = Vec3.Reflect(unitDir, hit.normal);
            else direction = Vec3.Refract(unitDir, hit.normal, refractionRatio);

            scattered = new Ray(hit.p, direction);
            return true;
        }

        private static double Reflectance(double cos, double refIdx)
        {
            double r0 = (1 - refIdx) / (1 + refIdx);
            r0 = r0 * r0;
            return r0 + (1 - r0) * Math.Pow((1 - cos), 5);
        }
    }
}
