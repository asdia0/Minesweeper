namespace Minesweeper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a minesweeper grid.
    /// </summary>
    public class Grid
    {
        /// <summary>
        /// Gets the length (y-axis) of the <see cref="Grid">grid </see> measured in <see cref="Cell">cells</see>.
        /// </summary>
        public int Length { get; init; }

        /// <summary>
        /// Gets the width (x-axis) of the <see cref="Grid">grid </see> measured in <see cref="Cell">cells</see>.
        /// </summary>
        public int Width { get; init; }

        /// <summary>
        /// Gets the number of mines on the <see cref="Grid">grid</see>.
        /// </summary>
        public int Mines { get; init; }

        /// <summary>
        /// Gets the <see cref="Mines">mine</see> density of the <see cref="Grid">grid</see>.
        /// </summary>
        public double MineDensity
        {
            get
            {
                return (double)this.Mines / (this.Length * this.Width);
            }
        }

        /// <summary>
        /// Gets the <see cref="Cell">cells</see> on the <see cref="Grid">grid</see>.
        /// </summary>
        public List<Cell> Cells { get; init; }

        /// <summary>
        /// Gets the 3BV of the grid.
        /// 3BV, or Bechtel's Board Benchmark Value, is the minimum number of left clicks required to clear a board.
        /// 3BV = <see cref="OpeningCount">number of openings</see> + all numbers that are "landlocked".
        /// </summary>
        public int BBBV
        {
            get
            {
                return this.Cells.Where(i => i.MineCount > 0 && !i.AdjacentCells.Where(i => i.MineCount == 0).Any()).Count();
            }
        }

        /// <summary>
        /// Gets the number of islands on the <see cref="Grid">grid</see>.
        /// Islands are defined as a group of <see cref="Cell">cells</see> that are numbers or mines that are surrounded by
        ///     openings (cells that are not adjacent to a mine) and the edge of the grid.
        /// Here, we consider two cells as in the same group if they are
        ///     <see cref="Cell.OrthogonallyAdjacentCells"> orthogonally adjacent</see> to each other.
        /// </summary>
        /// <returns>The number of islands on the  <see cref="Grid">grid</see>. The minimum possible value is 1.</returns>
        public int IslandCount
        {
            get
            {
                return this.IslandSizes.Count;
            }
        }

        /// <summary>
        /// Gets the number of openings on a <see cref="Grid">grid</see>.
        /// Openings are defined as a group of <see cref="Cell">cells</see> with a <see cref="Cell.MineCount">mine count</see> of 0.
        /// Here, we consider two openings as in the same group if they are <see cref="Cell.AdjacentCells">adjacent</see> to each other.
        /// </summary>
        /// <returns>The number of islands on the <see cref="Grid">grid</see>. The minimum possible value is 1.</returns>
        public int OpeningCount
        {
            get
            {
                return this.OpeningSizes.Count;
            }
        }

        /// <summary>
        /// Gets a list of the number of <see cref="Cell">cells</see> in each island.
        /// </summary>
        /// <returns>A list of <see cref="int">integers</see>, sorted in decreasing order.</returns>
        public List<int> IslandSizes
        {
            get
            {
                List<int> sizes = new();

                List<Cell> searchSpace = this.Cells.Where(i => i.MineCount != 0).ToList();

                List<Cell> searched = new();

                List<Cell> toSearch = new();

                int size = 0;

                while (searched.Count < searchSpace.Count)
                {
                    Cell seed = null;

                    if (toSearch.Any())
                    {
                        size++;
                        seed = toSearch.First();
                    }
                    else
                    {
                        // add previous island size to sizes
                        if (searched.Count != 0)
                        {
                            sizes.Add(size + 1);
                            size = 0;
                        }

                        // select another seed
                        seed = searchSpace.Except(searched).First();
                    }

                    // do not cocunt mines
                    if (seed.HasMine)
                    {
                        size--;
                    }

                    // add cells if 1) orthogonally adjacent to seed, 2) in cells, 3) not in searched
                    toSearch.AddRange(seed.OrthogonallyAdjacentCells.Intersect(searchSpace).Except(searched).Except(toSearch).ToList());
                    toSearch.Remove(seed);
                    searched.Add(seed);
                }

                sizes.Add(size + 1);
                return sizes.OrderByDescending(i => i).ToList();
            }
        }

        /// <summary>
        /// Gets a list of the number of <see cref="Cell">cells</see> in each opening.
        /// </summary>
        /// <returns>A list of <see cref="int">integers</see>, sorted in decreasing order.</returns>
        public List<int> OpeningSizes
        {
            get
            {
                List<int> sizes = new();

                List<Cell> searchSpace = this.Cells.Where(i => i.MineCount == 0).ToList();

                List<Cell> searched = new();

                List<Cell> toSearch = new();

                int size = 0;

                while (searched.Count < searchSpace.Count)
                {
                    Cell seed = null;

                    if (toSearch.Any())
                    {
                        size++;
                        seed = toSearch.First();
                    }
                    else
                    {
                        // add previous island size to sizes
                        if (searched.Count != 0)
                        {
                            sizes.Add(size + 1);
                            size = 0;
                        }

                        // select another seed
                        seed = searchSpace.Except(searched).First();
                    }

                    // add cells if 1) adjacent to seed, 2) in cells, 3) not in searched
                    toSearch.AddRange(seed.AdjacentCells.Intersect(searchSpace).Except(searched).Except(toSearch).ToList());
                    toSearch.Remove(seed);
                    searched.Add(seed);
                }

                sizes.Add(size + 1);
                return sizes.OrderByDescending(i => i).ToList();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Grid"/> class.
        /// </summary>
        /// <param name="length">The length (y-axis) of the <see cref="Grid">grid </see> measured in <see cref="Cell">cells</see>.</param>
        /// <param name="width">The width (x-axis) of the <see cref="Grid">grid </see> measured in <see cref="Cell">cells</see>.</param>
        /// <param name="mines">The number of mines on the <see cref="Grid">grid</see>.</param>
        public Grid(int length, int width, int mines)
        {
            // Catch invalid parameters.
            Utility.CheckGridParams(length, width, mines);

            // Assign properties.
            this.Length = length;
            this.Width = width;
            this.Mines = mines;
            this.Cells = new();

            // Randomly generate mines.
            bool[] minesArray = new bool[this.Length * this.Width];
            Random rand = new();
            while (mines > 0)
            {
                int randInt = rand.Next(this.Length * this.Width);
                if (!minesArray[randInt])
                {
                    minesArray[randInt] = true;
                    mines--;
                }
            }

            // Create cells.
            for (int index = 0; index < this.Length * this.Width; index++)
            {
                this.Cells.Add(new(this, new(length, width, Utility.CellIndexToCoordinates(index, this.Width)), minesArray[index]));
            }
        }

        /// <summary>
        /// Represents the <see cref="Grid">grid</see> as a <see cref="string"/>.
        /// <see cref="Cell">Cells</see> that are mines are represented by "X"s,
        ///     while other are represented by their <see cref="Cell.MineCount">mine count</see>.
        /// </summary>
        /// <returns>A <see cref="string"/> representation of the <see cref="Grid">grid</see>.</returns>
        public override string ToString()
        {
            string res = string.Empty;

            foreach (Cell cell in this.Cells.OrderBy(i => i.Point.ID))
            {
                if (cell.Point.ID % this.Length == 0 && cell.Point.ID > 0)
                {
                    res += "\n";
                }

                res += cell.HasMine ? "X" : cell.MineCount;
            }

            return res;
        }
    }
}