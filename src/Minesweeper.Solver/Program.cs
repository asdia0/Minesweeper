using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Minesweeper.Solver
{
    public class Program
    {
        public const string FileName = "RawWinData.csv";

        public const int MaxAttempts = 10000;

        public static void Main()
        {
            GetWinRate(2, 2, 2);
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

            foreach ((int, int, int) dimension in remainingDimensions)
            {
                GetWinRate(dimension.Item1, dimension.Item2, dimension.Item3);
            }
        }

        public static List<(int, int, int)> GetValidDimensions(int maxDim)
        {
            List<(int, int, int)> res = [];

            for (int i = 1; i <= maxDim; i++)
            {
                for (int j = i; j <= maxDim; j++)
                {
                    for (int m = 1; m < i * j; m++)
                    {
                        res.Add((i, j, m));
                    }
                }
            }

            return res;
        }

        public static void GetWinRate(int length, int width, int mines)
        {
            int wins = 0;
            decimal previousWinRate = 0;
            int streak = 0;

            Console.WriteLine("---");

            for (int i = 1; i <= MaxAttempts; i++)
            {
                wins += Solve(new(length, width, mines));
                decimal currentWinRate = (decimal)wins / i;
                if (previousWinRate == currentWinRate)
                {
                    streak++;
                    if (streak == 50 && i > 2500)
                    {
                        EndDimension(length, width, mines, currentWinRate);
                    }
                }
                previousWinRate = currentWinRate;

                if (i % 100 == 0)
                {
                    Console.WriteLine($"{length}x{width}/{mines}: {wins} wins out of {i} attempts");
                }
            }

            EndDimension(length, width, mines, previousWinRate);
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
                Solver solver = new(grid);

                solver.Solve();

                bool hasLogic = solver.Solutions.Count != 0;

                // Update cells
                if (hasLogic)
                {
                    foreach (Solution solution in solver.Solutions)
                    {
                        Cell cell = grid.Cells.Where(i => i.Point.ID == solution.ID).First();

                        switch (solution.Assignment)
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
                    Random rng = new();
                    Cell guess = grid.UnknownCells[rng.Next(grid.UnknownCells.Count)];

                    grid.OpenCell(guess);

                    //Console.WriteLine(grid.ShowKnown());
                    //Console.WriteLine();
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
