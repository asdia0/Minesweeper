using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json;

namespace Minesweeper.Solver
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine(GetWinRate(16, 16, 40));
        }

        public static List<double> GetWinRateData(int p, int q)
        {
            List<double> winRates = new();
            bool killWinRate = false;

            for (int m = 0; m < p * q; m++)
            {
                if (killWinRate)
                {
                    winRates.Add(0);
                }

                winRates.Add(GetWinRate(p, q, m + 1));

                // Set all subsequent winrates to 0
                if (m > 0.75 * p * q && winRates[m] > winRates[m - 1])
                {
                    winRates[m] = 0;
                    killWinRate = true;
                }
            }

            return winRates;
        }

        public static double GetWinRate(int length, int width, int mines)
        {
            int wins = 0;
            double previousWinRate = 0;
            int streak = 0;

            //for (int i = 1; i <= 10000; i++)
            for (int i = 1; i <= 1000; i++)
            {
                if (i % 100 == 0)
                {
                    Console.WriteLine(i);
                }

                wins += Solve(new(length, width, mines));
                double currentWinRate = (double)wins / i;
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
                Solver solver = new(grid);

                solver.SolveLogic();

                bool hasLogic = solver.Solutions.Any();

                // Update cells
                if (hasLogic)
                {
                    foreach ((int id, int sum) in solver.Solutions)
                    {
                        Cell cell = grid.Cells.Where(i => i.Point.ID == id).First();

                        switch (sum)
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
                else if (grid.State == State.ToBegin || grid.State == State.Ongoing)
                {
                    //Cell guess = GuessCell(grid);

                    Random rng = new();
                    Cell guess = grid.UnknownCells[rng.Next(grid.UnknownCells.Count)];

                    grid.OpenCell(guess);
                }
            }

            //Console.WriteLine(grid.ShowKnown());

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