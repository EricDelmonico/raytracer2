using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;

namespace raytracer2
{
    /// <summary>
    /// This class will hold the data that results from RayTracing and handle drawing it to the screen
    /// </summary>
    public class SimpleRenderTarget : IRenderTarget
    {
        // The actual texture that will be rendered to the screen
        private Texture2D tex;
        // The data that tex will contain
        private PixelData[] colorData;

        private uint[] TexData => colorData.Select((c) => Vec3.ToColor((c.color / c.sampleCount).Sqrt()).PackedValue).ToArray();

        // Dimensions of the texture
        public virtual int Width { get; protected set; }
        public virtual int Height { get; protected set; }

        private GraphicsDeviceManager graphicsDeviceManager;

        // True if there has been changes to the texture since last draw, false otherwise
        private bool dirty;
        
        public bool Refresh { get; protected set; }

        /// <summary>
        /// A render target that simply puts the data given to it into a texture and outputs the result
        /// </summary>
        /// <param name="ui"></param>
        /// <param name="graphicsDeviceManager"></param>
        public SimpleRenderTarget(int width, int height, GraphicsDeviceManager graphicsDeviceManager)
        {
            this.graphicsDeviceManager = graphicsDeviceManager;
            Resize(width, height);
            Clear();
        }

        /// <summary>
        /// Resizes this target to the given width and height
        /// </summary>
        /// <param name="newWidth"></param>
        /// <param name="newHeight"></param>
        public virtual void Resize(int newWidth, int newHeight)
        {
            int oldWidth = Width;
            int oldHeight = Height;

            // Apply the changes to the width and height of this texture
            Width = newWidth;
            Height = newHeight;

            // Make a new data array
            var oldData = colorData;
            // Make sure the new colorData array is initialized
            colorData = FreshColorDataArray;

            // Copy old data over
            if (oldData != null)
            {
                for (int y = 0; y < oldHeight; y++)
                {
                    for (int x = 0; x < oldWidth; x++)
                    {
                        // Only copy data if it'd fit in the new data array. Otherwise, throw the data out.
                        int oldTrueIndex = y * oldWidth + x;
                        int newtrueIndex = y * Width + x;
                        if (newtrueIndex < colorData.Length) colorData[newtrueIndex] = oldData[oldTrueIndex];
                    }
                }
            }

            // Create a new texture and copy over the data
            tex = new Texture2D(graphicsDeviceManager.GraphicsDevice, Width, Height);
            tex.SetData(TexData);

            Refresh = true;
        }

        /// <summary>
        /// Sets the pixel at the given x, y position to the given color
        /// </summary>
        /// <remarks>Note: This method will not apply changes until the next draw</remarks>
        /// <param name="x">The x position of the pixel to modify</param>
        /// <param name="y">The y position of the pixel to modify</param>
        /// <param name="color">The color to set the given pixel to</param>
        public void SetPixel(int x, int y, Vec3 color)
        {
            int trueIndex = Math.Clamp(((Height - 1) * Width - y * Width) + x, 0, colorData.Length - 1);
            colorData[trueIndex].color = color;
            dirty = true;
        }

        /// <summary>
        /// Gets the data of the pixel at the given x,y position
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>Data of the pixel at the given x,y position</returns>
        public PixelData GetPixel(int x, int y)
        {
            int trueIndex = Math.Clamp(((Height - 1) * Width - y * Width) + x, 0, colorData.Length - 1);
            return colorData[trueIndex];
        }

        public void AddPixel(int x, int y, Vec3 color)
        {
            int trueIndex = Math.Clamp(((Height - 1) * Width - y * Width) + x, 0, colorData.Length - 1);
            int samples = colorData[trueIndex].sampleCount + 1;
            colorData[trueIndex].color += color;
            colorData[trueIndex].sampleCount = samples;
            dirty = true;
        }

        private void ApplyTextureChangesIfDirty()
        {
            if (dirty) tex.SetData(TexData);
        }

        public virtual void Clear()
        {
            colorData = FreshColorDataArray;
            tex.SetData(TexData);
            Refresh = false;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            ApplyTextureChangesIfDirty();
            spriteBatch.Draw(tex, new Rectangle(Vector2.Zero.ToPoint(), new Point(graphicsDeviceManager.PreferredBackBufferWidth, graphicsDeviceManager.PreferredBackBufferHeight)), Color.White);
            if (Refresh) Clear();

            spriteBatch.End();
        }

        public PixelData[] FreshColorDataArray => new PixelData[Width * Height];

        public void SaveImage(string name)
        {
            ApplyTextureChangesIfDirty();
            using (var stream = System.IO.File.OpenWrite(name))
            {
                tex.SaveAsPng(stream, tex.Width, tex.Height);
            }
        }
    }
}
