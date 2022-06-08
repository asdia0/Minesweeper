namespace Minesweeper.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TUtility
    {
        [TestMethod]
        public void CheckGridParams()
        {
            // Zero length.
            Assert.ThrowsException<MinesweeperException>(() =>
            {
                Utility.CheckGridParams(0, 1, 1);
            });

            // Negative length.
            Assert.ThrowsException<MinesweeperException>(() =>
            {
                Utility.CheckGridParams(-1, 1, 1);
            });

            // Zero width.
            Assert.ThrowsException<MinesweeperException>(() =>
            {
                Utility.CheckGridParams(1, 0, 1);
            });

            // Negative length.
            Assert.ThrowsException<MinesweeperException>(() =>
            {
                Utility.CheckGridParams(1, -1, 1);
            });

            // Zero mines.
            Assert.ThrowsException<MinesweeperException>(() =>
            {
                Utility.CheckGridParams(1, 1, 0);
            });

            // Negative mines.
            Assert.ThrowsException<MinesweeperException>(() =>
            {
                Utility.CheckGridParams(1, 1, -1);
            });
        }

        [TestMethod]
        public void CellIndexToCoordinates()
        {
            // Set length and width.
            int length = 7;
            int width = 8;

            // Check that the index to coordinates conversion is correct.
            (int x, int y) coordinates = (0, 0);
            int index = 0;

            for (int y = 0; y < length; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Assert.AreEqual((x, y), Utility.CellIndexToCoordinates(index, width));
                    index++;
                    coordinates.x++;
                }

                coordinates.y++;
            }
        }

        [TestMethod]
        public void CellCoordinatesToIndex()
        {
            // Set length and width.
            int length = 7;
            int width = 8;

            // Check that the coordinates to index conversion is correct.
            (int x, int y) coordinates = (0, 0);
            int index = 0;

            for (int y = 0; y < length; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Assert.AreEqual(index, Utility.CellCoordinatesToIndex((x, y), width));
                    index++;
                    coordinates.x++;
                }

                coordinates.y++;
            }
        }
    }
}
