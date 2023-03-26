namespace Minesweeper.Analysis
{
    using System.Collections.Generic;
    using System.Linq;

    internal class Utility
    {
        /// <summary>
        /// Normalises a dataset.
        /// </summary>
        /// <param name="distribution">The dataset to normalise.</param>
        /// <returns></returns>
        public static List<(double, double)> NormaliseCount(List<(double, double)> distribution)
        {
            double max1 = distribution.Max(i => i.Item1);
            double min1 = distribution.Min(i => i.Item1);

            double max2 = distribution.Max(i => i.Item2);
            double min2 = distribution.Min(i => i.Item2);

            return distribution.Select(i => ((double)(i.Item1 - min1)/(max1 - min1), (double)(i.Item2 - min2)/(max2 - min2))).ToList();
        } 
    }
}
