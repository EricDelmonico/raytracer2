using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace raytracer2
{
    /// <summary>
    /// An interface for objects that allow rendering to them
    /// </summary>
    public interface IRenderTarget
    {
        // Dimensions of this target
        int Width { get; }
        int Height { get; }

        /// <summary>
        /// Whether tex should be cleared before the next draw
        /// </summary>
        bool Refresh { get; }
        
        /// <summary>
        /// Sets the pixel at the given x, y position to the passed in color
        /// </summary>
        /// <remarks>Note: This method will not apply changes until the next draw</remarks>
        /// <param name="x">The x position of the pixel to modify</param>
        /// <param name="y">The y position of the pixel to modify</param>
        /// <param name="data">The color to set the given pixel to</param>
        void SetPixel(int x, int y, Vec3 data);

        /// <summary>
        /// Adds the passed in color to the pixel at the given x, y position
        /// </summary>
        /// <param name="x">X position of the pixel to modify</param>
        /// <param name="y">Y position of the pixel to modify</param>
        /// <param name="data">Color to add to the pixel</param>
        void AddPixel(int x, int y, Vec3 data);

        /// <summary>
        /// Draw this target to the screen
        /// </summary>
        /// <param name="spriteBatch"></param>
        void Draw(SpriteBatch spriteBatch);

        /// <summary>
        /// Clears this render target
        /// </summary>
        void Clear();

        /// <summary>
        /// Resizes this target to the given width and height
        /// </summary>
        /// <param name="newWidth"></param>
        /// <param name="newHeight"></param>
        void Resize(int newWidth, int newHeight);
    }
}
