using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace raytracer2
{
    /// <summary>
    /// Vector3 struct using doubles instead of floats
    /// </summary>
    public struct Vec3
    {
        public static Vec3 One => new Vec3(1.0, 1.0, 1.0);
        public static Vec3 Zero => new Vec3(0.0, 0.0, 0.0);

        public double x, y, z;

        // This vector, normalized
        public Vec3 normalized => this / Length();

        public Vec3(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static Vec3 operator -(Vec3 a, Vec3 b)
        {
            a.x -= b.x;
            a.y -= b.y;
            a.z -= b.z;
            return a;
        }

        public static Vec3 operator +(Vec3 a, Vec3 b)
        {
            a.x += b.x;
            a.y += b.y;
            a.z += b.z;
            return a;
        }

        public static Vec3 operator /(Vec3 vec, double q)
        {
            vec.x /= q;
            vec.y /= q;
            vec.z /= q;
            return vec;
        }

        public static Vec3 operator *(Vec3 vec, double f)
        {
            vec.x *= f;
            vec.y *= f;
            vec.z *= f;
            return vec;
        }

        public static Vec3 operator *(double f, Vec3 vec)
        {
            vec *= f;
            return vec;
        }

        public static Vec3 operator -(Vec3 vec)
        {
            vec.x = -vec.x;
            vec.y = -vec.y;
            vec.z = -vec.z;
            return vec;
        }

        public static double Dot(Vec3 a, Vec3 b) => a.x * b.x + a.y * b.y + a.z * b.z;
        public static Vec3 Cross(Vec3 a, Vec3 b) =>
            new Vec3(
                a.y * b.z - a.z * b.y,
                a.z * b.x - a.x * b.z,
                a.x * b.y - a.y * b.x);

        public double SqrLength() => x * x + y * y + z * z;
        public double Length() => Math.Sqrt(SqrLength());

        public static Color ToColor(Vec3 v) => new Color((float)(v.x), (float)(v.y), (float)(v.z));

        public static Vec3 Random() =>
            new Vec3(RayHitHelpers.RandomDouble(), RayHitHelpers.RandomDouble(), RayHitHelpers.RandomDouble());

        public static Vec3 Random(double min, double max) =>
            new Vec3(RayHitHelpers.RandomDoubleRange(min, max), RayHitHelpers.RandomDoubleRange(min, max), RayHitHelpers.RandomDoubleRange(min, max));

        public static Vec3 RandomInUnitSphere()
        {
            while (true)
            {
                var p = Random(-1, 1);
                if (p.SqrLength() >= 1) continue;
                return p;
            }
        }

        public Vec3 Sqrt()
        {
            x = Math.Sqrt(x);
            y = Math.Sqrt(y);
            z = Math.Sqrt(z);
            return this;
        }
    }

    public struct PixelData
    {
        public Vec3 color;
        public int sampleCount;
    }
}
