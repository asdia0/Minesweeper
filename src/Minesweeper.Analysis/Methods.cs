namespace Minesweeper.Analysis
{
    using System.Collections.Generic;
    using System.Linq;

    public class Methods
    {
        public static int IslandCount(Grid grid)
        {
            int count = 0;

            List<Cell> cells = grid.Cells.Where(i => i.Count != 0).ToList();

            List<Cell> searched = new();

            List<Cell> toSearch = new();

            while (searched.Count() != cells.Count())
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
