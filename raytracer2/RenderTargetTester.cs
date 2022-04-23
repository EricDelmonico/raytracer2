using System;

namespace raytracer2
{
    /// <summary>
    /// Tests a RenderTarget class
    /// </summary>
    public class RenderTargetTester : IRenderer
    {
        private Random rng;
        public int SamplesPerPass { get; set; }

        public RenderTargetTester()
        {
            rng = new Random();
        }

        public void RenderTo(IRenderTarget target, int _)
        {
            target.Clear();
            for (int i = 0; i < 2000; i++)
            {
                target.SetPixel(rng.Next(0, target.Width), rng.Next(0, target.Height), new Vec3(rng.Next(0, 1), rng.Next(0, 1), rng.Next(0, 1)));
            }
        }
    }
}
