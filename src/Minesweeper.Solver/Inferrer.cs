using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

        public Inferrer(HashSet<Constraint> constraints)
        {
            this.Constraints = constraints;
            this.Solutions = [];
        }

        /// <summary>
        /// Solves the given constraints. Solutions can be accessed via <see cref="Solutions"/>.
        /// </summary>
        public void Solve()
        {
            bool run = true;
            
            List<Constraint> oldConstraints = new();

            // Only stop running when no new constraints can be constructed.
            while (run)
            {
                SolveTrivials();
                ConstructConstraints();
                RemoveUnnecessaryConstraints();
                UpdateSolvedConstraints();

                bool runTemp = Constraints.Except(oldConstraints).Any();

                oldConstraints = Constraints.ToList();

                run = runTemp;

                //Console.WriteLine($"Constraints: {this.Constraints.Count}, Solutions: {this.Solutions.Count}");
            }
        }

        /// <summary>
        /// Sets all variables in a constraint to be safe or mined, depending on <see cref="Constraint.Sum"/>.
        /// </summary>
        public void SolveTrivials()
        {
            HashSet<Constraint> trivialAllSafe = Constraints.Where(i => i.Sum == 0).ToHashSet();
            HashSet<Constraint> trivialAllMined = Constraints.Where(i => i.Sum == i.Variables.Count).ToHashSet();

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

                    bool canSubtract = X.Subtract(Y, out Constraint difference);

                    if (canSubtract)
                    {
                        this.Constraints.Add(difference);
                    }
                }
            }
        }

        public void UpdateSolvedConstraints()
        {
            Solutions.UnionWith(Constraints.Where(i => i.IsSolved));
            Constraints = Constraints.Except(Solutions).ToHashSet();

            foreach (Constraint solution in Solutions.Distinct())
            {
                int ID = solution.Variables.First();

                foreach (Constraint constraint in Constraints)
                {

                    if (constraint.Variables.Contains(ID))
                    {
                        constraint.Variables.Remove(ID);
                        constraint.Sum -= solution.Sum;
                    }
                }
            }
        }
    }
}
