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
        /// Gets the number of mines on the <see cref="Grid">grid</see>.
        /// </summary>
        public int Mines { get; init; }

        /// <summary>
        /// Gets the <see cref="Cell">cells</see> on the <see cref="Grid">grid</see>.
        /// </summary>
        public List<Cell> Cells { get; init; }

        public int BBBV
        {
            get
            {
                return this.Cells.Where(i => i.MineCount > 0 && !i.AdjacentCells.Where(i => i.MineCount == 0).Any()).Count();
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
            this.Game = null;
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
            // 1. game has ended
            // 2. cell is in incorrect grid
            // 3. cell has already been opened
            // 4. cell is flagged.
            if (this.Game.End != null || cell.Grid != this || cell.IsOpen || cell.HasFlag)
            {
                return;
            }

            // Check if this is the first cell being opened.
            if (!this.Cells.Where(cell => cell.IsOpen).Any())
            {
                this.Game.State = State.Ongoing;
                this.Game.Start = DateTime.Now;
            }

            // Open cell.
            cell.IsOpen = true;

            // Go through the outcomes of each various cases.
            switch (cell.MineCount)
            {
                case null:
                    this.Game.State = State.Fail;
                    return;
                case 0:
                    List<Cell> adjacentCells = cell.AdjacentCells;
                    adjacentCells.ForEach(cell => this.OpenCell(cell));
                    break;
            }

            // Mark game as won if all cells without mines have been opened.
            if (!this.Cells.Where(cell => !cell.HasMine && !cell.IsOpen).Any())
            {
                this.Game.State = State.Success;
                this.Game.End = DateTime.Now;
            }
        }

        /// <summary>
        /// Opens a  <see cref="Cell">cell</see> and all adjacent cells if the number of flags surrounding it matches its <see cref="Cell.MineCount">count</see>.
        /// </summary>
        /// <param name="cell">The <see cref="Cell">cell</see> to chord on.</param>
        public void Chord(Cell cell)
        {
            // Only chord on cells with the correct number of flags surrounding it.
            if (cell.MineCount == cell.AdjacentCells.Where(cell => cell.HasFlag).Count())
            {
                // Open cell and its adjacent cells.
                this.OpenCell(cell);
                cell.AdjacentCells.ForEach(adjCell => this.OpenCell(adjCell));
            }

            return;
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