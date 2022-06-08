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
        /// Gets the <see cref="Game"/> the grid is in.
        /// </summary>
        public Game Game { get; init; }

        /// <summary>
        /// Gets the length (y-axis) of the <see cref="Grid">grid </see> measured in <see cref="Cell">cells</see>.
        /// </summary>
        public int Length { get; init; }

        /// <summary>
        /// Gets the width (x-axis) of the <see cref="Grid">grid </see> measured in <see cref="Cell">cells</see>.
        /// </summary>
        public int Width { get; init; }

        /// <summary>
        /// Gets the <see cref="Cell">cells</see> on the <see cref="Grid">grid</see>.
        /// </summary>
        public List<Cell> Cells { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Grid"/> class.
        /// </summary>
        /// <param name="game">The game the <see cref="Grid">grid</see> is in.</param>
        /// <param name="length">The length (y-axis) of the <see cref="Grid">grid </see> measured in <see cref="Cell">cells</see>.</param>
        /// <param name="width">The width (x-axis) of the <see cref="Grid">grid </see> measured in <see cref="Cell">cells</see>.</param>
        /// <param name="mines">The number of mines on the <see cref="Grid">grid</see>.</param>
        public Grid(Game game, int length, int width, int mines)
        {
            // Catch invalid parameters.
            Utility.CheckGridParams(length, width, mines);

            // Assign properties.
            this.Game = game;
            this.Length = length;
            this.Width = width;
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
        /// If the cell's <see cref="Cell.Count">count</see> is positive, it opens that cell.
        /// If the cell's count is 0, it opens its adjacent cells.
        /// If the cell has a mine, the game ends.
        /// </summary>
        /// <param name="cell">The <see cref="Cell">cell</see> to open.</param>
        public void OpenCell(Cell cell)
        {
            // Check if game has ended.
            if (this.Game.State != State.Ongoing)
            {
                return;
            }

            // Check if cell is in correct grid.
            if (cell.Grid != this)
            {
                return;
            }

            // Check if cell has already been opened.
            if (cell.IsOpen)
            {
                return;
            }

            // Check if this is the first cell being opened.
            if (this.Cells.Where(cell => cell.IsOpen).Any())
            {
                this.Game.Start = DateTime.Now;
            }

            // Open cell.
            cell.IsOpen = true;

            // Go through the outcomes of each various cases.
            switch (cell.Count)
            {
                case null:
                    this.Game.State = State.Fail;
                    return;
                case 0:
                    List<Cell> adjacentCells = cell.AdjacentCells;
                    adjacentCells.ForEach(cell => this.OpenCell(cell));
                    return;
            }
        }
    }
}