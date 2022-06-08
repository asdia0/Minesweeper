namespace Minesweeper
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
    }
}