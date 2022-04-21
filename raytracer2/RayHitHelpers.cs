using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;

namespace raytracer2
{
    /// <summary>
    /// Contains methods that calculate ray intersections
    /// </summary>
    public static class RayHitHelpers
    {
    }
    
    /// <summary>
    /// Struct to track information about a hit
    /// </summary>
    public struct HitRecord
    {
        public Vec3 p;
        public Vec3 normal;
        public double t;
        public bool frontFace;

        public HitRecord(Vec3 p, Vec3 normal, double t)
        {
            this.p = p;
            this.normal = normal;
            this.t = t;
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
        public ConcurrentBag<Hittable> objects;

        public HittableList() : this(new ConcurrentBag<Hittable>()) { }

        public HittableList(ConcurrentBag<Hittable> objects)
        {
            this.objects = objects;
        }

        public override bool Hit(Ray ray, double tMin, double tMax, ref HitRecord rec)
        {
            HitRecord tempRec = new HitRecord();
            bool hitAnything = false;
            double closestSoFar = tMax;
            
            foreach (var o in objects)
            {
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

        public Sphere() : this(center: Vec3.Zero, radius: 1.0) { }

        public Sphere(Vec3 center, double radius)
        {
            Center = center;
            Radius = radius;
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

            return true;
        }
    }
}
