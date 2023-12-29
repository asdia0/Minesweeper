using System;
using System.Collections.Generic;
using System.Linq;

namespace Minesweeper.Solver
{
    public class Solver
    {
        /// <summary>
        /// A list of <see cref="Constraint"/>s to infer from.
        /// </summary>
        public List<Constraint> Constraints { get; set; }

        /// <summary>
        /// A list of solutions to the given constraints.
        /// </summary>
        public List<Solution> Solutions { get; set; }

        public Solver(Grid grid)
        {
            Constraints = [];
            Solutions = [];

            // Set up local constraints
            foreach (Cell boundaryCell in grid.OpenedCells.Where(i => i.AdjacentCells.Intersect(grid.UnknownCells).Any()))
            {
                HashSet<int> cellVariables = boundaryCell.AdjacentCells
                    .Intersect(grid.UnknownCells)
                    .Select(i => i.Point.ID)
                    .ToHashSet();
                Constraints.Add(new(cellVariables, (int)boundaryCell.MineCount - boundaryCell.AdjacentCells.Intersect(grid.FlaggedCells).Count()));
            }

            // Set up global constraint (check if minecounting available)
        }

        /// <summary>
        /// Solves the given constraints. Solutions can be accessed via <see cref="Solutions"/>.
        /// </summary>
        public void Solve()
        {
            bool run = true;

            List<Constraint> oldConstraints = [];

            RemoveUnnecessaryConstraints();

            // Only stop running when no new constraints can be constructed.
            while (run)
            {
                AllSafeOrMined();
                ConstructNewConstraints();
                UpdateSolvedVariables();
                RemoveUnnecessaryConstraints();

                bool runTemp = Constraints.Except(oldConstraints).Any();

                oldConstraints = Constraints;

                run = runTemp;
            }
        }

        /// <summary>
        /// Sets all variables in a constraint to be safe or mined, depending on <see cref="Constraint.Sum"/>.
        /// </summary>
        public void AllSafeOrMined()
        {
            foreach (Constraint constraint in Constraints.ToList())
            {
                int sum = 0;

                if (constraint.Sum == 0)
                {
                    // Sum already 0
                }
                else if (constraint.Sum == constraint.Variables.Count)
                {
                    sum = 1;
                }
                else
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
            RemoveEmptyConstraints();
            RemoveDuplicateConstraints();
        }

        /// <summary>
        /// Removes all constraints that have no variables.
        /// </summary>
        public void RemoveEmptyConstraints()
        {
            foreach (Constraint constraint in Constraints.ToList())
            {
                if (constraint.Variables.Count == 0)
                {
                    Constraints.Remove(constraint);
                }
            }
        }

        /// <summary>
        /// Removes all duplicate constraints.
        /// </summary>
        public void RemoveDuplicateConstraints()
        {
            int constraintsCount = Constraints.Count;
            List<Constraint> tempConstraints = [.. Constraints];

            for (int i = 0; i < constraintsCount; i++)
            {
                for (int j = i; j < constraintsCount; j++)
                {
                    if (i != j && tempConstraints[i] == tempConstraints[j])
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
        public void ConstructNewConstraints()
        {
            int constraintCount = Constraints.Count;

            for (int i = 0; i < constraintCount; i++)
            {
                for (int j = 0; j < constraintCount; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }

                    bool canSubtract = Constraints[i].Subtract(Constraints[j], out Constraint newConstraint);

                    if (canSubtract)
                    {
                        Constraints.Add(newConstraint);
                    }
                }
            }
        }

        /// <summary>
        /// Add solved constraints to <see cref="Solutions"/>.
        /// </summary>
        public void UpdateSolvedVariables()
        {
            foreach (Constraint constraint in Constraints)
            {
                if (!constraint.Solved)
                {
                    continue;
                }

                int ID = constraint.Variables.First();
                int sum = constraint.Sum;

                if (Solutions.Where(solution => solution.ID == ID).Any())
                {
                    return;
                }

                Solutions.Add(new Solution(ID, sum));
            }

            foreach (Constraint constraint in Constraints)
            {
                foreach (Solution solution in Solutions)
                {
                    int ID = solution.ID;

                    if (!constraint.Variables.Contains(ID))
                    {
                        continue;
                    }

                    constraint.Variables.Remove(ID);
                    constraint.Sum -= solution.Assignment;
                }
            }
        }
    }
}
