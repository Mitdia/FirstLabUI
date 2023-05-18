using System.Collections.ObjectModel;
using System.ComponentModel;
using ClassLibraryUI;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;

namespace ViewModelFirstLabUI;

public interface IUIServices
{
    // void ReportError(string message);
    void PlotLineSeries(LineSeries series);
    void PlotScatterSeries(ScatterSeries series);
}


public class MainViewModel : ViewModelBase, IDataErrorInfo
{
    public IUIServices uiServices;
    public double[] SegmentEnds
    {
        get => RawDataSource.SegmentEnds;
        set => RawDataSource.SegmentEnds = value;
    }
    public int NumberOfInitialPoints
    {
        get => RawDataSource.NumberOfPoints;
        set => RawDataSource.NumberOfPoints = value;
    }
    public bool IsGridUniform
    {
        get => RawDataSource.IsUniform;
        set => RawDataSource.IsUniform = value;
    }
    public FRawEnum ForceName
    {
        get => RawDataSource.ForceName;
        set => RawDataSource.ForceName = value;
    }
    public int NumberOfPoints { get; set; }
    public double FirstDerivativeOnLeftSegmentEnd { get; set; }
    public double FirstDerivativeOnRightSegmentEnd { get; set; }
    public ObservableCollection<RawDataItem>? ForceValues { get; set; }
    public ObservableCollection<SplineDataItem>? SplineValues { get; set; }
    public SeriesCollection ChartData { get; }
    
    public double? IntegralValue
    {
        get
        {
            if (SplineDataOutput == null)
            {
                return null;
            }
            else
            {
                return SplineDataOutput.IntegralValue;
            }
        }
        set { }
    }
    public string this[string columnName]
    {
        get
        {
            switch (columnName)
            {
                case nameof(NumberOfPoints):
                    if (NumberOfPoints <= 2)
                        return "Number of points must be more than two.";
                    break;
                case nameof(NumberOfInitialPoints):
                    if (NumberOfInitialPoints <= 2)
                        return "Number of initial points must be more than two.";
                    break;
                case nameof(SegmentEnds):
                    if (SegmentEnds == null)
                        return "Segments ends list must have exactly two elements.";
                    if (SegmentEnds[0] >= SegmentEnds[1])
                        return "Segments ends list must be ascending.";
                    break;
            }
            return null;
        }
    }
    public string Error => this[string.Empty];
    RawData RawDataSource { get; set; }
    SplineData? SplineDataOutput { get; set; }
    public MainViewModel(IUIServices uiServices)
    {
        this.uiServices = uiServices;
        double[] segmentEnds = new double[] { 1, 2 };
        RawDataSource = new RawData(segmentEnds, 0, true, new FRawEnum());
        ChartData = new SeriesCollection {};
    }
    public void ComputeRawData()
    {

        RawDataSource.ComputeRawData();
        if (RawDataSource.RawDataItems == null)
        {
            // MessageBox.Show("Error! Computation of a RawData failed.");
            throw new Exception("Raw Data field is null or field Values haven't been initialized correctly");
        }
        ForceValues = new ObservableCollection<RawDataItem>(RawDataSource.RawDataItems);
        ChartValues<ObservablePoint> scatterPlotValues = new ChartValues<ObservablePoint>();
        foreach (RawDataItem rawDataItem in RawDataSource.RawDataItems)
        {
            scatterPlotValues.Add(new ObservablePoint(rawDataItem.Coordinate, rawDataItem.Force));
        }

        var plot = new ScatterSeries { Title = "Raw Data", Values = scatterPlotValues }; 
        uiServices.PlotScatterSeries(plot);
    }
    public void Interpolate()
    {
        string? errorMessage = null;
        if (RawDataSource == null)
        {
            // MessageBox.Show("You should build a Raw Data object before interpolation!");
            throw new Exception("Raw Data object is null!");
        }
        SplineDataOutput = new SplineData(RawDataSource, FirstDerivativeOnLeftSegmentEnd, FirstDerivativeOnRightSegmentEnd, NumberOfPoints);
        try
        {
            SplineDataOutput.BuildSpline();
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }
        if (errorMessage != null || SplineDataOutput == null || SplineDataOutput.SplineDataItems == null)
        {
            // MessageBox.Show("Interpolation failed!\n" + errorMessage);
            throw new Exception("Either Spline Data or Spline Data Items is null");
        }
        SplineValues = new ObservableCollection<SplineDataItem>(SplineDataOutput.SplineDataItems);
        NotifyPropertyChanged("IntegralValue");
        ChartValues<ObservablePoint> linePlotValues = new ChartValues<ObservablePoint>();
        foreach (var splineDataItem in SplineDataOutput.SplineDataItems)
        {
            linePlotValues.Add(new ObservablePoint(splineDataItem.PointCoordinate, splineDataItem.SplineValue));
        }

        var line = new LineSeries { Title = "Interpolated Data", Values = linePlotValues };
        uiServices.PlotLineSeries(line);
    }
    public void Save(string filename)
    {
        if (RawDataSource == null)
        {
            throw new Exception("You should create a raw data object before saving");
        }
        else
        {
            // MessageBox.Show(filename);
            RawDataSource.Save(filename);
        }
    }
    public void Load(string filename)
    {
        RawDataSource = new RawData(filename);
        if (RawDataSource.RawDataItems == null)
        {
            throw new Exception("The loaded raw data has no Force Values array");
        }
        ForceValues = new ObservableCollection<RawDataItem>(RawDataSource.RawDataItems);
    }
    }