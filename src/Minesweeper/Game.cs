namespace Minesweeper
{
    using System;

    /// <summary>
    /// Represents a game of minesweeper.
    /// </summary>
    public class Game
    {
        private DateTime? end = null;

        private bool endSet = false;

        private DateTime? start = null;

        private bool startSet = false;

        /// <summary>
        /// Gets the <see cref="Grid">grid</see> the <see cref="Game">game</see> is played on.
        /// </summary>
        public Grid Grid { get; init; }

        /// <summary>
        /// Gets or sets the current <see cref="Game">game</see> <see cref="State">state</see>. Return `null` if the game has not started (no <see cref="Cell">cells</see> have been opened).
        /// </summary>
        public State? State { get; set; } = null;

        /// <summary>
        /// Gets or sets the time at which the game started (A <see cref="Cell">cell</see> has been opened).
        /// Returns `null` if the game has not started.
        /// </summary>
        public DateTime? Start
        {
            get
            {
                return this.start;
            }

            set
            {
                if (!this.startSet)
                {
                    this.start = value;
                    this.startSet = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the time at which the <see cref="Game">game</see> ended (<see cref="State"/> changed to <see cref="State.Success"/> or <see cref="State.Fail"/>).
        /// Returns `null` if the game is still <see cref="State.Ongoing">ongoing</see>.
        /// </summary>
        public DateTime? End
        {
            get
            {
                return this.end;
            }

            set
            {
                if (!this.endSet)
                {
                    this.end = value;
                    this.endSet = true;
                }
            }
        }

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

            this.Grid = new(this, length, width, mines);
        }
    }
}