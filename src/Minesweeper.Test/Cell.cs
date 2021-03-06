namespace Minesweeper.Test
{
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TCell
    {
        [TestMethod]
        public void Cell()
        {
            // Create a grid.
            Game game = new(5, 5, 1);
            Grid grid = game.Grid;

            // Select the first cell from the grid.
            Cell cell = grid.Cells[0];

            // Check that the cell's point is (0, 0).
            Assert.AreEqual(new Point(5, 5, (0, 0)), cell.Point);

            // Check the cell's grid.
            Assert.AreEqual(grid, cell.Grid);
        }

        [TestMethod]
        public void AdjacentCells_Corner()
        {
            // Create a grid.
            Game game = new(5, 5, 1);
            Grid grid = game.Grid;

            // Select the first cell from the grid.
            Cell cell = grid.Cells[0];

            // Check that there are 3 adjacent cells.
            Assert.AreEqual(3, cell.AdjacentCells.Count);
        }

        [TestMethod]
        public void AdjacentCells_Side()
        {
            // Create a grid.
            Game game = new(5, 5, 1);
            Grid grid = game.Grid;

            // Select the first cell from the grid.
            Cell cell = grid.Cells[1];

            // Check that there are 5 adjacent cells.
            Assert.AreEqual(5, cell.AdjacentCells.Count);
        }

        [TestMethod]
        public void AdjacentCells_Middle()
        {
            // Create a grid.
            Game game = new(5, 5, 1);
            Grid grid = game.Grid;

            // Select the first cell from the grid.
            Cell cell = grid.Cells[12];

            // Check that there are 8 adjacent cells.
            Assert.AreEqual(8, cell.AdjacentCells.Count);
        }

        [TestMethod]
        public void Count_Mine()
        {
            // Create a grid.
            Game game = new(5, 5, 1);
            Grid grid = game.Grid;

            // Select the cell with the mine.
            Cell cell = grid.Cells.Where(cell => cell.HasMine).First();

            // Check that cell.Count returns null.
            Assert.IsNull(cell.Count);
        }

        [TestMethod]
        public void Count_Positive()
        {
            // Create a grid.
            Game game = new(2, 2, 2);
            Grid grid = game.Grid;

            // Select a cell without a mine.
            Cell cell = grid.Cells.Where(cell => !cell.HasMine).First();

            // Check that there the cell's count is 2.
            Assert.AreEqual(2, cell.Count);
        }

        [TestMethod]
        public void Count_Zero()
        {
            // Create a grid.
            Game game = new(10, 10, 1);
            Grid grid = game.Grid;

            // Select the cell with the mine.
            Cell cell = grid.Cells.Where(cell => cell.HasMine).First();

            // Get the half the cell with the mine is in.
            // Bottom half (false) - sum of coordinates <= 10
            // Upper half (true) - sum of coordinates > 10

            bool half = (cell.Point.Coordinates.X + cell.Point.Coordinates.Y) > 10;

            // Check the corner cell in the opposite half of the cell with the mine.
            if (half) // Check cell in bottom half.
            {
                Assert.AreEqual(0, grid.Cells[0].Count);
            }
            else // Check cell in upper half.
            {
                Assert.AreEqual(0, grid.Cells[99].Count);
            }
        }
    }
}
