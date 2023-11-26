using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.Z3;
using System;
using System.Security.Principal;
using System.Runtime.CompilerServices;

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

            int wins = 0;

            for (int i = 0; i < 1; i++)
            {
                Console.WriteLine((i, wins));
                Grid grid = new(10, 10, 10);

                grid.OpenCell(grid.Cells.Where(i => i.MineCount == 0).First());

                Console.WriteLine(grid.ShowKnown() + "\n");

                Interpretation logic = SolveLogic(grid);

                if (!logic.Empty)
                {
                    foreach (Cell cell in logic.CellsAssigned)
                    {
                        if (logic.Assignments[cell])
                        {
                            grid.FlagCell(cell);
                        }
                        else
                        {
                            grid.OpenCell(cell);
                        }
                    }
                }

                foreach (Cell cell in grid.FlaggedCells)
                {
                    Console.WriteLine(cell.Point.ID);
                }

                Console.WriteLine(grid.ShowKnown() + "\n");
            }

            Console.WriteLine(wins);
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
            while (grid.State == State.ToBegin || grid.State == State.Ongoing)
            {
                // Check for low hanging fruit
                LowHangingFruit(grid);
                
                // Check for logic
                Interpretation logic = SolveLogic(grid);

                // Update cells
                if (!logic.Empty)
                {
                    foreach (Cell cell in logic.CellsAssigned)
                    {
                        if (logic.Assignments[cell] == true)
                        {
                            cell.HasFlag = true;
                        }
                        else
                        {
                            grid.OpenCell(cell);
                        }
                    }
                }
                else if (grid.State == State.ToBegin || grid.State == State.Ongoing)
                {
                    Cell guess = GuessCell(grid);

                    grid.OpenCell(guess);
                }
            }

            Console.WriteLine(grid.ShowKnown());

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
        /// Solves for obvious logic.
        /// </summary>
        /// <param name="grid">The grid to solve.</param>
        public static void LowHangingFruit(Grid grid)
        {
            bool update = true;

            while (update)
            {
                int updated = 0;

                foreach (Cell cell in grid.OpenedCells.Where(i => i.AdjacentCells.Intersect(grid.UnknownCells).Any()))
                {
                    List<Cell> adjacentUnknown = cell.AdjacentCells.Intersect(grid.UnknownCells).ToList();
                    List<Cell> adjacentFlagged = cell.AdjacentCells.Intersect(grid.FlaggedCells).ToList();

                    if (adjacentUnknown.Count + adjacentFlagged.Count == cell.MineCount)
                    {
                        foreach (Cell adjacentUnknownCell in adjacentUnknown)
                        {
                            adjacentUnknownCell.HasFlag = true;
                        }
                        updated++;
                    }

                    if (adjacentFlagged.Count == cell.MineCount)
                    {
                        grid.Chord(cell);
                        updated++;
                    }
                }

                if (updated == 0)
                {
                    update = false;
                }
            }
        }

        /// <summary>
        /// Gets a guaranteed <see cref="Interpretation">interpretation</see>.
        /// </summary>
        /// <param name="grid">The grid to solve for logic.</param>
        /// <returns></returns>
        public static Interpretation SolveLogic(Grid grid)
        {
            List<Cell> connectedCells = grid.Cells.Where(cell => grid.UnknownCells.Contains(cell) && cell.AdjacentCells.Intersect(grid.OpenedCells).Any()).ToList();

            Dictionary<int, Interpretation> allInterpretations = new();

            // Preliminary check - go through all possible mine counts
            for (int mines = 0; mines <= Math.Min(connectedCells.Count(), grid.Mines - grid.FlaggedCells.Count); mines++)
            {
                Interpretation interpretation = SolveModel(new(), grid, mines, connectedCells, new());

                if (!interpretation.Empty)
                {
                    allInterpretations.Add(mines, interpretation);
                }
            }

            Interpretation potentialCells = new();

            // Check for cells with constant interpretations
            foreach (KeyValuePair<int, Interpretation> kvpAllInt in allInterpretations)
            {
                Interpretation interpretation = kvpAllInt.Value;

                foreach (KeyValuePair<Cell, bool> kvpInt in interpretation.Assignments)
                {
                    Cell cell = kvpInt.Key;
                    bool hasMine = kvpInt.Value;

                    if (!potentialCells.Contains(cell))
                    {
                        potentialCells.Assign(cell, hasMine);
                    }
                    else if (potentialCells.Assignments[cell] != hasMine)
                    {
                        potentialCells.Remove(cell);
                    }
                }
            }

            Interpretation guaranteedCells = potentialCells;

            // Try negating the mine status of potential cells
            foreach (KeyValuePair<Cell, bool> kvp in potentialCells.Assignments)
            {
                Cell cell = kvp.Key;
                bool hasMine = kvp.Value;

                if (!guaranteedCells.Contains(cell))
                {
                    continue;
                }

                for (int mines = 0; mines <= Math.Min(connectedCells.Count(), grid.Mines - grid.FlaggedCells.Count); mines++)
                {
                    Interpretation model = SolveModel(new(), grid, mines, connectedCells, new List<(Cell, bool)> { (cell, !hasMine) });

                    if (model.Empty)
                    {
                        continue;
                    }

                    guaranteedCells.Remove(cell);

                    // further delete any potential cell that had a different value
                    foreach (KeyValuePair<Cell, bool> assignment in model.Assignments)
                    {
                        Cell assignedCell = assignment.Key;
                        bool assignedHasMine = assignment.Value;

                        if (guaranteedCells.Contains(assignedCell) && hasMine != guaranteedCells.Assignments[assignedCell])
                        {
                            guaranteedCells.Remove(assignedCell);
                        }
                    }
                }
            }

            return guaranteedCells;
        }

        /// <summary>
        /// Solves a given grid. Interpretation is not guaranteed to be correct.
        /// </summary>
        /// <param name="ctx">The Z3 context.</param>
        /// <param name="grid">The grid to solve.</param>
        /// <param name="totalMines">The total number of mines to consider.</param>
        /// <param name="searchSpace">A list of unknown cells that are adjacent to an opened cell.</param>
        /// <param name="relevantKnownCells">A list of opened cells that are adjacent to a connected cell.</param>
        /// <param name="constraints">Extra constraints to add.</param>
        /// <returns></returns>
        public static Interpretation SolveModel(Context ctx, Grid grid, int totalMines, List<Cell> searchSpace, List<(Cell, bool)> constraints)
        {
            if (!searchSpace.Any())
            {
                return new();
            }

            List<Cell> infoCells = grid.OpenedCells.Where(cell => cell.AdjacentCells.Intersect(searchSpace).Any()).ToList();

            using (ctx)
            {
                IntExpr fakeTrue = ctx.MkInt(1);
                IntExpr fakeFalse = ctx.MkInt(0);

                Microsoft.Z3.Solver solver = ctx.MkSolver();

                Dictionary<Cell, IntExpr> expressions = new();

                // Initialize variables
                foreach (Cell cell in searchSpace)
                {
                    int id = cell.Point.ID;
                    IntExpr expr = ctx.MkIntConst(id.ToString());
                    expressions.Add(cell, expr);

                    // Make sure each expressions are "boolean"
                    solver.Assert(ctx.MkOr(ctx.MkEq(expr, fakeTrue), ctx.MkEq(expr, fakeFalse)));
                }

                // Set up mine count
                foreach (Cell cell in infoCells)
                {
                    int mineCount = (int)cell.MineCount - grid.FlaggedCells.Intersect(cell.AdjacentCells).Count();

                    List<IntExpr> adjacentCells = searchSpace.Intersect(cell.AdjacentCells).Select(i => expressions[i]).ToList();

                    solver.Assert(ctx.MkEq(ctx.MkAdd(adjacentCells), ctx.MkInt(mineCount)));
                }

                // Sum of mines
                solver.Assert(ctx.MkEq(ctx.MkAdd(expressions.Values), ctx.MkInt(totalMines)));

                // Add constraints
                foreach ((Cell cell, bool hasMine) in constraints)
                {
                    if (!searchSpace.Contains(cell))
                    {
                        continue;
                    }

                    solver.Assert(ctx.MkEq(expressions[cell], ctx.MkInt(hasMine ? 1 : 0)));
                }

                // Return intepretations (if any)
                if (solver.Check() == Status.SATISFIABLE)
                {
                    Interpretation result = new();
                    Model model = solver.Model;

                    foreach (FuncDecl d in model.Decls)
                    {
                        Cell cell = grid.Cells.Where(i => i.Point.ID.ToString() == d.Name.ToString()).First();
                        bool hasMine = model.ConstInterp(d).ToString() == "1" ? true : false;
                        result.Assign(cell, hasMine);
                    }

                    return result;
                }

                return new();
            }
        }
    
        public static Cell GuessCell(Grid grid)
        {
            List<List<Cell>> groups = GetGroups(grid);

            if (!groups.Any())
            {
                return grid.UnknownCells.OrderBy(i => i.AdjacentCells.Count).First();
            }

            Dictionary<Cell, double> pSafety = new();

            Dictionary<int, List<List<(Cell, bool)>>> interpretations = new();

            foreach (List<Cell> group in groups)
            {
                for (int mines = 0; mines < Math.Min(group.Count, grid.Mines); mines++)
                {
                    using (Context ctx = new())
                    {
                        IntExpr fakeTrue = ctx.MkInt(1);
                        IntExpr fakeFalse = ctx.MkInt(0);

                        Microsoft.Z3.Solver solver = ctx.MkSolver();

                        Dictionary<Cell, IntExpr> expressions = new();

                        // Initialize variables
                        foreach (Cell cell in group)
                        {
                            int id = cell.Point.ID;
                            IntExpr expr = ctx.MkIntConst(id.ToString());
                            expressions.Add(cell, expr);

                            // Make sure each expressions are "boolean"
                            solver.Assert(ctx.MkOr(ctx.MkEq(expr, fakeTrue), ctx.MkEq(expr, fakeFalse)));
                        }

                        // Set up mine count
                        List<Cell> relevantKnownCells = grid.OpenedCells.Where(i => i.AdjacentCells.Intersect(group).Any()).ToList();
                        foreach (Cell cell in relevantKnownCells)
                        {
                            int mineCount = (int)cell.MineCount - grid.FlaggedCells.Intersect(cell.AdjacentCells).Count();

                            List<IntExpr> adjacentCells = group.Intersect(cell.AdjacentCells).Select(i => expressions[i]).ToList();

                            solver.Assert(ctx.MkEq(ctx.MkAdd(adjacentCells), ctx.MkInt(mineCount)));
                        }

                        // Sum of mines
                        solver.Assert(ctx.MkEq(ctx.MkAdd(expressions.Values), ctx.MkInt(mines)));

                        if (solver.Check() == Status.SATISFIABLE)
                        {
                            Model model = solver.Model;

                            List<(Cell, bool)> result = new();

                            foreach (FuncDecl d in model.Decls)
                            {
                                Cell cell = grid.Cells.Where(i => i.Point.ID.ToString() == d.Name.ToString()).First();
                                bool hasMine = model.ConstInterp(d).ToString() == "1" ? true : false;

                                result.Add((cell, hasMine));
                            }

                            if (interpretations.Keys.Contains(mines))
                            {
                                interpretations[mines].Add(result);
                            }
                            else
                            {
                                interpretations.Add(mines, new List<List<(Cell, bool)>> { result });
                            }

                            List<Cell> expressionKey = expressions.Keys.ToList();

                            for (int i = 0; i < expressions.Count; i++)
                            {
                                solver.Push();

                                IntExpr expr = expressions[expressionKey[i]];
                                IntExpr antiEval = ctx.MkInt(model.ConstInterp(expr).ToString() == "1" ? 0 : 1);
                                solver.Assert(ctx.MkEq(expr, antiEval));

                                for (int j = 0; j < 1; j++)
                                {
                                    IntExpr expr1 = expressions[expressionKey[i]];
                                    IntExpr antiEval1 = ctx.MkInt(model.ConstInterp(expr).ToString() == "1" ? 0 : 1);
                                    solver.Assert(ctx.MkEq(expr, antiEval));
                                }

                                solver.Pop();
                            }


                        }

                        int countInterpretations = 0;

                        // Return intepretations (if any)
                        while (countInterpretations <= group.Count)
                        {
                            if (solver.Check() != Status.SATISFIABLE)
                            {
                                continue;
                            }

                            countInterpretations++;
                            Console.WriteLine(countInterpretations);

                            
                            Model model = solver.Model;

                            foreach (FuncDecl d in model.Decls)
                            {
                                Cell cell = grid.Cells.Where(i => i.Point.ID.ToString() == d.Name.ToString()).First();
                                bool hasMine = model.ConstInterp(d).ToString() == "1" ? true : false;

                                //result.Add((cell, hasMine));
                            }



                            List<BoolExpr> block = new();

                            foreach (FuncDecl d in model.Decls)
                            {
                                IntExpr c = ctx.MkInt(d.Name.ToString());
                                IntExpr eval = ctx.MkInt(model.ConstInterp(d).ToString() == "1" ? 0 : 1);
                                block.Add(ctx.MkEq(c, eval));
                            }

                            solver.Assert(ctx.MkOr(block));
                        }

                        //Console.WriteLine("interps: " + interpretations.Count);
                    }
                }
            }

            Dictionary<Cell, long> safety = new();
            long total = 0;

            foreach (List<Cell> group in groups)
            {
                List<int> mineCounts = new();
                List<List<(Cell, bool)>> groupInterps = new();

                foreach (KeyValuePair<int, List<List<(Cell, bool)>>> kvp in interpretations)
                {
                    Dictionary<Cell, int> countSafe = new();

                    int mineCount = kvp.Key;

                    List<List<(Cell, bool)>> variations = kvp.Value.Where(i => i.Select(i => i.Item1).Intersect(group).Any()).ToList();

                    //Console.WriteLine((grid.UnknownCells.Count - group.Count, grid.Mines - mineCount));
                    //Console.WriteLine();
                    long mult = nCr(grid.UnknownCells.Count - group.Count, grid.Mines - mineCount);
                    total += kvp.Value.Count * mult;

                    foreach (List<(Cell, bool)> variation in variations)
                    {
                        foreach ((Cell cell, bool hasMine) in variation)
                        {
                            if (!hasMine)
                            {
                                if (countSafe.ContainsKey(cell))
                                {
                                    countSafe[cell]++;
                                }
                                else
                                {
                                    countSafe[cell] = 1;
                                }
                            }
                        }
                    }

                    foreach (Cell cell in group)
                    {
                        if (!countSafe.ContainsKey(cell))
                        {
                            countSafe[cell] = 0;
                        }

                        if (safety.ContainsKey(cell))
                        {
                            safety[cell] += countSafe[cell] * mult;
                        }
                        else
                        {
                            safety.Add(cell, countSafe[cell] * mult);
                        }
                    }
                }
            }

            foreach (KeyValuePair<Cell, long> kvp in safety)
            {
                pSafety[kvp.Key] = (double)kvp.Value / total;
            }

            List<Cell> connectedCell = grid.UnknownCells.Where(i => i.AdjacentCells.Intersect(grid.OpenedCells).Any()).ToList();
            double expectedMines = connectedCell.Count - pSafety.Values.Sum();

            //Console.WriteLine(expectedMines);

            List<Cell> floatingCells = grid.UnknownCells.Where(i => !i.AdjacentCells.Intersect(grid.OpenedCells).Any()).ToList();
            foreach (Cell cell in floatingCells)
            {
                pSafety[cell] = 1 - (grid.Mines - (int)expectedMines) / (double)floatingCells.Count;
            }

            foreach (KeyValuePair<Cell, double> kvp in pSafety)
            {
                //Console.WriteLine((kvp.Key.Point.ID, kvp.Value));
            }

            return pSafety.MaxBy(kvp => kvp.Value).Key;
        }

        public static List<List<Cell>> GetGroups(Grid grid)
        {
            List<List<Cell>> groups = new();

            List<Cell> searchSpace = grid.Cells.Where(cell => grid.UnknownCells.Contains(cell) && cell.AdjacentCells.Intersect(grid.OpenedCells).Any()).ToList();
            List<Cell> searched = new();
            List<Cell> toSearch = new();

            List<Cell> auxGroup = new();

            Cell seed = null;

            while (searched.Count < searchSpace.Count)
            {
                if (toSearch.Any())
                {
                    seed = toSearch.First();
                    auxGroup.Add(seed);
                }
                else
                {
                    // add previous island size to sizes
                    if (searched.Count != 0)
                    {
                        groups.Add(auxGroup);
                        auxGroup = new();
                    }

                    // select another seed
                    seed = searchSpace.Except(searched).First();
                    auxGroup.Add(seed);
                }

                // add cells if 1) adjacent to seed, 2) in cells, 3) not in searched
                toSearch.AddRange(seed.AdjacentCells.Intersect(searchSpace).Except(searched).Except(toSearch).ToList());
                toSearch.Remove(seed);
                searched.Add(seed);
            }

            if (auxGroup.Any())
            {
                groups.Add(auxGroup);
            }
            return groups;
        }

        public static long nCr(int n, int r)
        {
            // naive: return Factorial(n) / (Factorial(r) * Factorial(n - r));
            return nPr(n, r) / Factorial(r);
        }

        public static long nPr(int n, int r)
        {
            // naive: return Factorial(n) / Factorial(n - r);
            return FactorialDivision(n, n - r);
        }

        private static long FactorialDivision(int topFactorial, int divisorFactorial)
        {
            long result = 1;
            for (int i = topFactorial; i > divisorFactorial; i--)
                result *= i;
            return result;
        }

        private static long Factorial(int i)
        {
            if (i <= 1)
                return 1;
            return i * Factorial(i - 1);
        }
    }
}