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
        public abstract bool Scatter(Ray r, ref HitRecord hit, out Vec3 atten, out Ray scattered);
    }

    public class Lambertian : Material
    {
        public Vec3 albedo;

        public Lambertian(Vec3 color)
        {
            albedo = color;
        }

        public override bool Scatter(Ray r, ref HitRecord hit, out Vec3 atten, out Ray scattered)
        {
            Vec3 scatterDir = hit.normal + Vec3.Random().normalized;

            if (scatterDir.NearZero())
                scatterDir = hit.normal;

            scattered = new Ray(hit.p, scatterDir);
            atten = albedo;
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
            Vec3 reflected = RayHitHelpers.Reflect(r.direction.normalized, hit.normal);
            scattered = new Ray(hit.p, reflected + fuzz * Vec3.RandomInUnitSphere());
            atten = albedo;
            return Vec3.Dot(scattered.direction.normalized, hit.normal) > 0;
        }
    }
}
