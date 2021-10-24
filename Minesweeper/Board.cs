namespace Minesweeper
{
    using System.Collections.Generic;

    public class Board
    {
        private int Length;

        private int Breadth;

        private List<Cell> Cells = new();

        public Board(int length, int breadth)
        {
            this.Length = length;
            this.Breadth = breadth;

            for (int i = 0; i < length * breadth; i++)
            {
                this.Cells.Add(new());
            }
        }
    }
}
