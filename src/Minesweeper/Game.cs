namespace Minesweeper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a game of minesweeper.
    /// </summary>
    public class Game
    {
        /// <summary>
        /// Gets the <see cref="Grid">grid</see> the <see cref="Game">game</see> is played on.
        /// </summary>
        public Grid Grid { get; init; }

        /// <summary>
        /// Gets or sets the current <see cref="Game">game</see> <see cref="State">state</see>. Return `null` if the game has not started (no <see cref="Cell">cells</see> have been opened).
        /// </summary>
        public State State { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Game"/> class.
        /// </summary>
        /// <param name="length">The length (y-axis) of the <see cref="Grid">grid </see> measured in <see cref="Cell">cells</see>.</param>
        /// <param name="width">The width (x-axis) of the <see cref="Grid">grid </see> measured in <see cref="Cell">cells</see>.</param>
        /// <param name="mines">The number of mines on the <see cref="Grid">grid</see>.</param>
        public Game(int length, int width, int mines)
        {
            // Catch invalid parameters.
            Utility.CheckGridParams(length, width, mines);

            this.Grid = new(length, width, mines);
            this.State = State.ToBegin;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Game"/> class.
        /// </summary>
        /// <param name="grid">The <see cref="Grid"/> the game is played on.</param>
        public Game(Grid grid)
        {
            this.Grid = grid;
            this.State = State.ToBegin;
        }
    }
}