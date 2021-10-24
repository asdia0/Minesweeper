namespace Minesweeper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Board
    {
        private bool lengthSet = false;

        private bool breadthSet = false;

        private int _Length;

        private int _Breadth;

        public int Length
        {
            get
            {
                return _Length;
            }
            set
            {
                if (!lengthSet)
                {
                    this._Length = value;
                    this.lengthSet = true;
                }
            }
        }

        public int Breadth
        {
            get
            {
                return _Breadth;
            }
            set
            {
                if (!breadthSet)
                {
                    this._Breadth = value;
                    this.breadthSet = true;
                }
            }
        }

        public List<Cell> Cells = new();

        public Board(int length, int breadth, int mines)
        {
            if (mines > length * breadth)
            {
                throw new MinesweeperException("The number of mines must be less than or equal to the number of cells.");
            }

            this.Length = length;
            this.Breadth = breadth;

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

                board += this.Cells[i].HasMine ? "X" : this.Cells[i].CellNumber;
            }

            return board.Trim();
        }
    }
}
