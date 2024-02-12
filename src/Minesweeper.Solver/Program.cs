using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fractions;
using Newtonsoft.Json;

namespace Minesweeper.Solver
{
    public class Program
    {
        public const string FileName = "RawWinData.csv";

        public const int MaxAttempts = 1000;

        public const int UpdateInterval = 1;

        public static void Main()
        {
            Main1();
        }

        public static void Main2()
        {
            Console.Write("Stern-Bocrot Sequence Number: ");
            int sequenceNumber = int.Parse(Console.ReadLine());

            List<Fraction> sequence = Utility.GenerateLeftSternBocrotSequence(sequenceNumber);

            Dictionary<(int, int, int), decimal> completedDimensions = [];

            using (StreamReader reader = new(FileName))
            {
                while (!reader.EndOfStream)
                {
                    string? line = reader.ReadLine();

                    if (line == null)
                    {
                        continue;
                    }

                    List<string> information = [.. line.Split(",")];

                    int p = int.Parse(information[0]);
                    int q = int.Parse(information[1]);
                    int m = int.Parse(information[2]);
                    decimal winRate = decimal.Parse(information[3]);

                    completedDimensions.Add((p, q, m), winRate);
                }
            }

            List<(int, int)> validBoardSizes = [];

            foreach (Fraction fraction in sequence)
            {
                if (fraction.Numerator == 0)
                {
                    continue;
                }

                int maxProduct = Math.Max(3, (int)decimal.Round(36/(decimal)fraction.Numerator));
                for (int i = 1; i <= maxProduct; i++)
                {
                    validBoardSizes.Add(((int)fraction.Numerator * i, (int)fraction.Denominator * i));
                }
            }

            foreach ((int, int) boardSize in validBoardSizes)
            {
                bool zeroAchieved = false;
                decimal previousWinRate = 0;

                for (int m = 1; m < boardSize.Item1 * boardSize.Item2; m++)
                {
                    if (completedDimensions.ContainsKey((boardSize.Item1, boardSize.Item2, m)))
                    {
                        previousWinRate = completedDimensions[(boardSize.Item1, boardSize.Item2, m)];
                        continue;
                    }

                    if (zeroAchieved)
                    {
                        EndDimension(boardSize.Item1, boardSize.Item2, m, 0);
                        continue;
                    }

                    decimal winRate = GetWinRate(boardSize.Item1, boardSize.Item2, m);

                    if (winRate > previousWinRate && (double)m/(boardSize.Item1 * boardSize.Item2) > 0.5)
                    {
                        winRate = 0;
                    }

                    EndDimension(boardSize.Item1, boardSize.Item2, m, winRate);

                    previousWinRate = winRate;

                    if (winRate == 0)
                    {
                        zeroAchieved = true;
                    }
                }
            }
        }

        public static void Main1()
        {
            Console.Write("Max dimension: ");
            int maxDimensions = Console.ReadLine()
                .Split(",")
                .Select(i => int.Parse(i))
                .First();
            Console.WriteLine();

            Console.WriteLine($"Reading from {FileName}...");

            List<(int, int, int)> completedDimensions = [];

            using (StreamReader reader = new(FileName))
            {
                while (!reader.EndOfStream)
                {
                    string? line = reader.ReadLine();

                    if (line == null)
                    {
                        continue;
                    }

                    List<string> information = [.. line.Split(",")];

                    int p = int.Parse(information[0]);
                    int q = int.Parse(information[1]);
                    int m = int.Parse(information[2]);
                    double winRate = double.Parse(information[3]);

                    completedDimensions.Add((p, q, m));
                }
            }

            List<(int, int, int)> remainingDimensions = GetValidDimensions(maxDimensions)
                .Except(completedDimensions)
                .ToList();

            bool zeroAchieved = false;
            (int, int) currentDimension = (0, 0);

            foreach ((int, int, int) dimension in remainingDimensions)
            {
                if (currentDimension != (dimension.Item1, dimension.Item2))
                {
                    currentDimension = (dimension.Item1, dimension.Item2);
                    zeroAchieved = false;
                }

                if (zeroAchieved)
                {
                    EndDimension(dimension.Item1, dimension.Item2, dimension.Item3, 0);
                    continue;
                }

                decimal winRate = GetWinRate(dimension.Item1, dimension.Item2, dimension.Item3);

                if (winRate == 0)
                {
                    zeroAchieved = true;
                }

                EndDimension(dimension.Item1, dimension.Item2, dimension.Item3, winRate);
            }
        }

        public static List<(int, int, int)> GetValidDimensions(int maxDim)
        {
            List<(int, int, int)> res = [];

            for (int i = 1; i <= maxDim; i++)
            {
                for (int m = 1; m < maxDim * i; m++)
                {
                    res.Add((i, maxDim, m));
                }
            }

            return res;
        }

        public static decimal GetWinRate(int length, int width, int mines)
        {
            int wins = 0;
            //decimal previousWinRate = 0;
            //int streak = 0;

            Console.WriteLine("---");

            Stopwatch timer = new();
            timer.Start();

            for (int i = 1; i <= MaxAttempts; i++)
            {
                bool gameSimulated = false;

                while (!gameSimulated)
                {
                    try
                    {
                        wins += Solve(new(length, width, mines));
                        gameSimulated = true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }

                //decimal currentWinRate = (decimal)wins / i;
                //if (currentWinRate == previousWinRate)
                //{
                //    streak++;
                //    if (streak >= 50 && i > 250)
                //    {
                //        return previousWinRate;
                //    }
                //}

                //previousWinRate = currentWinRate;

                if (i % UpdateInterval == 0)
                {
                    Console.WriteLine($"{length}x{width}/{mines}: {wins} wins out of {i} attempts ({timer.Elapsed.TotalMilliseconds}ms per attempt)");
                    timer.Restart();
                }
            }

            timer.Stop();

            return (decimal)wins / MaxAttempts;
        }

        public static int Solve(Grid grid)
        {
            // Start at corner.
            grid.OpenCell(grid.Cells[0]);

            while (grid.State == State.Ongoing)
            {
                Inferrer solver = new(grid);
                solver.Solve();

                // Infer
                if (solver.Solutions.Count != 0)
                {
                    foreach (Constraint solution in solver.Solutions)
                    {
                        Cell cell = Utility.IDToCell(grid, solution.Variables.First());

                        // Update cells
                        switch (solution.Sum)
                        {
                            case 0:
                                grid.OpenCell(cell);
                                break;
                            case 1:
                                grid.FlagCell(cell);
                                break;
                            default:
                                throw new MinesweeperException("Invalid solution: " + solution);
                        }
                    }
                }
                // Guess
                else
                {
                    Guesser guesser = new(grid);

                    var task = Task.Run(guesser.GetScore);

                    if (task.Wait(TimeSpan.FromSeconds(3)))
                    {
                        Dictionary<int, double> scores = guesser.GetScore();

                        // Open guaranteed safe cells.
                        foreach (Cell safeCells in Utility.IDsToCells(grid, scores.Where(i => i.Value == 1).Select(i => i.Key)))
                        {
                            grid.OpenCell(safeCells);
                        }

                        //// Flag guaranteed mined cells.
                        //foreach (Cell minedCells in Utility.IDsToCells(grid, scores.Where(i => i.Value == 0).Select(i => i.Key)))
                        //{
                        //    grid.FlagCell(minedCells);
                        //}

                        // Determine cell to guess
                        double maxScore = scores.OrderByDescending(kvp => kvp.Value).First().Value;

                        Cell toOpen = Utility.IDsToCells(grid, scores.Where(i => i.Value == maxScore).Select(i => i.Key))
                            .OrderBy(i => i.AdjacentCells.Count)
                            .ThenByDescending(i => i.AdjacentCells.Intersect(grid.OpenedCells).Count())
                            .First();

                        grid.OpenCell(toOpen);
                    }
                    else
                    {
                        throw new MinesweeperException("Solver took too long.");
                    }
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

        public static void EndDimension(int length, int width, int mines, decimal winrate)
        {
            using (StreamWriter sw = File.AppendText(FileName))
            {
                sw.WriteLine($"{length},{width},{mines},{winrate}");
            }
        }
    }
}
