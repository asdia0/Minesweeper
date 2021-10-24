namespace Minesweeper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Board
    {
        private int _Length;

        private int _Breadth;

        private int _MineCount;

        public int Length
        {
            get
            {
                return this._Length;
            }
        }

        public int Breadth
        {
            get
            {
                return this._Breadth;
            }
        }

        public int MineCount
        {
            get
            {
                return this._MineCount;
            }
        }

        /// <summary>
        /// Gets a value determining if the player has hit a mine.
        /// </summary>
        public bool IsOver
        {
            get
            {
                return this.Cells.Where(i => i.IsSearched && i.HasMine).Any();
            }
        }

        /// <summary>
        /// Gets a value determining if the player has successfully completed the game.
        /// </summary>
        public bool IsFinished
        {
            get
            {
                if (this.Cells.Where(i => i.IsSearched && !i.HasMine).ToList().Count == this.Length * this.Breadth - this.MineCount)
                {
                    return true;
                }
                return false;
            }
        }

        public List<Cell> Cells = new();

        public Board(int length, int breadth, int mines)
        {
            if (mines > length * breadth)
            {
                throw new MinesweeperException("The number of mines must be less than or equal to the number of cells.");
            }

            this._Length = length;
            this._Breadth = breadth;
            this._MineCount = mines;

            List<int> minedCells = new();
            Random rnd = new();
            while (minedCells.Count != mines)
            {
                int randomID = rnd.Next(0, this.Length * this.Breadth);

                if (!minedCells.Contains(randomID))
                {
                    minedCells.Add(randomID);
                }
            }

            for (int i = 0; i < length * breadth; i++)
            {
                this.Cells.Add(new(this, i, (minedCells.Contains(i)) ? true : false));
            }
        }

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
