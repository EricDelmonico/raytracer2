using System;
using System.Collections.Generic;
using System.Text;

namespace raytracer2
{
    /// <summary>
    /// Simple Ray class using doubles rather than floats
    /// </summary>
    public class Ray
    {
        public Vec3 origin;
        public Vec3 direction;

        public Ray(Vec3 origin, Vec3 direction)
        {
            this.origin = origin;
            this.direction = direction;
        }

        public Vec3 At(double t) => origin + t * direction;
    }
}
