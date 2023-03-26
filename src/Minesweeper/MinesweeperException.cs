namespace Minesweeper
{
    using System;

    /// <summary>
    /// Defines an exception thrown in this project.
    /// </summary>
    public class MinesweeperException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MinesweeperException"/> class.
        /// </summary>
        public MinesweeperException()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MinesweeperException"/> class.
        /// </summary>
        /// <param name="message">A message about the exception.</param>
        public MinesweeperException(string message)
            : base(message)
        { }
    }
}