﻿namespace Minesweeper
{
    /// <summary>
    /// A class for utility functions.
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Checks that the parameters for a <see cref="Grid">grid</see> are valid.
        /// Throws an <see cref="MinesweeperException">exception</see> if not.
        /// </summary>
        /// <param name="length">The length (y-axis) of the <see cref="Grid">grid </see> measured in <see cref="Cell">cells</see>.</param>
        /// <param name="width">The width (x-axis) of the <see cref="Grid">grid </see> measured in <see cref="Cell">cells</see>.</param>
        /// <param name="mines">The number of mines on the <see cref="Grid">grid</see>.</param>
        public static void CheckGridParams(int length, int width, int mines)
        {
            if (length <= 0 || width <= 0)
            {
                throw new MinesweeperException("Invalid grid size: grid size must be positive.");
            }

            if (mines <= 0)
            {
                throw new MinesweeperException("Invalid number of mines: number of mines must be positive.");
            }
        }

        /// <summary>
        /// Converts a <see cref="Cell">cell</see>'s index in <see cref="Grid.Cells"/> to its <see cref="Point.Coordinates">coordinates</see>: (0, 0), (0, 1), ... (0, n), (1, 0), ... (m, n).
        /// </summary>
        /// <param name="index">The index of the <see cref="Cell">cell</see> in <see cref="Grid.Cells"/>.</param>
        /// <param name="width">The width (x-axis) of the <see cref="Grid">grid </see> measured in <see cref="Cell">cells</see>.</param>
        /// <returns>The <see cref="Point.Coordinates">coordinates</see> of the <see cref="Cell">cell</see>.</returns>
        public static (int X, int Y) CellIndexToCoordinates(int index, int width)
        {
            int x = index % width;
            int y = (index - x) / width;

            return (x, y);
        }

        /// <summary>
        /// Converts a <see cref="Cell">cell</see>'s <see cref="Point.Coordinates">coordinates</see> to its index in <see cref="Grid.Cells"/>.
        /// </summary>
        /// <param name="coordinates">The <see cref="Point.Coordinates">coordinates</see> of the <see cref="Cell">cell</see>.</param>
        /// <param name="width">The width (x-axis) of the <see cref="Grid">grid </see> measured in <see cref="Cell">cells</see>.</param>
        /// <returns>The <see cref="Cell">cell</see>'s index in <see cref="Grid.Cells"/>.</returns>
        public static int CellCoordinatesToIndex((int x, int y) coordinates, int width)
        {
            return coordinates.x + (coordinates.y * width);
        }

        /// <summary>
        /// Converts mine density to mines.
        /// Mind density must be between 0 and 1 inclusive.
        /// </summary>
        /// <param name="length">The length of the <see cref="Grid">grid</see>.</param>
        /// <param name="width">The width of the <see cref="Grid">grid</see>.</param>
        /// <param name="density">The density of mines on the <see cref="Grid">grid</see>.
        ///     Calculated as mines/(<paramref name="length"/>*<paramref name="width"/>).</param>
        /// <returns>The number of mines closest to the given <paramref name="density">density</paramref>.</returns>
        /// <exception cref="System.Exception">Throws an exception if the density is not strictly between 0 and 1.</exception>
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