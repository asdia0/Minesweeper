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
        /// Gets a list of all <see cref="Cell">cells</see> that have been flagged.
        /// </summary>
        public List<Cell> FlaggedCells
        {
            get
            {
                return this.Cells.Where(cell => cell.HasFlag).ToList();
            }
        }

        /// <summary>
        /// Gets a list of all <see cref="Cell">cells</see> that are open.
        /// </summary>
        public List<Cell> OpenedCells
        {
            get
            {
                return this.Cells.Where(cell => cell.IsOpen).ToList();
            }
        }

        /// <summary>
        /// Gets the <see cref="State">state</see> of the <see cref="Grid">grid</see>.
        /// </summary>
        public State State
        {
            get
            {
                // Game is yet to begin; no action has been committed.
                if (!this.FlaggedCells.Union(this.OpenedCells).Any())
                {
                    return State.ToBegin;
                }

                // Game lost; a miend cell has been opened.
                if (this.OpenedCells.Where(cell => cell.HasMine).Any())
                {
                    return State.Fail;
                }

                // Game won; all safe cells have been opened.
                if (this.SafeCells.All(this.OpenedCells.Contains) && this.SafeCells.Count == this.OpenedCells.Count)
                {
                    return State.Success;
                }

                // Game ongoing.
                return State.Ongoing;
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
        /// Opens a <see cref="Cell">cell</see>.
        /// If the cell's <see cref="Cell.MineCount">count</see> is positive, it opens that cell.
        /// If the cell's count is 0, it opens its adjacent cells.
        /// If the cell has a mine, the game ends.
        /// </summary>
        /// <param name="cell">The <see cref="Cell">cell</see> to open.</param>
        public void OpenCell(Cell cell)
        {
            // Skip cell if
            // 1. game has ended (success or fail)
            // 2. cell has already been opened
            // 3. cell is flagged.
            if (this.State == State.Success || this.State == State.Fail || cell.IsOpen || cell.HasFlag)
            {
                return;
            }

            // Check if this is the first cell being opened.
            if (this.State == State.ToBegin)
            {
                // If the first click has a mine, switch it with another cell.
                if (cell.HasMine)
                {
                    cell.HasMine = false;

                    this.SafeCells.First().HasMine = true;
                }
            }

            // Open cell.
            cell.IsOpen = true;

            // End if a mine has been opened.
            if (cell.HasMine)
            {
                return;
            }

            // Open all adjacent cells if clicked cell is an opening.
            if (cell.MineCount == 0)
            {
                cell.AdjacentCells.ForEach(cell => this.OpenCell(cell));
            }
        }

        /// <summary>
        /// Opens a  <see cref="Cell">cell</see> and all adjacent cells if the number of flags surrounding it matches its <see cref="Cell.MineCount">count</see>.
        /// </summary>
        /// <param name="cell">The <see cref="Cell">cell</see> to chord on.</param>
        public void Chord(Cell cell)
        {
            // Only chord on cells with the correct number of flags surrounding it.
            if (cell.MineCount == this.FlaggedCells.Intersect(cell.AdjacentCells).Count())
            {
                // Open cell and its adjacent cells.
                this.OpenCell(cell);
                cell.AdjacentCells.ForEach(adjCell => this.OpenCell(adjCell));
            }
        }

        /// <summary>
        /// Represents the <see cref="Grid">grid</see> as a <see cref="string"/>.
        /// <see cref="Cell">Cells</see> that are flagged are represented by "F"s,
        ///     cells that are  unnknown are represented by "?",
        ///     while others are represented by their <see cref="Cell.MineCount">mine count</see>.
        /// </summary>
        /// <returns>A <see cref="string"/> representation of the <see cref="Grid">grid</see>.</returns>
        public string ShowKnown()
        {
            string str = string.Empty;

            foreach (Cell cell in this.Cells.OrderBy(i => i.Point.ID))
            {
                if (cell.Point.ID % this.Length == 0 && cell.Point.ID > 0)
                {
                    str += "\n";
                }

                str += cell.IsOpen ? cell.MineCount : (cell.HasFlag ? "F" : "?");
            }

            return str;
        }

        /// <summary>
        /// Represents the <see cref="Grid">grid</see> as a <see cref="string"/>.
        /// <see cref="Cell">Cells</see> that are mines are represented by "X"s,
        ///     while others are represented by their <see cref="Cell.MineCount">mine count</see>.
        /// </summary>
        /// <returns>A <see cref="string"/> representation of the <see cref="Grid">grid</see>.</returns>
        public override string ToString()
        {
            string str = string.Empty;

            foreach (Cell cell in this.Cells.OrderBy(i => i.Point.ID))
            {
                if (cell.Point.ID % this.Length == 0 && cell.Point.ID > 0)
                {
                    str += "\n";
                }

                str += cell.HasMine ? "X" : cell.MineCount;
            }

            return str;
        }
    }
}