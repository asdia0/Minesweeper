using System;
using System.Collections.Generic;
using System.Linq;

namespace Minesweeper.Solver
{
    public class Inferrer
    {
        /// <summary>
        /// A list of <see cref="Constraint"/>s to infer from.
        /// </summary>
        public HashSet<Constraint> Constraints { get; set; }

        /// <summary>
        /// A list of solutions to the given constraints.
        /// </summary>
        public HashSet<Constraint> Solutions
        {
            get
            {
                return this.Constraints.Where(i => i.IsSolved).ToHashSet();
            }
        }

        public bool Updated { get; set; } = false;

        /// <summary>
        /// Initalizes a new instance of <see cref="Inferrer"/> class.
        /// </summary>
        /// <param name="grid">The <see cref="Grid"/> to infer from.</param>
        public Inferrer(Grid grid)
        {
            Constraints = [];
            //Solutions = [];

            // Set up local constraints
            foreach (Cell boundaryCell in grid.BoundaryCells)
            {
                HashSet<int> cellVariables = boundaryCell.AdjacentCells
                    .Intersect(grid.UnknownCells)
                    .Select(i => i.Point.ID)
                    .ToHashSet();
                Constraints.Add(new(cellVariables, (int)boundaryCell.MineCount - boundaryCell.AdjacentCells.Intersect(grid.FlaggedCells).Count()));
            }

            // Set up global constraint
            HashSet<int> unknownCellVariables = grid.UnknownCells
                .Select(i => i.Point.ID)
                .ToHashSet();
            Constraints.Add(new(unknownCellVariables, grid.Mines - grid.FlaggedCells.Count));
        }

        public Inferrer(HashSet<Constraint> constraints)
        {
            this.Constraints = constraints;
            //this.Solutions = [];
        }

        /// <summary>
        /// Solves the given constraints. Solutions can be accessed via <see cref="Solutions"/>.
        /// </summary>
        public void Solve()
        {
            bool run = true;

            HashSet<Constraint> oldConstraints = [];
            int oldSolutionCount = 0;

            RemoveUnnecessaryConstraints();

            // Only stop running when no new constraints can be constructed.
            while (run)
            {
                Console.WriteLine(this.Solutions.Count);

                SolveTrivials();
                ConstructConstraints();
                UpdateSolvedVariables();
                RemoveUnnecessaryConstraints();

                run = this.Updated;

                this.Updated = false;
            }
        }

        /// <summary>
        /// Sets all variables in a constraint to be safe or mined, depending on <see cref="Constraint.Sum"/>.
        /// </summary>
        public void SolveTrivials()
        {
            foreach (Constraint constraint in Constraints.ToList())
            {
                int sum = 0;
                
                if (constraint.Sum != 0)
                {
                    continue;
                }
                else if (constraint.Sum == constraint.Variables.Count)
                {
                    sum = 1;
                }

                foreach (int variable in constraint.Variables)
                {
                    Constraints.Add(new([variable], sum));
                }

                Constraints.Remove(constraint);

                this.Updated = true;
            }
        }

        /// <summary>
        /// Removes all unnecessary constraints.
        /// </summary>
        public void RemoveUnnecessaryConstraints()
        {
            // Remove constraints with no variables
            Constraints.RemoveWhere(i => i.Variables.Count == 0);
        }

        /// <summary>
        /// Constructs new constraints based on the existing constraints.
        /// For example, the constraints A+B=1 and A=1 will result in a new constraint B=0.
        /// </summary>
        public void ConstructConstraints()
        {
            int constraintCount = Constraints.Count;

            List<Constraint> constraintList = [.. Constraints];

            for (int i = 0; i < constraintCount; i++)
            {
                for (int j = 0; j < constraintCount; j++)
                {
                    Constraint X = constraintList[i];
                    Constraint Y = constraintList[j];

                    if (X.Variables.IsSupersetOf(Y.Variables) && X != Y)
                    {
                        Constraints.Add(new(X.Variables.Except(Y.Variables).ToHashSet(), X.Sum - Y.Sum));
                    }
                }
            }

            if (this.Constraints.Count > constraintCount)
            {
                this.Updated = true;
            }
        }

        /// <summary>
        /// Add solved constraints to <see cref="Solutions"/>.
        /// </summary>
        public void UpdateSolvedVariables()
        {
            foreach (Constraint constraint in Constraints.Where(i => i.IsSolved))
            {
                this.Updated = true;
                Solutions.Add(constraint);
            }
        }
    }
}
