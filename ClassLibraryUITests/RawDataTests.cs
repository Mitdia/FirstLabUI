using Xunit;
using ClassLibraryUI;

namespace ClassLibraryUITests
{
    public class RawDataTests
    {

        private bool CompareDouble(double expected, double actual)
        {
            double tolerance = 0.0001; // set a small tolerance value
            return Math.Abs(expected - actual) < tolerance;
        }
        [Fact]
        // Test that RawData ComputeRawData() works
        public void ComputeRawDataTest()
        {
            // Arrange
            RawData rawData = new RawData(new double[] { 0.0, 1.0 }, 10, true, FRawEnum.linearFunction);

            // Act
            rawData.ComputeRawData();

            // Assert
            Assert.NotNull(rawData.Points);
            Assert.NotNull(rawData.ForceValues);
            Assert.NotNull(rawData.RawDataItems);
            Assert.True(CompareDouble(10, rawData.Points.Length));
            Assert.True(CompareDouble(0.0, rawData.Points[0]));
            Assert.True(CompareDouble(1.0, rawData.Points[9]));
            Assert.Equal(10, rawData.ForceValues.Length);
            Assert.Equal(10, rawData.RawDataItems.Count);
            Assert.True(CompareDouble(0.0, rawData.RawDataItems[0].Coordinate));
            Assert.True(CompareDouble(0.0, rawData.RawDataItems[0].Force));
            Assert.True(CompareDouble(1.0, rawData.RawDataItems[9].Coordinate));
            Assert.True(CompareDouble(1.0, rawData.RawDataItems[9].Force));
        }
    }
}
