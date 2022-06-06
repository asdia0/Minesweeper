﻿namespace Minesweeper
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a cell on a <see cref="Grid">grid</see>.
    /// </summary>
    public class Cell
    {
        private bool isOpen = false;

        private bool isOpenSet = false;

        /// <summary>
        /// Gets the <see cref="Grid">grid</see> the <see cref="Cell">cell</see> is on.
        /// </summary>
        public Grid Grid { get; init; }

        /// <summary>
        /// Gets the <see cref="Point">point</see> of the <see cref="Cell">cell</see>.
        /// </summary>
        public Point Point { get; init; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="Cell">cell</see> has a mine.
        /// </summary>
        public bool HasMine { get; init; }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="Cell">cell</see> has been opened by the player. Not reversible.
        /// </summary>
        public bool IsOpen
        {
            get
            {
                return this.isOpen;
            }

            set
            {
                if (!this.isOpenSet)
                {
                    this.isOpen = value;
                    this.isOpenSet = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="Cell">cell</see> has been flagged.
        /// </summary>
        public bool HasFlag { get; set; }

        /// <summary>
        /// Gets a list of <see cref="Cell">cells</see> that are adjacent to the current cell. Cells diagonal to the current cell are considered adjacent.
        /// </summary>
        public List<Cell> AdjacentCells
        {
            get
            {
                // Get a list of the coordinates of adjacent cells.
                List<(int x, int y)> points = this.Point.AdjacentPoints;

                // Convert the coordinates to IDs.
                List<int> ids = points.Select(coor => coor.x + (coor.y * this.Grid.Width)).ToList();

                // Return the list of cells with the mapped IDs.
                return ids.Select(cell => this.Grid.Cells[cell]).ToList();
            }
        }

        /// <summary>
        /// Gets the number of mines in surrounding <see cref="Cell">cells</see>. Returns `null` if the cell itself has a mine.
        /// </summary>
        public int? Count
        {
            get
            {
                if (this.HasMine)
                {
                    return null;
                }

                // Get a list of adjacent cells and count the number of cells that have a mine.
                return this.AdjacentCells.Where(cell => cell.HasMine).Count();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Cell"/> class.
        /// </summary>
        /// <param name="grid">The <see cref="Grid">grid</see> the <see cref="Cell">cell</see> is on.</param>
        /// <param name="point">The <see cref="Point">point</see> of the <see cref="Cell">cell</see>.</param>
        /// <param name="hasMine">A value indicating whether the <see cref="Cell">cell</see> has a mine.</param>
        public Cell(Grid grid, Point point, bool hasMine)
        {
            this.Grid = grid;
            this.Point = point;
            this.HasMine = hasMine;
        }
    }
}