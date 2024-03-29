﻿namespace Minesweeper
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
        /// Gets a list of all <see cref="Cell">cells</see> that have been opened.
        /// </summary>
        public List<Cell> OpenedCells
        {
            get
            {
                return this.Cells.Where(cell => cell.IsOpen).ToList();
            }
        }

        /// <summary>
        /// Gets a list of <see cref="Cell">cells</see> that are neither flagged nor opened.
        /// </summary>
        public List<Cell> UnknownCells
        {
            get
            {
                return this.Cells.Except(this.FlaggedCells).Except(this.OpenedCells).ToList();
            }
        }

        /// <summary>
        /// Gets a list of cells with no adjacent mines.
        /// </summary>
        public List<Cell> Openings
        {
            get
            {
                return this.Cells.Where(cell => cell.MineCount == 0).ToList();
            }
        }

        /// <summary>
        /// Gets a list of safe cells adjacent to an unknown cell.
        /// </summary>
        public List<Cell> BoundaryCells
        {
            get
            {
                return this.OpenedCells.Where(cell => cell.AdjacentCells.Intersect(this.UnknownCells).Any()).ToList();
            }
        }

        /// <summary>
        /// Gets a list of unknown cells adjacent to a boundary cell.
        /// </summary>
        public List<Cell> ExposedCells
        {
            get
            {
                return this.UnknownCells
                    .Where(cell => cell.AdjacentCells.Intersect(this.OpenedCells).Any())
                    .ToList();
            }
        }

        /// <summary>
        /// Gets a list of unknown cells not adjacent to a boundary cell.
        /// </summary>
        public List<Cell> FloatingCells
        {
            get
            {
                return this.UnknownCells.Except(this.ExposedCells).ToList();
            }
        }

        /// <summary>
        /// Gets the <see cref="State">state</see> of the <see cref="Grid">grid</see>.
        /// </summary>
        public State State
        {
            get
            {
                // Game is yet to begin; all cells are unknown.
                if (this.UnknownCells.Count == this.Length * this.Width)
                {
                    return State.ToBegin;
                }

                // Game lost; a mined cell has been opened.
                if (this.OpenedCells.Where(cell => cell.HasMine).Any())
                {
                    return State.Fail;
                }

                // Game won; all safe cells have been opened.
                if (!this.SafeCells.Where(i => !i.IsOpen).Any())
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
            this.Cells = [];

            // Randomly generate mines.
            Random rnd = new();
            List<int> minedIDs = Enumerable.Range(0, this.Length * this.Width).OrderBy(i => rnd.Next()).Take(mines).ToList();

            // Create cells.
            for (int index = 0; index < this.Length * this.Width; index++)
            {
                this.Cells.Add(new(this, new(length, width, Utility.CellIndexToCoordinates(index, this.Width)), minedIDs.Contains(index)));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Grid"/> class.
        /// </summary>
        /// <param name="length">The length (y-axis) of the <see cref="Grid">grid </see> measured in <see cref="Cell">cells</see>.</param>
        /// <param name="width">The width (x-axis) of the <see cref="Grid">grid </see> measured in <see cref="Cell">cells</see>.</param>
        /// <param name="mines">The list of mined cell IDs.</param>
        public Grid(int length, int width, List<int> mines)
        {
            Utility.CheckGridParams(length, width, mines.Count);
            if (mines.Where(i => i > length * width).Any() || mines.Count != mines.ToHashSet().Count)
            {
                throw new Exception();
            }

            this.Length = length;
            this.Width = width;
            this.Mines = mines.Count;
            this.Cells = [];

            for (int index = 0; index < this.Length * this.Width; index++)
            {
                this.Cells.Add(new(this, new(length, width, Utility.CellIndexToCoordinates(index, this.Width)), mines.Contains(index)));
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
                    Random rng = new();
                    this.SafeCells[rng.Next(this.SafeCells.Count)].HasMine = true;

                    cell.HasMine = false;
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
        /// Flags a cell if it is unopened.
        /// </summary>
        /// <param name="cell">The <see cref="Cell">cell</see> to flag.</param>
        public void FlagCell(Cell cell)
        {
            if (!cell.IsOpen)
            {
                cell.HasFlag = true;
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
        public string ToStringKnown()
        {
            string str = string.Empty;

            foreach (Cell cell in this.Cells.OrderBy(i => i.Point.ID))
            {
                if (cell.Point.ID % this.Width == 0 && cell.Point.ID > 0)
                {
                    str += "\n";
                }

                if (cell.HasFlag)
                {
                    str += "F";
                }
                else
                {
                    if (cell.IsOpen)
                    {
                        if (cell.HasMine)
                        {
                            str += "X";
                        }
                        else
                        {
                            str += cell.MineCount;
                        }
                    }
                    else
                    {
                        str += "?";
                    }
                }
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
                if (cell.Point.ID % this.Width == 0 && cell.Point.ID > 0)
                {
                    str += "\n";
                }

                str += cell.HasMine ? "X" : cell.MineCount;
            }

            return str;
        }
    }
}