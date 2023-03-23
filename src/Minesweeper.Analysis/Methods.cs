namespace Minesweeper.Analysis
{
    using System.Collections.Generic;
    using System.Linq;

    public class Methods
    {
        /// <summary>
        /// Counts the number of islands on a <see cref="Grid">grid</see>.
        /// Islands are defined as a group of <see cref="Cell">cells</see> that are numbers or mines that are surrounded by
        ///     openings (cells that are not adjacent to a mine) and the edge of the grid.
        /// Here, we consider two cells as in the same group if they are
        ///     <see cref="Cell.OrthogonallyAdjacentCells"> orthogonally adjacent</see> to each other.
        /// </summary>
        /// <param name="grid">The <see cref="Grid">grid</see> to calculate.</param>
        /// <returns>The number of islands on the given <see cref="Grid">grid</see>. The minimum possible value is 1.</returns>
        public static int IslandCount(Grid grid)
        {
            int count = 0;

            List<Cell> searchSpace = grid.Cells.Where(i => i.MineCount != 0).ToList();

            List<Cell> searched = new();

            List<Cell> toSearch = new();

            while (searched.Count != searchSpace.Count)
            {
                Cell seed = null;

                if (toSearch.Any())
                {
                    seed = toSearch.First();
                }
                else
                {
                    count++;
                    // select another seed
                    seed = searchSpace.Except(searched).First();
                }

                // add cells if 1) orthogonally adjacent to seed, 2) in cells, 3) not in searched
                toSearch.AddRange(seed.OrthogonallyAdjacentCells.Intersect(searchSpace).Except(searched).Except(toSearch).ToList());
                toSearch.Remove(seed);
                searched.Add(seed);
            }

            return count;
        }

        /// <summary>
        /// Counts the number of openings on a <see cref="Grid">grid</see>.
        /// Openings are defined as a group of <see cref="Cell">cells</see> with a <see cref="Cell.MineCount">mine count</see> of 0.
        /// Here, we consider two openings as in the same group if they are <see cref="Cell.AdjacentCells">adjacent</see> to each other.
        /// </summary>
        /// <param name="grid">The <see cref="Grid">grid</see> to calculate.</param>
        /// <returns>The number of islands on the given <see cref="Grid">grid</see>. The minimum possible value is 1.</returns>
        public static int OpeningCount(Grid grid)
        {
            int count = 0;

            List<Cell> searchSpace = grid.Cells.Where(i => i.MineCount == 0).ToList();

            List<Cell> searched = new();

            List<Cell> toSearch = new();

            while (searched.Count != searchSpace.Count)
            {
                Cell seed = null;

                if (toSearch.Any())
                {
                    seed = toSearch.First();
                }
                else
                {
                    count++;
                    // select another seed
                    seed = searchSpace.Except(searched).First();
                }

                // add cells if 1) adjacent to seed, 2) in cells, 3) not in searched
                toSearch.AddRange(seed.AdjacentCells.Intersect(searchSpace).Except(searched).Except(toSearch).ToList());
                toSearch.Remove(seed);
                searched.Add(seed);
            }

            return count;
        }

        public static List<int> IslandSizes(Grid grid)
        {
            List<int> sizes = new();

            List<Cell> searchSpace = grid.Cells.Where(i => i.MineCount != 0).ToList();

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
                        sizes.Add(size);
                        size = 0;
                    }

                    // select another seed
                    seed = searchSpace.Except(searched).First();
                }

                // add cells if 1) orthogonally adjacent to seed, 2) in cells, 3) not in searched
                toSearch.AddRange(seed.OrthogonallyAdjacentCells.Intersect(searchSpace).Except(searched).Except(toSearch).ToList());
                toSearch.Remove(seed);
                searched.Add(seed);
            }

            sizes.Add(size);
            return sizes.OrderByDescending(i => i).ToList();
        }
    }
}
