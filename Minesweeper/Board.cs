namespace Minesweeper
{
    using System;
    using System.Collections.Generic;

    public class Board
    {
        private int Length;

        private int Breadth;

        private List<Cell> Cells = new();

        public Board(int length, int breadth, int mines)
        {
            this.Length = length;
            this.Breadth = breadth;

            List<int> minedCells = new();
            Random rnd = new();
            while (minedCells.Count != mines)
            {
                minedCells.Add(rnd.Next(0, this.Length * this.Breadth));
            }


            for (int i = 0; i < length * breadth; i++)
            {
                this.Cells.Add(new((minedCells.Contains(i)) ? true : false));
            }
        }
    }
}
