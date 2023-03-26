namespace Minesweeper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a game of minesweeper.
    /// </summary>
    public class Game
    {
        private DateTime? end = null;

        private DateTime? start = null;

        /// <summary>
        /// Gets the <see cref="Grid">grid</see> the <see cref="Game">game</see> is played on.
        /// </summary>
        public Grid Grid { get; init; }

        /// <summary>
        /// Gets or sets the current <see cref="Game">game</see> <see cref="State">state</see>. Return `null` if the game has not started (no <see cref="Cell">cells</see> have been opened).
        /// </summary>
        public State? State { get; set; } = null;

        /// <summary>
        /// Gets or sets the time at which the game started (A <see cref="Cell">cell</see> has been opened).
        /// Returns `null` if the game has not started.
        /// </summary>
        public DateTime? Start
        {
            get
            {
                return this.start;
            }

            set
            {
                if (this.start == null)
                {
                    this.start = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the time at which the <see cref="Game">game</see> ended (<see cref="State"/> changed to <see cref="State.Success"/> or <see cref="State.Fail"/>).
        /// Returns `null` if the game is still <see cref="State.Ongoing">ongoing</see>.
        /// </summary>
        public DateTime? End
        {
            get
            {
                return this.end;
            }

            set
            {
                if (this.end == null)
                {
                    this.end = value;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Game"/> class.
        /// </summary>
        /// <param name="length">The length (y-axis) of the <see cref="Grid">grid </see> measured in <see cref="Cell">cells</see>.</param>
        /// <param name="width">The width (x-axis) of the <see cref="Grid">grid </see> measured in <see cref="Cell">cells</see>.</param>
        /// <param name="mines">The number of mines on the <see cref="Grid">grid</see>.</param>
        public Game(int length, int width, int mines)
        {
            // Catch invalid parameters.
            Utility.CheckGridParams(length, width, mines);

            this.Grid = new(length, width, mines);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Game"/> class.
        /// </summary>
        /// <param name="grid">The <see cref="Grid"/> the game is played on.</param>
        public Game(Grid grid)
        {
            this.Grid = grid;
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
            if (this.End != null || cell.Grid != this.Grid || cell.IsOpen || cell.HasFlag)
            {
                return;
            }

            // Check if this is the first cell being opened.
            if (this.State == null)
            {
                this.State = Minesweeper.State.Ongoing;
                this.Start = DateTime.Now;

                // First click cannot be a mine.
                if (cell.HasMine)
                {
                    cell.HasMine = false;
                }
            }

            // Open cell.
            cell.IsOpen = true;

            // Go through the outcomes of each various cases.
            switch (cell.MineCount)
            {
                case null:
                    this.State = Minesweeper.State.Fail;
                    return;
                case 0:
                    List<Cell> adjacentCells = cell.AdjacentCells;
                    adjacentCells.ForEach(cell => this.OpenCell(cell));
                    break;
            }

            // Mark game as won if all cells without mines have been opened.
            if (!this.Grid.Cells.Where(cell => !cell.HasMine && !cell.IsOpen).Any())
            {
                this.State = Minesweeper.State.Success;
                this.End = DateTime.Now;
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
    }
}