using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.Z3;
using System.ComponentModel;
using System;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;

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

            List<Cell> connectedCells = grid.Cells.Where(cell => !grid.OpenedCells.Contains(cell) && cell.AdjacentCells.Intersect(grid.OpenedCells).Any()).ToList();
            List<Cell> relevantKnownCells = grid.OpenedCells.Where(cell => cell.AdjacentCells.Intersect(connectedCells).Any()).ToList();

            for (int totalMines = 1; totalMines <= connectedCells.Count(); totalMines++)
            {
                List<(Cell, bool)>? model = SolveModel(new(), grid, totalMines, connectedCells, relevantKnownCells, new());

                if (model != null)
                {
                    Console.WriteLine(totalMines);
                    Console.WriteLine(grid.ShowKnown());
                    foreach ((Cell cell, bool status) in model)
                    {
                        Console.WriteLine((cell.Point.ID, status));
                    }
                    Console.WriteLine();
                }
            }

            return 0;
        }


        public static List<(Cell, bool)>? SolveModel(Context ctx, Grid grid, int totalMines, List<Cell> connectedCells, List<Cell> relevantKnownCells, List<(Cell, bool)> constraints)
        {
            using (ctx)
            {
                IntExpr fakeTrue = ctx.MkInt(1);
                IntExpr fakeFalse = ctx.MkInt(0);

                Microsoft.Z3.Solver solver = ctx.MkSolver();

                Dictionary<Cell, IntExpr> expressions = new();

                // Initialize variables
                foreach (Cell cell in connectedCells)
                {
                    int id = cell.Point.ID;
                    IntExpr expr = ctx.MkIntConst(id.ToString());
                    expressions.Add(cell, expr);

                    // Make sure each expressions are "boolean"
                    solver.Assert(ctx.MkOr(ctx.MkEq(expr, fakeTrue), ctx.MkEq(expr, fakeFalse)));
                }

                // Set up mine count
                foreach (Cell cell in relevantKnownCells)
                {
                    int mineCount = (int)cell.MineCount - grid.FlaggedCells.Intersect(cell.AdjacentCells).Count();

                    List<IntExpr> adjacentCells = connectedCells.Intersect(cell.AdjacentCells).Select(i => expressions[i]).ToList();

                    solver.Assert(ctx.MkEq(ctx.MkAdd(adjacentCells), ctx.MkInt(mineCount)));
                }

                // Sum of mines
                solver.Assert(ctx.MkEq(ctx.MkAdd(expressions.Values), ctx.MkInt(totalMines)));

                // Add constraints
                foreach ((Cell cell, bool status) in constraints)
                {
                    if (!connectedCells.Contains(cell))
                    {
                        continue;
                    }

                    solver.Assert(ctx.MkEq(expressions[cell], ctx.MkInt(status ? "1" : "0")));
                }

                // Return intepretations (if any)
                if (solver.Check() == Status.SATISFIABLE)
                {
                    List<(Cell, bool)> result = new();
                    Model model = solver.Model;

                    foreach (FuncDecl d in model.Decls)
                    {
                        Cell cell = grid.Cells.Where(i => i.Point.ID.ToString() == d.Name.ToString()).First();
                        bool status = model.ConstInterp(d).ToString() == "1" ? true : false;

                        result.Add((cell, status));
                    }

                    return result;
                }

                return null;
            }
        }
    }
}