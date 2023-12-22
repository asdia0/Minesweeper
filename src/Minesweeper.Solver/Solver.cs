using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using Newtonsoft.Json;

namespace Minesweeper.Solver
{
    public class Solver
    {
        public List<Equation> Equations { get; set; }

        public List<(int, int)> Solutions { get; set; }

        public Solver(Grid grid)
        {
            Equations = new();
            Solutions = new();

            // Set up local constraint
            foreach (Cell boundaryCell in grid.OpenedCells.Where(i => i.AdjacentCells.Intersect(grid.UnknownCells).Any()))
            {
                HashSet<int> cellVariables = new();

                foreach (Cell adjacentExposedCell in boundaryCell.AdjacentCells.Intersect(grid.UnknownCells))
                {
                    cellVariables.Add(adjacentExposedCell.Point.ID);
                }

                Equations.Add(new(cellVariables, (int)boundaryCell.MineCount - boundaryCell.AdjacentCells.Intersect(grid.FlaggedCells).Count()));
            }

            //Console.WriteLine(string.Join("\n", Equations.Select(i => (string.Join(",", i.Variables), i.Sum))));

            // Set up global constraint (check if minecounting available)
        }

        public void SolveLogic()
        {
            bool run = true;

            List<Equation> previousEquations = new();

            RemoveEquations();

            while (run)
            {
                GetLowHangingFruit();
                GenerateNewEquations();
                UpdateSolvedVariables();
                RemoveEquations();

                bool runTemp = Equations.Except(previousEquations).Any();

                previousEquations = Equations;

                run = runTemp;
            }
        }

        public void GetLowHangingFruit()
        {
            foreach (Equation equation in Equations.ToList())
            {
                if (equation.Sum == 0)
                {
                    foreach (int variable in equation.Variables)
                    {
                        Equations.Add(new(new() { variable }, 0));
                    }
                }
                else if (equation.Sum == equation.Variables.Count)
                {
                    foreach (int variable in equation.Variables)
                    {
                        Equations.Add(new(new() { variable }, 1));
                    }

                    Equations.Remove(equation);
                }
            }
        }

        public void RemoveEquations()
        {
            RemoveEmptyEquations();
            RemoveDuplicates();
        }

        public void RemoveEmptyEquations()
        {
            foreach (Equation equation in Equations.ToList())
            {
                if (equation.Variables.Count == 0)
                {
                    Equations.Remove(equation);
                }
            }
        }

        public void RemoveDuplicates()
        {
            int equationsCount = Equations.Count;
            List<Equation> tempEquations = Equations.ToList();

            for (int i = 0; i < equationsCount; i++)
            {
                for (int j = i; j < equationsCount; j++)
                {
                    if (i != j && tempEquations[i] == tempEquations[j])
                    {
                        Equations.Remove(tempEquations[i]);
                    }
                }
            }
        }

        public void GenerateNewEquations()
        {
            int equationsCount = Equations.Count;

            for (int i = 0; i < equationsCount; i++)
            {
                for (int j = 0; j < equationsCount; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }

                    bool canSubtract = Equations[i].Subtract(Equations[j], out Equation newEquation);

                    if (canSubtract)
                    {
                        Equations.Add(newEquation);
                    }
                }
            }
        }

        public void UpdateSolvedVariables()
        {
            foreach (Equation equation in Equations)
            {
                if (!equation.Solved)
                {
                    continue;
                }

                int ID = equation.Variables.First();
                int sum = equation.Sum;

                AddNewSolution(ID, sum);
            }

            foreach (Equation equation in Equations)
            {
                foreach ((int ID, int sum) in this.Solutions)
                {
                    if (!equation.Variables.Contains(ID))
                    {
                        continue;
                    }

                    equation.Variables.Remove(ID);
                    equation.Sum -= sum;
                }
            }
        }

        public void AddNewSolution(int ID, int sum)
        {
            if (Solutions.Where(i => i.Item1 == ID).Any())
            {
                return;
            }

            Solutions.Add((ID, sum));
        }
    }
}
