using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Fractions;
using System.Net;

namespace Minesweeper.Solver
{
    public class Program
    {
        public const string FileName = "RawWinData.csv";

        public const int MaxAttempts = 1000;

        public static void Main()
        {
            //Main1();
            GetWinRate(16, 16, 40);
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
                    string line = reader.ReadLine();

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
                    string line = reader.ReadLine();

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

            //for (int i = 1; i <= maxDim; i++)
            //{
                for (int j = 1; j <= maxDim; j++)
                {
                    for (int m = 1; m < maxDim * j; m++)
                    {
                        res.Add((j, maxDim, m));
                    }
                }
            //}

            return res;
        }

        public static decimal GetWinRate(int length, int width, int mines)
        {
            int wins = 0;
            decimal previousWinRate = 0;
            int streak = 0;

            Console.WriteLine("---");

            var timer = new Stopwatch();
            timer.Start();

            for (int i = 1; i <= MaxAttempts; i++)
            {
                wins += Solve(new(length, width, mines));
                decimal currentWinRate = (decimal)wins / i;
                if (previousWinRate == currentWinRate)
                {
                    streak++;
                    if (streak == 5 && i > 250)
                    {
                        EndDimension(length, width, mines, currentWinRate);
                    }
                }
                previousWinRate = currentWinRate;

                if (i % 1 == 0)
                {
                    double timeTaken = timer.Elapsed.TotalSeconds;
                    Console.WriteLine($"{length}x{width}/{mines}: {wins} wins out of {i} attempts ({timeTaken / i}s per attempt)");
                }
            }

            timer.Stop();

            return previousWinRate;
        }

        public static void EndDimension(int length, int width, int mines, decimal winrate)
        {
            using (StreamWriter sw = File.AppendText(FileName))
            {
                sw.WriteLine($"{length},{width},{mines},{winrate}");
            }
        }

        public static int Solve(Grid grid)
        {
            // Start at corner.
            grid.OpenCell(grid.Cells[0]);

            while (grid.State == State.ToBegin || grid.State == State.Ongoing)
            {
                Inferrer solver = new(grid);

                solver.Solve();

                // Update cells
                if (solver.Solutions.Count != 0)
                {
                    foreach (Constraint solution in solver.Solutions)
                    {
                        Cell cell = grid.Cells.Where(i => i.Point.ID == solution.Variables.First()).First();

                        switch (solution.Sum)
                        {
                            case 0:
                                grid.OpenCell(cell);
                                break;
                            case 1:
                                cell.HasFlag = true;
                                break;
                            default:
                                throw new Exception();
                        }
                    }
                }
                else
                {
                    //Utility.WriteColor(grid.ShowKnown() + "\n");

                    Guesser guesser = new(grid);

                    Dictionary<int, double> scores = guesser.GetScore();

                    if (scores.ContainsValue(1) || scores.ContainsKey(0))
                    {
                        foreach (Cell safeCells in scores.Where(i => i.Value == 1).Select(i => i.Key).Select(i => grid.Cells.Where(j => j.Point.ID == i).First()))
                        {
                            grid.OpenCell(safeCells);
                        }

                        foreach (Cell minedCells in scores.Where(i => i.Value == 1).Select(i => i.Key).Select(i => grid.Cells.Where(j => j.Point.ID == i).First()))
                        {
                            minedCells.HasFlag = true;
                        }
                    }
                    else
                    {

                        double maxScore = scores.OrderByDescending(kvp => kvp.Value).First().Value;
                        List<Cell> maxScorers = scores.Where(i => i.Value == maxScore).Select(i => i.Key).Select(j => grid.Cells.Where(i => i.Point.ID == j).First()).ToList();
                        Cell toOpen = maxScorers.OrderBy(i => i.AdjacentCells.Count).ThenByDescending(i => i.AdjacentCells.Intersect(grid.OpenedCells).Count()).First();

                        grid.OpenCell(toOpen);
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
    }
}
