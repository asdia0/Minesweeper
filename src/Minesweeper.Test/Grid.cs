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
            Grid grid = new(5, 6, 1);

            // Check the dimensions of the grid.
            Assert.AreEqual(5, grid.Length);
            Assert.AreEqual(6, grid.Width);

            // Check the number of cells.
            Assert.AreEqual(30, grid.Cells.Count);

            // Check the number of mines on the grid.
            Assert.AreEqual(7, grid.Cells.Where(cell => cell.HasMine).Count());

            // Check that the game has not yet started.
            Assert.IsNull(grid.State);
        }

        [TestMethod]
        public void OpenCell_PositiveMineCount()
        {
            // Create a grid.
            Grid grid = new(5, 5, 1);

            // Select cell which has one neighbouring mine.
            Cell cell = grid.Cells.Where(cell => cell.MineCount == 1).First();

            // Check then when opened, only one cell is opened.
            grid.OpenCell(cell);
            Assert.AreEqual(1, grid.OpenedCells.Count());

            // Check that the game has started.
            Assert.AreEqual(State.Ongoing, grid.State);
        }

        [TestMethod]
        public void OpenCell_Opening()
        {
            // Create a grid.
            Grid grid = new(5, 5, 1);

            // Select cell which has no neighbouring mines.
            Cell cell = grid.Cells.Where(cell => cell.MineCount == 0).First();

            // Check then when opened, only a certain number of cells are opened.
            grid.OpenCell(cell);

            // List all the possible numbers of opened cells.
            List<int> possibilities = new()
            {
                21,
                23,
                24,
            };

            int count = grid.OpenedCells.Count();

            CollectionAssert.Contains(possibilities, count);

            // Check that game has been won if all non-mine cells have been opened.
            if (count == 24)
            {
                Assert.AreEqual(State.Success, grid.State);
            }
        }

        [TestMethod]
        public void OpenCell_Mine()
        {
            // Create a grid.
            Grid grid = new(5, 5, 1);

            // Select cell which has a mine.
            Cell cell = grid.Cells.Where(cell => cell.HasMine).First();

            // Check then when opened, only one cell is opened.
            grid.OpenCell(cell);
            Assert.AreEqual(1, grid.OpenedCells.Count());

            // Check that the game has ended.
            Assert.AreEqual(State.Fail, grid.State);
        }

        [TestMethod]
        public void OpenCell_Flag()
        {
            // Create a grid.
            Grid grid = new(5, 5, 1);

            // Select a cell which is at least 2 cells away from a mine
            Cell cell = grid.Cells.Where(cell => cell.MineCount == 0 && !cell.AdjacentCells.Where(adjCell => adjCell.MineCount != 0).Any()).First();

            // Flag an adjacent cell
            Cell flag = cell.AdjacentCells[0];
            flag.HasFlag = true;

            // Check then when opened, only a certain number of cells are opened.
            grid.OpenCell(cell);

            // List all the possible numbers of opened cells.
            // Same possibilities as OpenCell_ZeroCount, but with 1 less cell.
            List<int> possibilities = new()
            {
                20,
                22,
                23,
            };

            int count = grid.OpenedCells.Count();

            CollectionAssert.Contains(possibilities, count);

            // Check that cell with flag has not been opened.
            Assert.IsFalse(flag.IsOpen);

            // Check that game has been won if all non-mine cells have been opened.
            if (count == 23)
            {
                Assert.AreEqual(State.Success, grid.State);
            }
        }

        [TestMethod]
        public void Chord_UnequalFlags()
        {
            // Create a grid.
            Grid grid = new(2, 2, 1);

            // Flag the cell with the mine.
            grid.Cells.Where(cell => cell.HasMine).First().HasFlag = true;

            // Flag another cell.
            grid.Cells.Where(cell => !cell.HasMine).First().HasFlag = true;

            // Chord a separate cell.
            grid.Chord(grid.Cells.Where(cell => !cell.HasMine).Last());

            // Check that no cells are opened.
            Assert.AreEqual(0, grid.OpenedCells.Count());
        }

        [TestMethod]
        public void Chord_EqualFlags()
        {
            // Create a grid.
            Grid grid = new(2, 2, 1);

            // Flag the cell with the mine.
            grid.Cells.Where(cell => cell.HasMine).First().HasFlag = true;

            // Chord a separate cell.
            grid.Chord(grid.Cells.Where(cell => !cell.HasMine).First());

            // Check that all cells are opened and the game has ended.
            Assert.AreEqual(3, grid.OpenedCells.Count());
            Assert.AreEqual(State.Success, grid.State);
        }
    }
}
