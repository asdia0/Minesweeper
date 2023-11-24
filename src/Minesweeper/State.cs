namespace Minesweeper
{
    /// <summary>
    /// Represents a <see cref="Grid">grid</see> state.
    /// </summary>
    public enum State
    {
        /// <summary>
        /// The <see cref="Grid">grid</see> has ended and is a win.
        /// </summary>
        Success,

        /// <summary>
        /// The <see cref="Grid">grid</see> has ended and is a loss.
        /// </summary>
        Fail,

        /// <summary>
        /// The <see cref="Grid">grid</see> has started but has not ended.
        /// </summary>
        Ongoing,

        /// <summary>
        /// The <see cref="Grid">grid</see> is yet to start.
        /// </summary>
        ToBegin,
    }
}