namespace Minesweeper.Solver
{
    using System.Collections.Generic;
    using System.Linq;

    internal class Utility
    {
        /// <summary>
        /// Normalises a list.
        /// </summary>
        /// <param name="distribution">The list to normalise.</param>
        /// <returns></returns>
        public static List<double> NormaliseCount(List<double> distribution)
        {
            double max = distribution.Max();
            double min = distribution.Min();

            return distribution.Select(i => (double)(i - min)/(max - min)).ToList();
        } 
    }
}
