namespace raytracer2
{
    /// <summary>
    /// Renders to a render target
    /// </summary>
    public interface IRenderer
    {
        int SamplesPerPass { get; set; }

        /// <summary>
        /// Renders image to the given target
        /// </summary>
        /// <param name="target">The target to render to</param>
        public void RenderTo(IRenderTarget target, int row);
    }
}
