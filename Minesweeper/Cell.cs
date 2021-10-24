namespace Minesweeper
{
    using System.Collections.Generic;

    public class Cell
    {
        private bool IDSet = false;

        private int _ID;

        public bool IsFlagged = false;

        public bool HasMine;

        public bool IsSearched = false;

        public Board Board;

        public List<Cell> Neighbours
        {
            get
            {
                List<Cell> results = new();

                (int x, int y) = this.Position;

                List<(int, int)> candidates = new()
                {
                    (x - 1, y - 1),
                    (x, y - 1),
                    (x + 1, y - 1),
                    (x - 1, y),
                    (x + 1, y),
                    (x - 1, y + 1),
                    (x, y + 1),
                    (x + 1, y + 1),
                };

                foreach ((int, int) position in candidates)
                {
                    foreach (Cell candidateCell in this.Board.Cells)
                    {
                        if (candidateCell.Position == position)
                        {
                            results.Add(candidateCell);
                        }
                    }
                }

                return results;
            }
        }

        public int CellNumber
        {
            get
            {
                int counter = 0;

                foreach (Cell cell in this.Neighbours)
                {
                    if (cell.HasMine)
                    {
                        counter++;
                    }
                }

                return counter;
            }
        }

        public int ID
        {
            get
            {
                return this._ID;
            }
            set
            {
                if (!this.IDSet)
                {
                    this._ID = value;
                    this.IDSet = true;
                }
            }
        }

        public (int, int) Position
        {
            get
            {
                int row = this.ID / this.Board.Breadth;
                int column = this.ID % this.Board.Breadth;

                return (row, column);
            }
        }

        public Cell(Board board, int id, bool hasMine)
        {
            this.Board = board;
            this.ID = id;
            this.HasMine = hasMine;
        }

        public void SwitchFlagState()
        {
            this.IsFlagged ^= true;
        }
    }
}