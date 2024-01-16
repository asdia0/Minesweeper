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
        public List<Constraint> Constraints { get; set; }

        /// <summary>
        /// A list of solutions to the given constraints.
        /// </summary>
        public HashSet<Constraint> Solutions { get; set; }

        /// <summary>
        /// Initalizes a new instance of <see cref="Inferrer"/> class.
        /// </summary>
        /// <param name="grid">The <see cref="Grid"/> to infer from.</param>
        public Inferrer(Grid grid)
        {
            Constraints = [];
            Solutions = [];

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

        /// <summary>
        /// Solves the given constraints. Solutions can be accessed via <see cref="Solutions"/>.
        /// </summary>
        public void Solve()
        {
            bool newConstraintsConstructed = true;

            List<Constraint> oldConstraints = [];

            RemoveUnnecessaryConstraints();

            // Only stop running when no new constraints can be constructed.
            while (newConstraintsConstructed)
            {
                SolveTrivials();
                ConstructConstraints();
                UpdateSolvedVariables();
                RemoveUnnecessaryConstraints();

                bool runTemp = Constraints.Except(oldConstraints).Any();

                oldConstraints = Constraints;

                newConstraintsConstructed = runTemp;
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

                if (constraint.Sum == constraint.Variables.Count)
                {
                    sum = 1;
                }
                else if (constraint.Sum != 0)
                {
                    continue;
                }

                foreach (int variable in constraint.Variables)
                {
                    Constraints.Add(new([variable], sum));
                }

                Constraints.Remove(constraint);
            }
        }

        /// <summary>
        /// Removes all unnecessary constraints.
        /// </summary>
        public void RemoveUnnecessaryConstraints()
        {
            // Remove constraints with no variables
            foreach (Constraint constraint in Constraints.ToList())
            {
                if (constraint.Variables.Count == 0)
                {
                    Constraints.Remove(constraint);
                }
            }

            // Remove duplicate constraints
            int constraintsCount = Constraints.Count;
            List<Constraint> tempConstraints = [.. Constraints];

            for (int i = 0; i < constraintsCount; i++)
            {
                for (int j = i + 1; j < constraintsCount; j++)
                {
                    if (tempConstraints[i] == tempConstraints[j])
                    {
                        Constraints.Remove(tempConstraints[i]);
                    }
                }
            }
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

                    if (X.Variables.IsSupersetOf(Y.Variables))
                    {
                        Constraints.Add(new(X.Variables.Except(Y.Variables).ToHashSet(), X.Sum - Y.Sum));
                    }
                }
            }
        }

        /// <summary>
        /// Add solved constraints to <see cref="Solutions"/>.
        /// </summary>
        public void UpdateSolvedVariables()
        {
            foreach (Constraint constraint in Constraints.Where(i => i.IsSolved))
            {
                Solutions.Add(constraint);
            }
        }
    }
}
