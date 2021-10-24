namespace Minesweeper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Board
    {
        private int Length;

        private int Breadth;

        private List<Cell> Cells = new();

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
                this.Cells.Add(new((minedCells.Contains(i)) ? true : false));
            }
        }

        public int GetCellNumber(int cellID)
        {
            int count = 0;

            List<int> neighbourIDs = new()
            {
                cellID - this.Breadth - 1,
                cellID - this.Breadth,
                cellID - this.Breadth + 1,
                cellID - 1,
                cellID + 1,
                cellID + this.Breadth - 1,
                cellID + this.Breadth,
                cellID + this.Breadth + 1,
            };

            foreach (int id in neighbourIDs.Where(i => (0 <= i) && (i < this.Length * this.Breadth)))
            {
                if (this.Cells[id].HasMine)
                {
                    count++;
                }
            }

            return count;
        }

        public override string ToString()
        {
            string board = string.Empty;

            for (int i = 0; i < this.Length * this.Breadth; i++)
            {
                if ((i + 1) % this.Breadth == 0)
                {
                    board += "\n";
                }

                board += this.GetCellNumber(i);
            }

            return board;
        }
    }
}
