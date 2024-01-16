using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Minesweeper.Solver
{
    public static class Utility
    {
        public static HashSet<HashSet<int>> GetGroups(HashSet<HashSet<int>> lists)
        {
            HashSet<HashSet<int>> groups = new();

            Dictionary<int, HashSet<int>> groupsOneDirectional = new();

            List<int> numbers = new();

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
                foreach (int intersections in groupsOneDirectional[num])
                {
                    if (groupsOneDirectional[intersections].Contains(num))
                    {
                        groups.Add(groupsOneDirectional[num]);
                    }
                }
            }

            return groups;
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
    }
}
