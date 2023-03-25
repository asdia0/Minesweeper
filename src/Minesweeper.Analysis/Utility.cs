namespace Minesweeper.Analysis
{
    using System.Linq;

    internal class Utility
    {
        /// <summary>
        /// Converts mine density to mines.
        /// Mind density must be between 0 and 1 inclusive.
        /// </summary>
        /// <param name="length">The length of the <see cref="Grid">grid</see>.</param>
        /// <param name="width">The width of the <see cref="Grid">grid</see>.</param>
        /// <param name="density">The density of mines on the <see cref="Grid">grid</see>. Calculated as mines/(<paramref name="length"/>*<paramref name="width"/>).</param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public static int DensityToMines(int length, int width, double density)
        {
            if (density > 1 || density < 0)
            {
                throw new System.Exception("Invalid density.");
            }

            return (int)(length * width * density);
        }
    }
}
