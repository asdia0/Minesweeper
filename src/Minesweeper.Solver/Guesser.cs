using System;
using System.Collections.Generic;

namespace Minesweeper.Solver
{
    public class Guesser
    {
        public Grid Grid { get; set; }

        public Guesser(Grid grid)
        {
            this.Grid = grid;
        }

        public List<List<Constraint>> GetBoarderContinuations()
        {
            List<List<Constraint>> results = new();

            int maxMines = Math.Min(Grid.Mines - Grid.FlaggedCells.Count, Grid.ExposedCells.Count);

            for (int i = 1; i <= maxMines; i++)
            {
                Inferrer inferrer = new(this.Grid);
            }

            return results;
        }
    }
}
