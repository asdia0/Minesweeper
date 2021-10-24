namespace Minesweeper
{
    public class Cell
    {
        public bool IsFlagged = false;

        public bool HasMine;

        public bool IsSearched = false;

        public Cell(bool hasMine)
        {
            this.HasMine = hasMine;
        }

        public void SwitchFlagState()
        {
            this.IsFlagged ^= true;
        }
    }
}