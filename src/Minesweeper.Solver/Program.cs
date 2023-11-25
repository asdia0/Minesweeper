using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.Z3;
using System.ComponentModel;
using System;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;

namespace Minesweeper.Solver
{
    public class Program
    {
        public static void Main()
        {
            //int maxLength = 10;
            //int maxWidth = 10;

            //Dictionary<(int, int), List<double>> data = new();

            //for (int p = 1; p <= maxLength; p++)
            //{
            //    for (int q = p; q <= maxWidth; q++)
            //    {
            //        data.Add((p, q), GetWinRateData(p, q));
            //    }
            //}

            //string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            //File.WriteAllText("data.json", json);

            SolveLogic(new(10, 10, 1));
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

        public static int Solve(Grid grid)
        {
            while (grid.State != State.Success && grid.State != State.Fail)
            {
                List<(Cell, bool)>? logic = SolveLogic(grid);

                if (logic != null)
                {
                    foreach ((Cell cell, bool hasMine) in logic)
                    {
                        if (hasMine)
                        {
                            cell.HasFlag = true;
                        }
                        else
                        {
                            grid.OpenCell(cell);
                        }
                    }
                }
                else
                {
                    Cell guess = GuessCell(grid);

                    grid.OpenCell(guess);
                }
            }

            if (grid.State == State.Success)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Gets a list of cells that are guaranteed to be safe or mined.
        /// </summary>
        /// <param name="grid1"></param>
        /// <returns></returns>
        public static List<(Cell, bool)> SolveLogic(Grid grid1)
        {
            Grid grid = new(10, 10, 10);

            grid.OpenCell(grid.Cells.Where(i => i.MineCount == 0).FirstOrDefault());

            List<Cell> connectedCells = grid.Cells.Where(cell => grid.UnknownCells.Contains(cell) && cell.AdjacentCells.Intersect(grid.OpenedCells).Any()).ToList();
            List<Cell> relevantKnownCells = grid.OpenedCells.Where(cell => cell.AdjacentCells.Intersect(connectedCells).Any()).ToList();

            List<(int, List<(Cell, bool)>)> allInterpretations = new();

            // Preliminary check - go through all possible mine counts
            for (int mines = 1; mines <= Math.Min(connectedCells.Count(), grid.Mines); mines++)
            {
                List<(Cell, bool)> interpretation = SolveModel(new(), grid, mines, connectedCells, relevantKnownCells, new());

                if (interpretation.Any())
                {
                    allInterpretations.Add((mines, interpretation));
                }
            }

            Dictionary<Cell, bool> potentialCells = new();

            // Check for cells with constant interpretations
            foreach ((int mines, List<(Cell, bool)> interpretation) in allInterpretations)
            {
                foreach ((Cell cell, bool hasMine) in interpretation)
                {
                    if (potentialCells.Keys.Contains(cell) && potentialCells[cell] != hasMine)
                    {
                        potentialCells.Remove(cell);
                    }
                    else if (!potentialCells.Keys.Contains(cell))
                    {
                        potentialCells.Add(cell, hasMine);
                    }
                }
            }

            return potentialCells.Select(kvp => (kvp.Key, kvp.Value)).ToList();
        }

        /// <summary>
        /// Solves a given grid. Intepretation is not guaranteed to be correct.
        /// </summary>
        /// <param name="ctx">The Z3 context.</param>
        /// <param name="grid">The grid to solve.</param>
        /// <param name="totalMines">The total number of mines to consider.</param>
        /// <param name="connectedCells">A list of unknown cells that are adjacent to an opened cell.</param>
        /// <param name="relevantKnownCells">A list of opened cells that are adjacent to a connected cell.</param>
        /// <param name="constraints">Extra constraints to add.</param>
        /// <returns></returns>
        public static List<(Cell, bool)> SolveModel(Context ctx, Grid grid, int totalMines, List<Cell> connectedCells, List<Cell> relevantKnownCells, List<(Cell, bool)> constraints)
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
                foreach ((Cell cell, bool hsMine) in constraints)
                {
                    if (!connectedCells.Contains(cell))
                    {
                        continue;
                    }

                    solver.Assert(ctx.MkEq(expressions[cell], ctx.MkInt(hsMine ? "1" : "0")));
                }

                // Return intepretations (if any)
                if (solver.Check() == Status.SATISFIABLE)
                {
                    List<(Cell, bool)> result = new();
                    Model model = solver.Model;

                    foreach (FuncDecl d in model.Decls)
                    {
                        Cell cell = grid.Cells.Where(i => i.Point.ID.ToString() == d.Name.ToString()).First();
                        bool hasMine = model.ConstInterp(d).ToString() == "1" ? true : false;

                        result.Add((cell, hasMine));
                    }

                    return result;
                }

                return new();
            }
        }
    
        public static Cell GuessCell(Grid grid)
        {
            return grid.Cells.Where(cell => !cell.IsOpen).First();
        }
    }
}