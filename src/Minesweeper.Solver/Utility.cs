using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    }
}
