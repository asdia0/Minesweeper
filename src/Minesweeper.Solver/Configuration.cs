using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Minesweeper.Solver
{
    public struct Configuration
    {
        public Dictionary<int, int?> Assignments { get; set; }

        public Configuration(List<int> variables, HashSet<Constraint> solutions)
        {
            this.Assignments = new();

            foreach (int variable in variables)
            {
                this.Assignments.Add(variable, null);
            }

            foreach (Constraint constraint in solutions)
            {
                this.Assignments[constraint.Variables.First()] = constraint.Sum;
            }
        }

        public int Sum
        {
            get
            {
                return this.Assignments.Where(i => i.Value == 1).Count();
            }
        }

        public bool IsSolved
        {
            get
            {
                return this.Assignments.Where(i => i.Value == null).Count() == 0;
            }
        }

        /// <summary>
        /// Adds two configurations that are assumed to be solved.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Configuration operator +(Configuration left, Configuration right)
        {
            Configuration sum = new();
            sum.Assignments = left.Assignments.Concat(right.Assignments).ToDictionary(i => i.Key, i => i.Value);
            return sum;
        }
        //public bool Add(Configuration input, out Configuration output)
        //{
        //    HashSet<int> nullLHS = this.Assignments.Where(i => i.Value == null).Select(i => i.Key).ToHashSet();
        //    HashSet<int> nullRHS = input.Assignments.Where(i => i.Value == null).Select(i => i.Key).ToHashSet();

        //    if (nullLHS.IsSupersetOf(nullRHS))
        //    {

        //        return true;
        //    }
        //    else if (nullRHS.IsSupersetOf(nullLHS))
        //    {
        //        return true;
        //    }

        //    output = new();
        //    return false;
        //}
    }
}
