namespace Minesweeper.Interaction
{
    using System;
    using Minesweeper;

    class Program
    {
        static void Main()
        {
            Grid board = new(5, 5, 5);

            while (!board.IsOver && !board.IsFinished)
            {
                Console.WriteLine(board);
                Console.WriteLine("Enter the coordinates of the cell to search.");
                string tupleString = Console.ReadLine();
                int x = int.Parse(tupleString.Split(",")[0]);
                int y = int.Parse(tupleString.Split(",")[1]);
                board.Cells[y * board.Breadth + x].Search();
                Console.Clear();
            }

            if (board.IsOver)
            {
                Console.WriteLine("You have hit a mine!");
            }
            else if (board.IsFinished)
            {
                Console.WriteLine("Congratulations! You have completed the game.");
            }

            Console.WriteLine(board);
        }
    }
}
