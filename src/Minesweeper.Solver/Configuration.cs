using System.Collections.Generic;
using System.Linq;

namespace Minesweeper.Solver
{
    public struct Configuration
    {
        public Dictionary<int, int?> Assignments { get; set; }

        public Configuration(Configuration config1, Configuration config2)
        {
            foreach (KeyValuePair<int, int?> pair in config2.Assignments)
            {
                config1.Assignments[pair.Key] = pair.Value;
            }
            this.Assignments = config1.Assignments;
        }

        public Configuration(List<Constraint> constraints, List<Constraint> solutions)
        {
            this.Assignments = new();

            List<int> variables = constraints.SelectMany(i => i.Variables).ToList();

            foreach (int variable in variables.Distinct())
            {
                if (solutions.Where(i => i.Variables.Contains(variable)).Any())
                {
                    Assignments.Add(variable, solutions.Where(i => i.Variables.Contains(variable)).First().Sum);
                }
                else
                {
                    Assignments.Add(variable, null);
                }
            }
        }

        public bool IsSolved
        {
            get
            {
                return !this.Assignments.Values.Where(i => i == null).Any();
            }
        }
    }
}
