﻿namespace Minesweeper
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
    }
}
