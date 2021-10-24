namespace Minesweeper
{
    using System.Collections.Generic;
    using System.Linq;

    public class Cell
    {
        private Board _Board;

        private int _ID;

        private bool _HasMine;

        private bool _IsVisible = false;

        public Board Board
        {
            get
            {
                return _Board;
            }
        }

        public int ID
        {
            get
            {
                return this._ID;
            }
        }

        public bool HasMine
        {
            get
            {
                return this._HasMine;
            }
        }

        public bool IsSearched
        {
            get
            {
                return this._IsVisible;
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

        public (int, int) Position
        {
            get
            {
                int row = this.ID / this.Board.Breadth;
                int column = this.ID % this.Board.Breadth;

                return (row, column);
            }
        }

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

        public Cell(Board board, int id, bool hasMine)
        {
            this._Board = board;
            this._ID = id;
            this._HasMine = hasMine;
        }

        public void Search()
        {
            this._IsVisible = true;

            if (this.CellNumber == 0 && !this.HasMine)
            {
                List<Cell> candidates = this.Neighbours.ToList();

                while (candidates.Count != 0)
                {
                    Cell cell = candidates[0];

                    if (!cell.IsSearched && !cell.HasMine)
                    {
                        cell._IsVisible = true;

                        if (cell.CellNumber == 0)
                        {
                            candidates.AddRange(cell.Neighbours);
                        }
                    }

                    candidates.Remove(cell);
                }
            }
        }
    }
}