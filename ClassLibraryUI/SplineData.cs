using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibraryUI
{
    public struct SplineDataItem
    {
        public double PointCoordinate {get; set;}
        public double SplineValue { get; set;}
        public double FirstDerivativeValue { get; set;}
        public double SecondDerivativeValue { get; set;}
        public SplineDataItem(double pointCoordinate, double splineValue, double firstDerivativeValue, double secondDerivativeValue)
        {
            PointCoordinate = pointCoordinate;
            SplineValue = splineValue;
            FirstDerivativeValue = firstDerivativeValue;
            SecondDerivativeValue = secondDerivativeValue;
        }
        public string ToString(string format)
        {
            string pointCoordinateString = string.Format(format , PointCoordinate);
            string splineValueString = string.Format(format, SplineValue);
            string firstDerivativeValueString = string.Format(format, FirstDerivativeValue);
            string secondDerivativeValueString = string.Format(format, SecondDerivativeValue);
            return string.Format(format,
                $" Point  with coordinate {pointCoordinateString}:\n" +
                $" - spline = {splineValueString};\n" +
                $" - first derivative = {firstDerivativeValueString};\n" +
                $" - second derivative = {secondDerivativeValueString}");
        }
        public override string ToString()
        {
            string format = "{0:0.00}";
            return ToString(format);
        }
    }
    public class SplineData
    {
        public RawData? RawData { get; set;}
        public int NumberOfPoints { get; set;}
        public double IntegralValue { get; set; }
        public double[] FirstDerivativeOnSegmentEnds { get; set;} = new double[2];
        public List<SplineDataItem>? SplineDataItems { get; set;}
        
        public SplineData(RawData rawData, double leftEndFirstDerivative, double rightEndFirstDerivative, int numberOfPoints)
        {
            this.RawData = rawData;
            this.NumberOfPoints = numberOfPoints;
            FirstDerivativeOnSegmentEnds[0] = leftEndFirstDerivative;
            FirstDerivativeOnSegmentEnds[1] = rightEndFirstDerivative;
        }
 
        public void BuildSpline()
        {
            SplineDataItems = new List<SplineDataItem>();
            if (RawData == null || RawData.Points == null || RawData.ForceValues == null)
            {
                throw new Exception("Raw data can't be interpolated, because it has no points or is corrupted");
            }
            double[] leftIntegralEnds = { RawData.SegmentEnds[0] };
            double[] rightIntegralEnds = { RawData.SegmentEnds[1] };
            double[] interpolationResults = new double[3 * NumberOfPoints];
            double[] integralValues = new double[1];
            int errorCode = interpolate(RawData.NumberOfPoints,
                RawData.Points,
                RawData.ForceValues,
                FirstDerivativeOnSegmentEnds,
                NumberOfPoints,
                RawData.SegmentEnds,
                leftIntegralEnds,
                rightIntegralEnds,
                interpolationResults,
                integralValues);
            if (errorCode != 0)
            {
                throw new Exception($"Error with code {errorCode} happend during interpolation!");
            }
            double x = RawData.SegmentEnds[0];
            double step = (RawData.SegmentEnds[1] - RawData.SegmentEnds[0]) / (NumberOfPoints - 1);
            for (int i = 0; i < NumberOfPoints; ++i)
            {
                SplineDataItems.Add(new SplineDataItem(x,
                    interpolationResults[3 * i],
                    interpolationResults[3 * i + 1],
                    interpolationResults[3 * i + 2]));
                x += step;
            }
            IntegralValue = integralValues[0];

        }
        [DllImport("C:\\Users\\knorr\\source\\repos\\FirstLabUI\\x64\\Debug\\DllFirstLabUI.dll")]
        private static extern int interpolate(int numberOfInterpolationPoints,
            double[] interpolationPoints, double[] forceValuesInInterpolationPoints,
            double[] firstDerivativeOnSegmentEnds,
            int numberOfOutputPoints, double[] outputPointsSegmentEnds,
            double[] leftIntegralEnds, double[] rightIntegralEnds,
            double[] forceValuesInOutputPoints, double[] integralValues);
    }
}
