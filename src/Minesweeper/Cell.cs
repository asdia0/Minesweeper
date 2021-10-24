namespace Minesweeper
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represnts a cell on a <see cref="Grid"/>.
    /// </summary>
    public class Cell
    {
        private Grid _board;

        private int _id;

        private bool _hasMine;

        private bool _isSearched = false;

        /// <summary>
        /// Gets the <see cref="Grid"/> the <see cref="Cell"/> is on.
        /// </summary>
        public Grid Board
        {
            get
            {
                return this._board;
            }
        }

        /// <summary>
        /// Gets the <see cref="Cell"/>'s >unique indentification number.
        /// </summary>
        public int ID
        {
            get
            {
                return this._id;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="Cell"/> has a mine.
        /// </summary>
        public bool HasMine
        {
            get
            {
                return this._hasMine;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="Cell"/> has been searched.
        /// </summary>
        public bool IsSearched
        {
            get
            {
                return this._isSearched;
            }
        }

        /// <summary>
        /// Gets the number of <see cref="Neighbours"/> that have a mine.
        /// </summary>
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

        /// <summary>
        /// Gets the <see cref="Cell"/>'s coordinates as (x,y), where x is the horizontal axis and y is the vertical axis. (0,0) is at the top-left corner.
        /// </summary>
        public (int, int) Coordinates
        {
            get
            {
                int row = this.ID / this.Board.Breadth;
                int column = this.ID % this.Board.Breadth;

                return (row, column);
            }
        }

        /// <summary>
        /// Gets a list of neighbouring <see cref="Cells"/>.
        /// </summary>
        public List<Cell> Neighbours
        {
            get
            {
                List<Cell> results = new();

                (int x, int y) = this.Coordinates;

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
                        if (candidateCell.Coordinates == position)
                        {
                            results.Add(candidateCell);
                        }
                    }
                }

                return results;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Cell"/> class.
        /// </summary>
        /// <param name="board">The <see cref="Board"/> the <see cref="Cell"/> is on.</param>
        /// <param name="id">The <see cref="Cell"/>'s unique identification number.</param>
        /// <param name="hasMine">A value indicating whether the <see cref="Cell"/> has a mine.</param>
        public Cell(Grid board, int id, bool hasMine)
        {
            this._board = board;
            this._id = id;
            this._hasMine = hasMine;
        }

        /// <summary>
        /// Searches the <see cref="Cell"/>. This causes <see cref="IsSearched"/> to become `true`.
        /// Furthermore, if <see cref="CellNumber"/> is `0`, then the cell's <see cref="Neighbours"/>
        /// that are not mines will also be considered to be searched. This continues if a neighbouring
        /// cell's <see cref="CellNumber"/> is also `0`.
        /// </summary>
        public void Search()
        {
            this._isSearched = true;

            if (this.CellNumber == 0 && !this.HasMine)
            {
                List<Cell> candidates = this.Neighbours.ToList();

                while (candidates.Count != 0)
                {
                    Cell cell = candidates[0];

                    if (!cell.IsSearched && !cell.HasMine)
                    {
                        cell._isSearched = true;

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