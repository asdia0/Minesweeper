namespace Minesweeper.Analysis
{
    using System.Linq;

    internal class Utility
    {
        /// <summary>
        /// Calculates the mean coordinate of mines on a <see cref="Grid">grid</see>.
        /// </summary>
        /// <param name="length">The length of the <see cref="Grid">grid</see>.</param>
        /// <param name="width">The width of the <see cref="Grid">grid</see>.</param>
        /// <param name="mines">The number of mines on the <see cref="Grid">grid</see>.</param>
        /// <param name="count">The number of times to simulate different <see cref="Grid">grids</see> of the same dimensions.</param>
        /// <returns></returns>
        public static (double, double) Centroid(int length, int width, int mines, int count)
        {
            (double, double) centroid = new();

            // Simulate grids
            for (int i = 0; i < count; i++)
            {
                Grid grid = new(length, width, mines);

                foreach (Cell cell in grid.Cells.Where(j => j.HasMine))
                {
                    centroid.Item1 = cell.Point.Coordinates.X;
                    centroid.Item2 = cell.Point.Coordinates.Y;
                }
            }

            // Calculate average
            centroid.Item1 /= (count * mines);
            centroid.Item2 /= (count * mines);

            return centroid;
        }
    }
}
