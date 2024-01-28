using System;
using System.Collections.Generic;
using System.Linq;

namespace Minesweeper.Solver
{
    public struct Configuration
    {
        public Dictionary<int, int?> Assignments { get; set; }

        public Configuration(List<int> variables, HashSet<Constraint> solutions)
        {
            this.Assignments = new();

            foreach (int variable in variables)
            {
                this.Assignments.Add(variable, null);
            }

            foreach (Constraint constraint in solutions)
            {
                this.Assignments[constraint.Variables.First()] = constraint.Sum;
            }
        }

        public int Sum
        {
            get
            {
                return this.Assignments.Where(i => i.Value == 1).Count();
            }
        }

        public bool IsSolved
        {
            get
            {
                return this.Assignments.Where(i => i.Value == null).Count() == 0;
            }
        }

        public static Configuration operator +(Configuration left, Configuration right)
        {
            Configuration sum = new();
            sum.Assignments = left.Assignments.Concat(right.Assignments).ToDictionary(i => i.Key, i => i.Value);
            return sum;
        }
    }
}
