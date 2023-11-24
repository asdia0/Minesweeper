namespace Minesweeper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a minesweeper grid.
    /// </summary>
    public class Grid
    {
        /// <summary>
        /// Gets the length (y-axis) of the <see cref="Grid">grid </see> measured in <see cref="Cell">cells</see>.
        /// </summary>
        public int Length { get; init; }

        /// <summary>
        /// Gets the width (x-axis) of the <see cref="Grid">grid </see> measured in <see cref="Cell">cells</see>.
        /// </summary>
        public int Width { get; init; }

        /// <summary>
        /// Gets the number of mines on the <see cref="Grid">grid</see>.
        /// </summary>
        public int Mines { get; init; }

        /// <summary>
        /// Gets the <see cref="Mines">mine</see> density of the <see cref="Grid">grid</see>.
        /// </summary>
        public double MineDensity
        {
            get
            {
                return (double)this.Mines / (this.Length * this.Width);
            }
        }

        /// <summary>
        /// Gets the <see cref="Cell">cells</see> on the <see cref="Grid">grid</see>.
        /// </summary>
        public List<Cell> Cells { get; init; }

        /// <summary>
        /// Gets a list of all <see cref="Cell">cells</see> with <see cref="Cell.HasMine">mines</see>.
        /// </summary>
        public List<Cell> MinedCells
        {
            get
            {
                return this.Cells.Where(cell => cell.HasMine).ToList();
            }
        }

        /// <summary>
        /// Gets a list of all <see cref="Cell">cells</see> without <see cref="Cell.HasMine">mines</see>.
        /// </summary>
        public List<Cell> SafeCells
        {
            get
            {
                return this.Cells.Where(cell => !cell.HasMine).ToList();
            }
        }

        /// <summary>
        /// Gets a list of all <see cref="Cell">cells</see> that have been flagged; they are assumed to be mined.
        /// </summary>
        public List<Cell> KnownMinedCells
        {
            get
            {
                return this.Cells.Where(cell => cell.HasFlag).ToList();
            }
        }

        /// <summary>
        /// Gets a list of all <see cref="Cell">cells</see> that are open; they are known to be safe.
        /// </summary>
        public List<Cell> KnownSafeCells
        {
            get
            {
                return this.Cells.Where(cell => cell.IsOpen).ToList();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Grid"/> class.
        /// </summary>
        /// <param name="length">The length (y-axis) of the <see cref="Grid">grid </see> measured in <see cref="Cell">cells</see>.</param>
        /// <param name="width">The width (x-axis) of the <see cref="Grid">grid </see> measured in <see cref="Cell">cells</see>.</param>
        /// <param name="mines">The number of mines on the <see cref="Grid">grid</see>.</param>
        public Grid(int length, int width, int mines)
        {
            // Catch invalid parameters.
            Utility.CheckGridParams(length, width, mines);

            // Assign properties.
            this.Length = length;
            this.Width = width;
            this.Mines = mines;
            this.Cells = new();

            // Randomly generate mines.
            bool[] minesArray = new bool[this.Length * this.Width];
            Random rand = new();
            while (mines > 0)
            {
                int randInt = rand.Next(this.Length * this.Width);
                if (!minesArray[randInt])
                {
                    minesArray[randInt] = true;
                    mines--;
                }
            }

            // Create cells.
            for (int index = 0; index < this.Length * this.Width; index++)
            {
                this.Cells.Add(new(this, new(length, width, Utility.CellIndexToCoordinates(index, this.Width)), minesArray[index]));
            }
        }

        /// <summary>
        /// Represents the <see cref="Grid">grid</see> as a <see cref="string"/>.
        /// <see cref="Cell">Cells</see> that are mines are represented by "X"s,
        ///     while other are represented by their <see cref="Cell.MineCount">mine count</see>.
        /// </summary>
        /// <returns>A <see cref="string"/> representation of the <see cref="Grid">grid</see>.</returns>
        public override string ToString()
        {
            string res = string.Empty;

            foreach (Cell cell in this.Cells.OrderBy(i => i.Point.ID))
            {
                if (cell.Point.ID % this.Length == 0 && cell.Point.ID > 0)
                {
                    res += "\n";
                }

                res += cell.HasMine ? "X" : cell.MineCount;
            }

            return res;
        }
    }
}