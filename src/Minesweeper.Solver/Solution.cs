namespace Minesweeper.Solver
{
    public struct Solution(int ID, int Assignment)
    {
        public int ID { get; set; } = ID;

        public int Assignment { get; set; } = Assignment;
    }
}
