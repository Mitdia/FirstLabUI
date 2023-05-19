using Xunit;
using ClassLibraryUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibraryUITests
{
    public class SplineDataTests
    {
        private bool CompareDouble(double expected, double actual)
        {
            double tolerance = 0.0001; // set a small tolerance value
            return Math.Abs(expected - actual) < tolerance;
        }
        [Fact]
        public void BuildSplineTest()
        {
            RawData rawData = new RawData(new double[] { 0.0, 1.0 }, 10, true, FRawEnum.linearFunction);
            rawData.ComputeRawData();
            SplineData splineData = new SplineData(rawData, 1, 1, 15);
            Assert.NotNull(splineData.RawData);
            Assert.NotNull(splineData.RawData.ForceValues);
            splineData.BuildSpline();
            Assert.NotNull(splineData.SplineDataItems);
            Assert.Equal(15, splineData.SplineDataItems.Count);
            Assert.True(CompareDouble(0.0, splineData.SplineDataItems[0].PointCoordinate));
            Assert.True(CompareDouble(1.0, splineData.SplineDataItems[14].PointCoordinate));
            Assert.True(CompareDouble(splineData.RawData.ForceValues[0], splineData.SplineDataItems[0].SplineValue));
            Assert.True(CompareDouble(splineData.RawData.ForceValues[9], splineData.SplineDataItems[14].SplineValue));
            Assert.Equal(15, splineData.NumberOfPoints);
            Assert.Equal(0.5, splineData.IntegralValue);
            Assert.True(CompareDouble(1.0, splineData.FirstDerivativeOnSegmentEnds[0]));
            Assert.True(CompareDouble(1.0, splineData.FirstDerivativeOnSegmentEnds[1]));
        }
    }
}