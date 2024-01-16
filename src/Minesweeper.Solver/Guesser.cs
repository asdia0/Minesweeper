using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks.Dataflow;
using Newtonsoft.Json;

namespace Minesweeper.Solver
{
    public class Guesser
    {
        public Grid Grid { get; set; }

        public List<Constraint> Constraints { get; set; }

        public Guesser(Grid grid, List<Constraint> constraints)
        {
            // Ensure that grid has no more logic
            this.Grid = grid;
            this.Constraints = new();

            foreach (Constraint constraint in constraints)
            {
                if (!constraint.Variables.Except(grid.ExposedCells.Select(i => i.Point.ID)).Any())
                {
                    Constraints.Add(constraint);
                }
            }
        }

        public List<List<Constraint>> GetConstraintGroups(List<Constraint> constraints)
        {
            List<List<Constraint>> groups = new();

            List<Constraint> remainingConstraints = constraints.ToList();

            List<Constraint> toSearch = new();
            List<Constraint> group = new();
            List<Constraint> searched = new();

            while(remainingConstraints.Count > 0)
            {
                Constraint seed = null;

                if (toSearch.Any())
                {
                    seed = toSearch.First();
                }
                else
                {
                    groups.Add(group);
                    group = new();
                    seed = remainingConstraints.First();
                }

                foreach (int id in seed.Variables)
                {
                    toSearch.AddRange(remainingConstraints.Where(i => i.Variables.Contains(id)));
                }

                toSearch = toSearch.Distinct().ToList();
                toSearch = toSearch.Except(searched).ToList();

                group.Add(seed);
                group = group.Distinct().ToList();

                searched.Add(seed);
                remainingConstraints.Remove(seed);
            }

            groups.Add(group);

            //foreach (List<Constraint> conGroup in groups)
            //{
            //    Console.WriteLine(string.Join(", ", conGroup) + "\n");
            //}
            //Console.WriteLine();

            return groups;
        }
    }
}
