using System;
using System.Collections.Generic;
using System.Linq;
using Fractions;

namespace Minesweeper.Solver
{
    public static class Utility
    {
        public static HashSet<HashSet<int>> GetGroups(HashSet<HashSet<int>> lists)
        {
            HashSet<HashSet<int>> groups = new();

            Dictionary<int, HashSet<int>> groupsOneDirectional = new();

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

        public static void WriteColor(string message)
        {
            for (int i = 0; i < message.Length; i++)
            {
                char c = message[i];

                switch (c)
                {
                    case 'F':
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case '0':
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        break;
                    case '?':
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                }

                Console.Write(c);
                Console.ResetColor();
            }

            Console.WriteLine();
        }

        public static double nCr(int n, int r)
        {
            double tmp = 1;
            int j = 2;
            int k = n - r;
            for (int i = n; i > k; i--)
            {
                tmp *= i;
                while (j <= r && tmp % j == 0)
                {
                    tmp /= j++;
                }
            }
            while (j <= r)
            {
                tmp /= j++;
            }
            return tmp;
        }

        public static Fraction Mediant(Fraction left, Fraction right)
        {
            return new(left.Numerator + right.Numerator, left.Denominator + right.Denominator, true);
        }

        public static List<Fraction> GenerateLeftSternBocrotSequence(int order)
        {
            if (order < 1)
            {
                return [];
            }

            if (order == 1)
            {
                return [new(0, 1, false), new(1, 2), new(1, 1, false)];
            }

            List<Fraction> previousSequence = GenerateLeftSternBocrotSequence(order - 1);
            List<Fraction> currentSequence = [];

            for (int i = 1; i <= previousSequence.Count - 2; i++)
            {
                if (i % 2 == 0)
                {
                    continue;
                }

                currentSequence.Add(Utility.Mediant(previousSequence[i - 1], previousSequence[i]));
                currentSequence.Add(Utility.Mediant(previousSequence[i], previousSequence[i + 1]));
            }

            currentSequence.AddRange(previousSequence);

            currentSequence.Sort();

            return currentSequence;
        }
    }
}
