using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.Z3;
using System.ComponentModel;
using System;
using System.Reflection.Metadata;

namespace Minesweeper.Solver
{
    public class Program
    {
        public static void Main()
        {
            int maxLength = 1;
            int maxWidth = 1;

            Dictionary<(int, int), List<double>> data = new();

            for (int p = 1; p <= maxLength; p++)
            {
                for (int q = p; q <= maxWidth; q++)
                {
                    data.Add((p, q), GetWinRateData(p, q));
                }
            }

            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText("data.json", json);
        }

        public static List<double> GetWinRateData(int p, int q)
        {
            List<double> winRates = new();
            bool killWinRate = false;

            for (int m = 0; m < p*q; m++)
            {
                if (killWinRate)
                {
                    winRates.Add(0);
                }

                Grid grid = new(p, q, m+1);
                winRates.Add(GetWinRate(grid));

                // Set all subsequent winrates to 0
                if (m > 0.75 * p * q && winRates[m] > winRates[m - 1])
                {
                    winRates[m] = 0;
                    killWinRate = true;
                }
            }

            return winRates;
        }

        public static double GetWinRate(Grid grid)
        {
            int wins = 0;
            double previousWinRate = 0;
            int streak = 0;

            //for (int i = 1; i <= 10000; i++)
            for (int i = 1; i <= 1; i++)
            {
                wins += Solve(grid);
                double currentWinRate = wins / i;
                if (previousWinRate == currentWinRate)
                {
                    streak++;
                    if (streak == 50 && i > 2500)
                    {
                        return currentWinRate;
                    }
                }
                previousWinRate = currentWinRate;
            }

            return previousWinRate;
        }

        public static int Solve(Grid grid1)
        {
            // Set up system of boolean equations
            // Only consider non-landlocked cells so as to reduce computation
            // Can do this by iterating through number of mines m and trying to find solutions
            // Get guaranteed solutions at the end

            Grid grid = new(4, 4, 2);

            grid.OpenCell(grid.Cells.Where(i => i.MineCount == 0).FirstOrDefault());

            List<Cell> knownCells = grid.OpenedCells.Union(grid.FlaggedCells).ToList();
            List<Cell> connectedCells = grid.Cells.Where(cell => !knownCells.Contains(cell) && cell.AdjacentCells.Intersect(knownCells).Any()).ToList();
            List<Cell> relevantKnownCells = knownCells.Where(cell => cell.AdjacentCells.Intersect(connectedCells).Any()).ToList();

            for (int m = 1; m <= connectedCells.Count(); m++)
            {
                using (Context ctx = new())
                {
                    IntExpr fakeTrue = ctx.MkInt(1);
                    IntExpr fakeFalse = ctx.MkInt(0);

                    Microsoft.Z3.Solver solver = ctx.MkSolver();

                    Dictionary<Cell, IntExpr> expressions = new();

                    // Initialize variables
                    foreach (Cell cell in connectedCells)
                    {
                        int id = cell.Point.ID;
                        IntExpr expr = ctx.MkIntConst($"c_{id}");
                        expressions.Add(cell, expr);

                        // Make sure each expressions are "boolean"
                        solver.Assert(ctx.MkOr(ctx.MkEq(expr, fakeTrue), ctx.MkEq(expr, fakeFalse)));
                    }

                    // Set up mine count
                    foreach (Cell cell in relevantKnownCells)
                    {
                        if (cell.HasFlag)
                        {
                            break;
                        }

                        int mineCount = (int)cell.MineCount - grid.FlaggedCells.Intersect(cell.AdjacentCells).Count();

                        List<IntExpr> adjacentCells = connectedCells.Intersect(cell.AdjacentCells).Select(i => expressions[i]).ToList();

                        solver.Assert(ctx.MkEq(ctx.MkAdd(adjacentCells), ctx.MkInt(mineCount)));
                    }

                    // Sum of mines
                    solver.Assert(ctx.MkEq(ctx.MkAdd(expressions.Values), ctx.MkInt(m)));

                    Console.WriteLine(m);
                    Console.WriteLine(grid.ShowKnown());

                    // If there is a solution, begin blocking model
                    if (solver.Check() == Status.SATISFIABLE)
                    {
                        Console.WriteLine(solver.Model);
                    }
                    Console.Write("\n");
                }
            }

            return 0;
        }
    }
}