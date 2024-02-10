using System;
using System.Collections.Generic;
using System.Linq;

namespace Minesweeper.Solver
{
    public class Inferrer
    {
        /// <summary>
        /// A list of <see cref="Constraint">constraints</see> to infer from.
        /// </summary>
        public HashSet<Constraint> Constraints { get; set; }

        /// <summary>
        /// A list of solutions to the given constraints.
        /// </summary>
        public HashSet<Constraint> Solutions { get; set; }

        /// <summary>
        /// Checks if there any contradicting constraints.
        /// </summary>
        public bool HasContradiction { get; set; }

        /// <summary>
        /// Initalizes a new instance of <see cref="Inferrer"/> class.
        /// </summary>
        /// <param name="grid">The <see cref="Grid"/> to infer from.</param>
        public Inferrer(Grid grid)
        {
            this.Constraints = [];
            this.Solutions = [];

            // Set up local constraints
            foreach (Cell boundaryCell in grid.BoundaryCells)
            {
                HashSet<int> cellVariables = Utility.CellsToIDs(boundaryCell.AdjacentCells.Intersect(grid.UnknownCells)).ToHashSet();
                this.Constraints.Add(new(cellVariables, (int)boundaryCell.MineCount - boundaryCell.AdjacentCells.Intersect(grid.FlaggedCells).Count()));
            }

            // Set up global constraint
            HashSet<int> unknownCellVariables = Utility.CellsToIDs(grid.UnknownCells).ToHashSet();
            this.Constraints.Add(new(unknownCellVariables, grid.Mines - grid.FlaggedCells.Count));
        }

        /// <summary>
        /// Solves the given constraints. Solutions can be accessed via <see cref="Solutions"/>.
        /// </summary>
        public void Solve()
        {
            bool run = true;

            List<Constraint> oldConstraints = [];

            // Only stop running when no new constraints can be constructed.
            while (run)
            {
                this.SolveTrivials();
                this.ConstructConstraints();
                this.RemoveUnnecessaryConstraints();
                this.UpdateSolutions();

                bool runTemp = this.Constraints.Except(oldConstraints).Any() && !this.HasContradiction;
                oldConstraints = [.. this.Constraints];
                run = runTemp;
            }
        }

        /// <summary>
        /// Sets all variables in a constraint to be safe or mined, depending on its <see cref="Constraint.Sum">sum</see>.
        /// </summary>
        public void SolveTrivials()
        {
            HashSet<Constraint> trivialAllSafe = this.Constraints.Where(i => i.Sum == 0).ToHashSet();
            HashSet<Constraint> trivialAllMined = this.Constraints.Where(i => i.Sum == i.Variables.Count).ToHashSet();

            foreach (Constraint constraint in trivialAllSafe)
            {
                foreach (int variable in constraint.Variables)
                {
                    this.Solutions.Add(new([variable], 0));
                }

                this.Constraints.Remove(constraint);
            }

            foreach (Constraint constraint in trivialAllMined)
            {
                foreach (int variable in constraint.Variables)
                {
                    this.Solutions.Add(new([variable], 1));
                }

                this.Constraints.Remove(constraint);
            }
        }

        /// <summary>
        /// Removes all unnecessary constraints.
        /// </summary>
        public void RemoveUnnecessaryConstraints()
        {
            this.Constraints.RemoveWhere(i => i.Variables.Count == 0);
        }

        /// <summary>
        /// Constructs new constraints based on the existing constraints.
        /// For example, the constraints A+B=1 and A=1 will result in a new constraint B=0.
        /// </summary>
        public void ConstructConstraints()
        {
            int constraintCount = this.Constraints.Count;

            List<Constraint> constraintList = [.. this.Constraints];

            for (int i = 0; i < constraintCount; i++)
            {
                for (int j = 0; j < constraintCount; j++)
                {
                    Constraint X = constraintList[i];
                    Constraint Y = constraintList[j];

                    bool canSubtract = X.Subtract(Y, out Constraint difference);

                    if (canSubtract)
                    {
                        this.HasContradiction = difference.Sum < 0;
                        this.Constraints.Add(difference);
                    }
                }
            }
        }

        /// <summary>
        /// Updates <see cref="Solutions"/> and all affected <see cref="Constraints">constraints</see>.
        /// </summary>
        public void UpdateSolutions()
        {
            this.Solutions.UnionWith(this.Constraints.Where(i => i.IsSolved));
            this.Constraints = this.Constraints.Except(this.Solutions).ToHashSet();

            foreach (Constraint solution in this.Solutions.Distinct())
            {
                int ID = solution.Variables.First();

                foreach (Constraint constraint in this.Constraints)
                {
                    bool solutionPresent = constraint.Variables.Remove(ID);

                    if (solutionPresent)
                    {
                        constraint.Sum -= solution.Sum;
                    }
                }
            }
        }
    }
}
