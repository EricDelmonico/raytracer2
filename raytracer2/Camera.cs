using Microsoft.Xna.Framework;

namespace raytracer2
{
    public enum MoveAxis
    {
        LeftRight,
        UpDown,
        ForwardBack
    }

    /// <summary>
    /// Simple Camera that contains basic info like viewport dimensions, focal length, origin, direction, etc
    /// </summary>
    public class Camera : SimpleRenderTarget
    {
        private int _width;
        public override int Width
        {
            get => _width;
            protected set
            {
                if (_width != value)
                {
                    _width = value;
                    _aspect = null; // Reset aspect
                }
            }
        }
        private int _height;
        public override int Height
        {
            get => _height;
            protected set
            {
                if (_height != value)
                {
                    _height = value;
                    _aspect = null; // Reset aspect
                }
            }
        }

        // Aspect ratio of the camera, initializes when needed
        private float? _aspect;
        public float Aspect => _aspect.HasValue ? _aspect.Value : (_aspect = (float)Width / Height).Value;
        public double FocalLength => 1.0;

        public Vec3 Position { get; private set; } = Vec3.Zero;
        public double ViewportHeight => 2.0;
        public double ViewportWidth => ViewportHeight * Aspect;

        public Vec3 Horizontal => Right * ViewportWidth;
        public Vec3 Vertical => Up * ViewportHeight;

        public Vec3 Right { get; private set; }
        public Vec3 Up { get; private set; }
        public Vec3 Forward { get; private set; }

        // Make viewport vectors
        public Vec3 LowerLeft => Position - Horizontal / 2 - Vertical / 2 + Forward * FocalLength;

        public Camera(Vec3 forward, int width, int height, GraphicsDeviceManager graphicsDeviceManager) : base(width, height, graphicsDeviceManager)
        {
            Width = width;
            Height = height;

            Forward = forward;
            // Cross with global up to get right
            Right = Vec3.Cross(new Vec3(0, 1, 0), Forward).normalized;
            // Cross forward with right for actual up
            Up = Vec3.Cross(Forward, Right).normalized;
            Refresh = true;
        }

        public override void Resize(int newWidth, int newHeight)
        {
            base.Resize(newWidth, newHeight);
        }

        public void ChangeForwardVec(Vector2 moveBy)
        {
            Forward -= moveBy.X * Right + -moveBy.Y * Up;
            Forward = Forward.normalized;
            // Cross with global up to get right
            Right = Vec3.Cross(new Vec3(0, 1, 0), Forward).normalized;
            // Cross forward with right for actual up
            Up = Vec3.Cross(Forward, Right).normalized;
            Refresh = true;
        }

        public void Move(double amount, MoveAxis axis)
        {
            switch (axis)
            {
                case MoveAxis.ForwardBack:
                    Position += Forward * amount;
                    break;
                case MoveAxis.LeftRight:
                    Position += Right * amount;
                    break;
                case MoveAxis.UpDown:
                    Position += Up * amount;
                    break;
            }
            Refresh = true;
        }

        /// <summary>
        /// Returns a ray pointing into the passed in viewport coordinates
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public Ray GetRay(double u, double v)
        {
            return new Ray(Position, LowerLeft + u * Horizontal + v * Vertical - Position);
        }
    }
}
