namespace Minesweeper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a grid on which a game of minesweeper is played on.
    /// </summary>
    public class Grid
    {
        private int _length;

        private int _breadth;

        private int _mineCount;

        /// <summary>
        /// Gets the length of the <see cref="Grid"/>, measured in <see cref="Cell"/>s.
        /// In this case, length refers to the vertical axis.
        /// </summary>
        public int Length
        {
            get
            {
                return this._length;
            }
        }

        /// <summary>
        /// Gets the breadth of the <see cref="Grid"/>, measured in <see cref="Cell"/>s.
        /// In this case, breadth refers to the horizontal axis.
        /// </summary>
        public int Breadth
        {
            get
            {
                return this._breadth;
            }
        }

        /// <summary>
        /// Gets the number of mines on the <see cref="Grid"/>.
        /// </summary>
        public int MineCount
        {
            get
            {
                return this._mineCount;
            }
        }

        /// <summary>
        /// Gets or sets a list of the <see cref="Cell"/>s on the <see cref="Grid"/>.
        /// </summary>
        public List<Cell> Cells { get; set; }

        /// <summary>
        /// Gets a value indicating whether the player has hit a mine.
        /// </summary>
        public bool IsOver
        {
            get
            {
                return this.Cells.Where(i => i.IsSearched && i.HasMine).Any();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the player has successfully completed the game.
        /// </summary>
        public bool IsFinished
        {
            get
            {
                if (this.Cells.Where(i => i.IsSearched && !i.HasMine).ToList().Count == (this.Length * this.Breadth) - this.MineCount)
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Grid"/> class.
        /// </summary>
        /// <param name="length">The length of the <see cref="Grid"/>, measured in <see cref="Cell"/>s. Length refers to the vertical axis.</param>
        /// <param name="breadth">The breadth of the <see cref="Grid"/>, measured in <see cref="Cell"/>s. Breadth refers to the horizontal axis.</param>
        /// <param name="mines">The number of mines on the <see cref="Grid"/>.</param>
        public Grid(int length, int breadth, int mines)
        {
            if (mines > length * breadth)
            {
                throw new MinesweeperException("The number of mines must be less than or equal to the number of cells.");
            }

            this._length = length;
            this._breadth = breadth;
            this._mineCount = mines;

            List<int> cellsWithMine = new();
            Random rnd = new();
            while (cellsWithMine.Count != mines)
            {
                int randomID = rnd.Next(0, this.Length * this.Breadth);

                if (!cellsWithMine.Contains(randomID))
                {
                    cellsWithMine.Add(randomID);
                }
            }

            for (int i = 0; i < length * breadth; i++)
            {
                this.Cells.Add(new(this, i, cellsWithMine.Contains(i)));
            }
        }

        /// <summary>
        /// Converts the <see cref="Grid"/> to a <see cref="string"/>.
        /// </summary>
        /// <returns><see cref="Cells"/> as a string.
        /// A searched square that does not have a mine is represented by its <see cref="Cell.CellNumber"/>.
        /// A searched square that has a mine is represented by an "X".
        /// An unsearched square is represented by a ".".</returns>
        public override string ToString()
        {
            string board = string.Empty;

            for (int i = 0; i < this.Length * this.Breadth; i++)
            {
                if (i % this.Breadth == 0)
                {
                    board += "\n";
                }

                Cell cell = this.Cells[i];

                if (cell.IsSearched)
                {
                    if (cell.HasMine)
                    {
                        board += "X";
                    }
                    else
                    {
                        board += cell.CellNumber;
                    }
                }
                else
                {
                    board += ".";
                }
            }

            return board.Trim();
        }
    }
}
