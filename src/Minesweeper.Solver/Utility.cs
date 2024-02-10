using System;
using System.Collections.Generic;
using System.Linq;
using Fractions;

namespace Minesweeper.Solver
{
    /// <summary>
    /// A class for utility functions.
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Converts an ID into its corresponding <see cref="Cell">cell</see>.
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        public static Cell IDToCell(Grid grid, int ID)
        {
            return grid.Cells.Where(i => i.Point.ID == ID).First();
        }

        /// <summary>
        /// Converts a cell into its corresponding ID.
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public static int CellToID(Cell cell)
        {
            return cell.Point.ID;
        }

        /// <summary>
        /// Converts a list of IDs into their corresponding <see cref="Cell">cells</see>.
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="IDs"></param>
        /// <returns></returns>
        public static IEnumerable<Cell> IDsToCells(Grid grid, IEnumerable<int> IDs)
        {
            return IDs.Select(i => grid.Cells.Where(j => j.Point.ID == i).First());
        }

        /// <summary>
        /// Converts a list of cells into their corresponding IDs.
        /// </summary>
        /// <param name="cells"></param>
        /// <returns></returns>
        public static IEnumerable<int> CellsToIDs(IEnumerable<Cell> cells)
        {
            return cells.Select(i => i.Point.ID);
        }

        /// <summary>
        /// Gets a list of disjoint subsets.
        /// </summary>
        /// <param name="lists"></param>
        /// <returns></returns>
        public static HashSet<HashSet<int>> GetGroups(HashSet<HashSet<int>> lists)
        {
            HashSet<HashSet<int>> groups = [];

            Dictionary<int, HashSet<int>> groupsOneDirectional = [];

            List<int> numbers = lists.SelectMany(i => i).Distinct().ToList();

            foreach (int num in numbers)
            {
                HashSet<HashSet<int>> groupsWithNum = lists.Where(i => i.Contains(num)).ToHashSet();

                HashSet<int> intersection = groupsWithNum
                    .Skip(1)
                    .Aggregate(
                        new HashSet<int>(groupsWithNum.First()),
                        (h, e) => { h.IntersectWith(e); return h; }
                    );

                groupsOneDirectional.Add(num, intersection);
            }

            foreach (int num in numbers)
            {
                if (!groups.Where(i => i.Contains(num)).Any())
                {
                    continue;
                }

                bool addGroup = true;

                foreach (int intersections in groupsOneDirectional[num])
                {
                    if (!groupsOneDirectional[intersections].Contains(num))
                    {
                        addGroup = false;
                        break;
                    }
                }

                if (addGroup)
                {
                    groups.Add(groupsOneDirectional[num]);
                }
            }

            return groups.Where(i => i.Count > 1).ToHashSet();
        }

        /// <summary>
        /// Prints a coloured <see cref="Grid.ShowKnown"/>.
        /// </summary>
        /// <param name="message"></param>
        public static void WriteColor(Grid grid)
        {
            string message = grid.ShowKnown();

            for (int i = 0; i < message.Length; i++)
            {
                char c = message[i];

                Console.ForegroundColor = c switch
                {
                    'F' => ConsoleColor.Red,
                    '0' => ConsoleColor.DarkGray,
                    '?' => ConsoleColor.Yellow,
                    _ => ConsoleColor.White,
                };
                Console.Write(c);
                Console.ResetColor();
            }

            Console.Write("\n\n");
        }

        /// <summary>
        /// Returns the binomial coefficient of <paramref name="n"/> and <paramref name="k"/>.
        /// </summary>
        /// <param name="n"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        public static double Choose(int n, int k)
        {
            if (k > n - k) k = n - k; // because C(n, r) == C(n, n - r)
            double ans = 1;
            int i;

            for (i = 1; i <= k; i++)
            {
                ans *= n - k + i;
                ans /= i;
            }

            return ans;
        }

        /// <summary>
        /// Gets the mediant of two fractions.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Fraction Mediant(Fraction left, Fraction right)
        {
            return new(left.Numerator + right.Numerator, left.Denominator + right.Denominator, true);
        }

        /// <summary>
        /// Returns a list of all proper fractions in the <paramref name="n"/>-th Stern-Bocrot sequence.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static List<Fraction> GenerateLeftSternBocrotSequence(int n)
        {
            if (n < 1)
            {
                return [];
            }

            if (n == 1)
            {
                return [new(0, 1, false), new(1, 2), new(1, 1, false)];
            }

            List<Fraction> previousSequence = GenerateLeftSternBocrotSequence(n - 1);
            List<Fraction> currentSequence = [];

            for (int i = 1; i <= previousSequence.Count - 2; i++)
            {
                if (i % 2 == 0)
                {
                    continue;
                }

                currentSequence.Add(Mediant(previousSequence[i - 1], previousSequence[i]));
                currentSequence.Add(Mediant(previousSequence[i], previousSequence[i + 1]));
            }

            currentSequence.AddRange(previousSequence);

            currentSequence.Sort();

            return currentSequence;
        }
    }
}
