using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Minesweeper.Solver
{
    public class Program
    {
        public static void Main()
        {
            int maxLength = 10;
            int maxWidth = 10;

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

            return winRates.ToList();
        }

        public static double GetWinRate(Grid grid)
        {
            int wins = 0;
            double previousWinRate = 0;
            int streak = 0;

            for (int i = 1; i <= 10000; i++)
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

            return 0;
        }
    }
}