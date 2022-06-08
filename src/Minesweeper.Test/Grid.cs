namespace Minesweeper.Test
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    
    [TestClass]
    public class TGrid
    {
        [TestMethod]
        public void Grid()
        {
            // Create a grid.
            Game game = new(5, 6, 7);
            Grid grid = game.Grid;

            // Check the dimensions of the grid.
            Assert.AreEqual(5, grid.Length);
            Assert.AreEqual(6, grid.Width);

            // Check the number of cells.
            Assert.AreEqual(30, grid.Cells.Count);

            // Check the number of mines on the grid.
            Assert.AreEqual(7, grid.Cells.Where(cell => cell.HasMine).Count());

            // Check that the game has not yet started.
            Assert.AreEqual(null, game.State);
        }

        [TestMethod]
        public void OpenCell_PositiveCount()
        {
            // Create a grid.
            Game game = new(5, 5, 1);
            Grid grid = game.Grid;

            // Select cell which has one neighbouring mine.
            Cell cell = grid.Cells.Where(cell => cell.Count == 1).First();

            // Check then when opened, only one cell is opened.
            grid.OpenCell(cell);
            Assert.AreEqual(1, grid.Cells.Where(cell => cell.IsOpen).Count());

            // Check that the game has started.
            Assert.AreEqual(State.Ongoing, game.State);
        }

        [TestMethod]
        public void OpenCell_ZeroCount()
        {
            // Create a grid.
            Game game = new(5, 5, 1);
            Grid grid = game.Grid;

            // Select cell which has no neighbouring mines.
            Cell cell = grid.Cells.Where(cell => cell.Count == 0).First();

            // Check then when opened, only a certain number of cells are opened.
            grid.OpenCell(cell);

            // List all the possible numbers of opened cells.
            List<int> possibilities = new()
            {
                21,
                23,
                24,
            };

            int count = grid.Cells.Where(cell => cell.IsOpen).Count();

            CollectionAssert.Contains(possibilities, count);

            // Check that game has been won if all non-mine cells have been opened.
            if (count == 24)
            {
                Assert.AreEqual(State.Success, game.State);
            }
        }

        [TestMethod]
        public void OpenCell_Mine()
        {
            // Create a grid.
            Game game = new(5, 5, 1);
            Grid grid = game.Grid;

            // Select cell which has a mine.
            Cell cell = grid.Cells.Where(cell => cell.HasMine).First();

            // Check then when opened, only one cell is opened.
            grid.OpenCell(cell);
            Assert.AreEqual(1, grid.Cells.Where(cell => cell.IsOpen).Count());

            // Check that the game has ended.
            Assert.AreEqual(State.Fail, game.State);
        }
    }
}
