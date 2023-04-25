using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text.Json.Serialization;

namespace ClassLibraryUI
{
    public static class ForceFunctions
    {
        static public double[] linearCoefficients = { 1, 0 };
        static public double[] cubicCoefficients = {1, 0, 0, 0};
        static public Random randomSeed = new Random();
        static public double linearFunction(double x)
        {
            return x * linearCoefficients[0] + linearCoefficients[1]; 
        }
        static public double cubicFunction(double x) 
        {
            double result = 0;
            for (int i = 0; i <= 3; ++i)
            {
                result += Math.Pow(x, 3 - i) * cubicCoefficients[i];
            }
            return result; 
        }
        static public double randomFunction(double x) 
        {
            return randomSeed.NextDouble();
        }
    }

    public delegate double FRaw(double x);
    public enum FRawEnum { linearFunction, cubicFunction, randomFunction }
    [Serializable]  
    public class RawDataItem
    {
        public double Coordinate { get; set; }
        public double Force { get; set; }
        public RawDataItem(double coordinate, double force)
        {
            Coordinate = coordinate;
            Force = force;
        }
    }
    [Serializable]
    public class RawData
    {
        public double[] SegmentEnds { get; set; }
        public int NumberOfPoints { get; set; }
        public bool IsUniform { get; set; }
        public FRawEnum ForceName { get; set; }
        [JsonIgnore]
        public double[]? Points { get; set; }
        [JsonIgnore]
        public double[]? ForceValues { get; set; }
        [JsonIgnore]
        public List<RawDataItem>? RawDataItems { get; set; }
        public RawData(double[] segmentEnds, int numberOfPoints, bool isUniform, FRawEnum forceName)
        {
            SegmentEnds = segmentEnds;
            NumberOfPoints = numberOfPoints;
            IsUniform = isUniform;
            ForceName = forceName;
        }
        public void ComputeRawData()
        {
            MethodInfo? method = typeof(ForceFunctions).GetMethod(ForceName.ToString());
            if (method is null)
            {
                return;
            }
            FRaw force = (FRaw)Delegate.CreateDelegate(typeof(FRaw), method);
            ForceValues = new double[NumberOfPoints];
            Points = new double[NumberOfPoints];
            RawDataItems = new List<RawDataItem>();
            double step = (SegmentEnds[1] - SegmentEnds[0]) / (NumberOfPoints - 1);
            double x = SegmentEnds[0];
            for (int i = 0; i < NumberOfPoints; ++i)
            {
                Points[i] = x;
                ForceValues[i] = force(x);
                RawDataItems.Add(new RawDataItem(x, ForceValues[i]));
                x += step;
            }
        }

        public RawData(string filename)
        {
            RawData? rawData = null;
            
            try { Load(filename, out rawData); }
            catch (Exception) { }

            if (rawData != null) {
                SegmentEnds = rawData.SegmentEnds;
                NumberOfPoints = rawData.NumberOfPoints;
                IsUniform = rawData.IsUniform;
                Points = rawData.Points;
                ComputeRawData();
            } else
            {
                double[] segmentEndsDefaultValues = { 0, 1 };
                SegmentEnds = segmentEndsDefaultValues;
                Console.WriteLine("Initializing array using default values.");
            }

        }
        public void Save(string filename)
        {
            FileStream? fileStream = null;
            try
            {
                fileStream = File.Create(filename);
                DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(RawData));
                jsonSerializer.WriteObject(fileStream, this);
            }
            finally
            {
                fileStream?.Close();
            }
        }
        public static void Load(string filename, out RawData? rawData)
        {
            FileStream? fileStream = null;
            rawData = null;
            bool answer = false;
            try
            {
                fileStream = File.OpenRead(filename);
                DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(RawData));
                rawData = (RawData?)jsonSerializer.ReadObject(fileStream);
                answer = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Load failed:\n " + ex.Message);

            }
            finally
            {
                fileStream?.Close();
                if (!answer)
                {
                    throw new Exception();
                }
            }
        }
    }

}