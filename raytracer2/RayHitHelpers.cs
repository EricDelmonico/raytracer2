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
        public double u;
        public double v;
        public bool frontFace;

        public HitRecord(Vec3 p, Vec3 normal, double t, double u, double v, Material material)
        {
            this.p = p;
            this.normal = normal;
            this.t = t;
            this.u = u;
            this.v = v;
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
            GetSphereUV(outwardNormal, out rec.u, out rec.v);
            rec.material = Material;

            return true;
        }

        private static void GetSphereUV(Vec3 p, out double u, out double v)
        {
            double theta = Math.Acos(-p.y);
            double phi = Math.Atan2(-p.z, p.x) + Math.PI;

            u = phi / (2 * Math.PI);
            v = theta / Math.PI;
        }
    }

    public class XYRect : Hittable
    {
        private Material material;
        private double x0, x1, y0, y1, k;

        public XYRect(double x0, double x1, double y0, double y1, double k, Material material)
        {
            this.x0 = x0;
            this.x1 = x1;
            this.y0 = y0;
            this.y1 = y1;
            this.k = k;
            this.material = material;
        }

        public override bool Hit(Ray ray, double tMin, double tMax, ref HitRecord rec)
        {
            double t = (k - ray.origin.z) / ray.direction.z;
            if (t < tMin || t > tMax)
                return false;

            double x = ray.origin.x + t * ray.direction.x;
            double y = ray.origin.y + t * ray.direction.y;
            if (x < x0 || x > x1 || y < y0 || y > y1)
                return false;

            rec.u = (x - x0) / (x1 - x0);
            rec.v = (y - y0) / (y1 - y0);
            rec.t = t;
            Vec3 outwardNormal = new Vec3(0, 0, 1);
            rec.SetFaceNormal(ray, outwardNormal);
            rec.material = material;
            rec.p = ray.At(t);
            return true;
        }
    }

    public class XZRect : Hittable
    {
        private Material material;
        private double x0, x1, z0, z1, k;

        public XZRect(double x0, double x1, double z0, double z1, double k, Material material)
        {
            this.x0 = x0;
            this.x1 = x1;
            this.z0 = z0;
            this.z1 = z1;
            this.k = k;
            this.material = material;
        }

        public override bool Hit(Ray ray, double tMin, double tMax, ref HitRecord rec)
        {
            double t = (k - ray.origin.y) / ray.direction.y;
            if (t < tMin || t > tMax)
                return false;

            double x = ray.origin.x + t * ray.direction.x;
            double z = ray.origin.z + t * ray.direction.z;
            if (x < x0 || x > x1 || z < z0 || z > z1)
                return false;

            rec.u = (x - x0) / (x1 - x0);
            rec.v = (z - z0) / (z1 - z0);
            rec.t = t;
            Vec3 outwardNormal = new Vec3(0, 1, 0);
            rec.SetFaceNormal(ray, outwardNormal);
            rec.material = material;
            rec.p = ray.At(t);
            return true;
        }
    }

    public class YZRect : Hittable
    {
        private Material material;
        private double y0, y1, z0, z1, k;

        public YZRect(double y0, double y1, double z0, double z1, double k, Material material)
        {
            this.y0 = y0;
            this.y1 = y1;
            this.z0 = z0;
            this.z1 = z1;
            this.k = k;
            this.material = material;
        }

        public override bool Hit(Ray ray, double tMin, double tMax, ref HitRecord rec)
        {
            double t = (k - ray.origin.x) / ray.direction.x;
            if (t < tMin || t > tMax)
                return false;

            double y = ray.origin.y + t * ray.direction.y;
            double z = ray.origin.z + t * ray.direction.z;
            if (y < y0 || y > y1 || z < z0 || z > z1)
                return false;

            rec.u = (y - y0) / (y1 - y0);
            rec.v = (z - z0) / (z1 - z0);
            rec.t = t;
            Vec3 outwardNormal = new Vec3(1, 0, 0);
            rec.SetFaceNormal(ray, outwardNormal);
            rec.material = material;
            rec.p = ray.At(t);
            return true;
        }
    }

    public class Box : Hittable
    {
        private Vec3 boxMin;
        private Vec3 boxMax;
        private HittableList sides;

        public Box(Vec3 p0, Vec3 p1, Material material)
        {
            boxMin = p0;
            boxMax = p1;

            sides = new HittableList();
            sides.Add(new XYRect(p0.x, p1.x, p0.y, p1.y, p1.z, material));
            sides.Add(new XYRect(p0.x, p1.x, p0.y, p1.y, p0.z, material));

            sides.Add(new XZRect(p0.x, p1.x, p0.z, p1.z, p1.y, material));
            sides.Add(new XZRect(p0.x, p1.x, p0.z, p1.z, p0.y, material));

            sides.Add(new YZRect(p0.y, p1.y, p0.z, p1.z, p1.x, material));
            sides.Add(new YZRect(p0.y, p1.y, p0.z, p1.z, p0.x, material));
        }

        public override bool Hit(Ray ray, double tMin, double tMax, ref HitRecord rec)
        {
            return sides.Hit(ray, tMin, tMax, ref rec);
        }
    }

    #region Instances

    public class Translate : Hittable
    {
        private Hittable hittable;
        private Vec3 offset;

        public Vec3 Offset
        {
            get => offset;
            set => offset = value;
        }

        public Translate(Hittable hittable, Vec3 offset)
        {
            this.hittable = hittable;
            this.offset = offset;
        }

        public override bool Hit(Ray ray, double tMin, double tMax, ref HitRecord rec)
        {
            Ray moved = new Ray(ray.origin - offset, ray.direction);
            if (!hittable.Hit(moved, tMin, tMax, ref rec))
                return false;

            rec.p += offset;
            rec.SetFaceNormal(moved, rec.normal);
            return true;
        }
    }

    public class RotateY : Hittable
    {
        private Hittable hittable;
        private double sin;
        private double cos;

        public RotateY(Hittable hittable, double angle)
        {
            this.hittable = hittable;
            angle = (Math.PI / 180) * angle;
            sin = Math.Sin(angle);
            cos = Math.Cos(angle);
        }

        public override bool Hit(Ray ray, double tMin, double tMax, ref HitRecord rec)
        {
            Vec3 origin = ray.origin;
            Vec3 direction = ray.direction;

            origin.x = cos * ray.origin.x - sin * ray.origin.z;
            origin.z = sin * ray.origin.x + cos * ray.origin.z;

            direction.x = cos * ray.direction.x - sin * ray.direction.z;
            direction.z = sin * ray.direction.x + cos * ray.direction.z;

            Ray rotated = new Ray(origin, direction);

            if (!hittable.Hit(rotated, tMin, tMax, ref rec))
                return false;

            Vec3 p = rec.p;
            Vec3 normal = rec.normal;

            p.x = cos * rec.p.x + sin * rec.p.z;
            p.z = -sin * rec.p.x + cos * rec.p.z;

            normal.x = cos * rec.normal.x + sin * rec.normal.z;
            normal.z = -sin * rec.normal.x + cos * rec.normal.z;

            rec.p = p;
            rec.SetFaceNormal(rotated, normal);
            return true;
        }
    }

    #endregion
}
