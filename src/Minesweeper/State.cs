namespace Minesweeper
{
    /// <summary>
    /// Represents a <see cref="Game">game</see> state.
    /// </summary>
    public enum State
    {
        /// <summary>
        /// The <see cref="Game">game</see> has been won.
        /// </summary>
        Success,

        /// <summary>
        /// The <see cref="Game">game</see> has been lost.
        /// </summary>
        Fail,

        /// <summary>
        /// The <see cref="Game">game</see> is still ongoing.
        /// </summary>
        Ongoing,
    }
}