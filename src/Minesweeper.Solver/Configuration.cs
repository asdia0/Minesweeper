using System;
using System.Collections.Generic;
using System.Linq;

namespace Minesweeper.Solver
{
    /// <summary>
    /// Represents a configuration of the minedness of a list of <see cref="Cell">cells</see>.
    /// </summary>
    public struct Configuration
    {
        /// <summary>
        /// Represents the minedness of each cell.
        /// </summary>
        public Dictionary<int, int?> Assignments { get; set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="Configuration"/> struct from a list of solved constraints.
        /// </summary>
        /// <param name="IDs">The IDs of the exposed cells.</param>
        /// <param name="solutions">A list of solved constraints.</param>
        public Configuration(List<int> IDs, HashSet<Constraint> solutions)
        {
            this.Assignments = [];

            foreach (int variable in IDs)
            {
                this.Assignments.Add(variable, null);
            }

            foreach (Constraint constraint in solutions)
            {
                this.Assignments[constraint.Variables.First()] = constraint.Sum;
            }
        }

        /// <summary>
        /// Gets the number of mined cells in the configuration.
        /// </summary>
        public readonly int Sum
        {
            get
            {
                return this.Assignments.Where(i => i.Value == 1).Count();
            }
        }

        /// <summary>
        /// Checks if the minedness of all cells have been assigned.
        /// </summary>
        public readonly bool IsSolved
        {
            get
            {
                return !this.Assignments.Where(i => i.Value == null).Any();
            }
        }

        public static Configuration operator *(Configuration left, Configuration right)
        {
            return new Configuration()
            {
                Assignments = left.Assignments.Concat(right.Assignments).ToDictionary(i => i.Key, i => i.Value)
            };
        }

        public static Configuration operator +(Configuration left, Configuration right)
        {
            return new Configuration()
            {
                Assignments = left.Assignments.ToDictionary(orig => orig.Key, orig => orig.Value + right.Assignments[orig.Key])
            };
        }
    }
}
