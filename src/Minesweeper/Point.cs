namespace Minesweeper
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a particular point on a lattice.
    /// </summary>
    public record Point
    {
        /// <summary>
        /// Gets the maximum y-value the <see cref="Coordinates">coordinates</see> can take on.
        /// </summary>
        public int Length { get; init; }

        /// <summary>
        /// Gets the maximum x-value the <see cref="Coordinates">coordinates</see> can take on.
        /// </summary>
        public int Width { get; init; }

        /// <summary>
        /// Gets the coordinates of the <see cref="Point">point</see> on the lattice. The x- and y-values start from 0.
        /// </summary>
        public (int X, int Y) Coordinates { get; init; }

        /// <summary>
        /// Gets the unique identifier for the <see cref="Point"/>.
        /// </summary>
        public int ID
        {
            get
            {
                return (this.Width * this.Coordinates.Y) + this.Coordinates.X;
            }
        }

        /// <summary>
        /// Gets a list of <see cref="Coordinates">coordinates</see> adjacent to the current <see cref="Point">point</see>. Points diagonal to the current point are considered adjacent.
        /// </summary>
        public List<(int X, int Y)> AdjacentPoints
        {
            get
            {
                // Create a list of candidate points.
                int x = this.Coordinates.X;
                int y = this.Coordinates.Y;

                List<(int x, int y)> points =
                [
                    (x - 1, y - 1),
                    (x - 1, y),
                    (x - 1, y + 1),
                    (x, y - 1),
                    (x, y + 1),
                    (x + 1, y - 1),
                    (x + 1, y),
                    (x + 1, y + 1),
                ];

                // Keep valid points.
                return points.Where(coor => coor.x >= 0 && coor.x < this.Width && coor.y >= 0 && coor.y < this.Length).ToList();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Point"/> class.
        /// </summary>
        /// <param name="length">The maximum y-value the <see cref="Coordinates">coordinates</see> can take on.</param>
        /// <param name="width">The maximum x-value the <see cref="Coordinates">coordinates</see> can take on.</param>
        /// <param name="coordinates">The coordinates of the <see cref="Point">point</see> on the lattice. The x- and y-values start from 0.</param>
        public Point(int length, int width, (int x, int y) coordinates)
        {
            // Catch invalid parameters.
            Utility.CheckGridParams(length, width, 1);

            // Assign properties.
            this.Length = length;
            this.Width = width;
            this.Coordinates = coordinates;
        }
    }
}
