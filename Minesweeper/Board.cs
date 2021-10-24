namespace Minesweeper
{
    using System;
    using System.Collections.Generic;

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
                return this._Length;
            }

            set
            {
                if (!this.lengthSet)
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
                return this._Breadth;
            }

            set
            {
                if (!this.breadthSet)
                {
                    this._Breadth = value;
                    this.breadthSet = true;
                }
            }
        }

        public List<Cell> Cells { get; set; }

        public Board(int length, int breadth)
        {
            this.Length = length;
            this.Breadth = breadth;

            for (int i = 0; i < length * breadth; i++)
            {
                this.Cells = new();
                this.Cells.Add(new());
            }
        }
    }
}
