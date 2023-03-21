namespace Minesweeper.Analysis
{
    using System.Collections.Generic;
    using System.Linq;

    public class Methods
    {
        /// <summary>
        /// Counts the number of islands on a <see cref="Grid">grid</see>.
        /// Islands are defined as a group of cells that are numbers or mines that are surrounded by
        ///     openings (cells that are not adjacent to a mine) and the edge of the grid.
        /// </summary>
        /// <param name="grid">The <see cref="Grid">grid</see> to calculate.</param>
        /// <returns>The number of islands on the given <see cref="Grid">grid</see>. The minimum possible value is 1.</returns>
        public static int IslandCount(Grid grid)
        {
            int count = 0;

            List<Cell> cells = grid.Cells.Where(i => i.MineCount != 0).ToList();

            List<Cell> searched = new();

            List<Cell> toSearch = new();

            while (searched.Count != cells.Count)
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
                    seed = cells.Except(searched).First();
                }

                // add cells if 1) orthogonally adjacent to seed, 2) in cells, 3) not in searched
                toSearch.AddRange(seed.OrthogonallyAdjacentCells.Intersect(cells).Except(searched).Except(toSearch).ToList());
                toSearch.Remove(seed);
                searched.Add(seed);
            }

            return count;
        }
    }
}
