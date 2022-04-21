namespace raytracer2
{
    /// <summary>
    /// Renders to a render target
    /// </summary>
    public interface IRenderer
    {
        /// <summary>
        /// Renders image to the given target
        /// </summary>
        /// <param name="target">The target to render to</param>
        public void RenderTo(IRenderTarget target, int thread = 1, int maxThreads = 1);
    }
}
