using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Security.Cryptography;

namespace raytracer2
{
    /// <summary>
    /// Contains methods that help with calculations
    /// </summary>
    public static class RayHitHelpers
    {
        /// <summary>
        /// Returns a random double
        /// </summary>
        /// <returns></returns>
        private static double Rand()
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
        public static double RandomDoubleRange(double min, double max)
        {
            return min + (max - min) * Rand();
        }

        /// <summary>
        /// Returns a random double from [0, 1)
        /// </summary>
        /// <returns></returns>
        public static double RandomDouble()
        {
            return RandomDoubleRange(0, 1.0);
        }

        public static Vec3 Reflect(Vec3 v, Vec3 n)
        {
            return v - 2 * Vec3.Dot(v, n) * n;
        }
    }
    
    /// <summary>
    /// Struct to track information about a hit
    /// </summary>
    public struct HitRecord
    {
        public Vec3 p;
        public Vec3 normal;
        public Material material;
        public double t;
        public bool frontFace;

        public HitRecord(Vec3 p, Vec3 normal, double t, Material material)
        {
            this.p = p;
            this.normal = normal;
            this.t = t;
            this.material = material;
            frontFace = false;
        }

        public void SetFaceNormal(Ray ray, Vec3 outwardNormal)
        {
            frontFace = Vec3.Dot(ray.direction, outwardNormal) < 0;
            normal = frontFace ? outwardNormal : -outwardNormal;
        }
    }

    /// <summary>
    /// Abstract hittable class
    /// </summary>
    public class Hittable
    {
        public virtual bool Hit(Ray ray, double tMin, double tMax, ref HitRecord rec) { return false; }
    }

    /// <summary>
    /// A hittable list of hittable objects!
    /// </summary>
    public class HittableList : Hittable
    {
        public List<Hittable> objects;

        public HittableList() : this(new List<Hittable>()) { }

        public HittableList(List<Hittable> objects)
        {
            this.objects = objects;
        }

        public override bool Hit(Ray ray, double tMin, double tMax, ref HitRecord rec)
        {
            HitRecord tempRec = new HitRecord();
            bool hitAnything = false;
            double closestSoFar = tMax;
            
            for (int i = 0; i < objects.Count; i++)
            {
                var o = objects[i];
                if (o.Hit(ray, tMin, closestSoFar, ref tempRec))
                {
                    hitAnything = true;
                    closestSoFar = tempRec.t;
                    rec = tempRec;
                }
            }

            return hitAnything;
        }

        public void Add(Hittable hittable) => objects.Add(hittable);
    }

    /// <summary>
    /// Mathematical sphere as a hittable object
    /// </summary>
    public class Sphere : Hittable
    {
        public Vec3 Center { get; set; }
        public double Radius { get; set; }
        public Material Material { get; set; }

        public Sphere() : this(center: Vec3.Zero, radius: 1.0, new Lambertian(Vec3.One)) { }

        public Sphere(Vec3 center, double radius, Material material)
        {
            Center = center;
            Radius = radius;
            Material = material;
        }

        public override bool Hit(Ray ray, double tMin, double tMax, ref HitRecord rec)
        {
            Vec3 oToC = ray.origin - Center;
            double a = ray.direction.SqrLength();
            double halfB = Vec3.Dot(oToC, ray.direction);
            double c = oToC.SqrLength() - Radius * Radius;
            double discriminant = halfB * halfB - a * c;
            if (discriminant < 0) return false;
            double sqrtDiscriminant = Math.Sqrt(discriminant);

            double root = (-halfB - sqrtDiscriminant) / a;
            if (root < tMin || tMax < root)
            {
                root = (-halfB + sqrtDiscriminant) / a;
                if (root < tMin || tMax < root)
                {
                    return false;
                }
            }

            rec.t = root;
            rec.p = ray.At(rec.t);
            Vec3 outwardNormal = (rec.p - Center) / Radius;
            rec.SetFaceNormal(ray, outwardNormal);
            rec.material = Material;

            return true;
        }
    }
}
