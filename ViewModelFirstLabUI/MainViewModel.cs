using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using ClassLibraryUI;

namespace ViewModelFirstLabUI;

public interface IUIServices
{
    void ReportError(string message);
    void Plot(double[] coordinate, double[] forceValues, string plotType);

    string? ChooseFileToOpen();
    string? ChooseFileToSave();
}


public class MainViewModel : ViewModelBase, IDataErrorInfo
{
    private readonly IUIServices uiServices;
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
    public ObservableCollection<RawDataItem>? ForceValues 
    {
        get => RawDataSource != null && RawDataSource.RawDataItems != null?
            new ObservableCollection<RawDataItem>(RawDataSource.RawDataItems) : null;
    }
    public ObservableCollection<SplineDataItem>? SplineValues 
    { 
        get => SplineDataOutput != null && SplineDataOutput.SplineDataItems != null?
            new ObservableCollection<SplineDataItem>(SplineDataOutput.SplineDataItems) : null;
    }
    public Array ForceTypes { get; set; }
    public double? IntegralValue
    {
        get => SplineDataOutput?.IntegralValue;
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
                    if (SegmentEnds.Length != 2)
                        return "Segments ends list must have exactly two elements.";
                    if (SegmentEnds[0] >= SegmentEnds[1])
                        return "Segments ends list must be ascending.";
                    break;
            }
            return "";
        }
    }
    private void ExecuteFromData(object sender)
    {
        try
        {
            RawDataSource.ComputeRawData();
            NotifyPropertyChanged(nameof(ForceValues));
            if (RawDataSource.Points == null || RawDataSource.ForceValues == null)
            {
                throw new Exception("Raw Data field is null or field Values haven't been initialized correctly");
            }
            uiServices.Plot(RawDataSource.Points, RawDataSource.ForceValues, "raw");
            SplineDataOutput = new SplineData(RawDataSource, FirstDerivativeOnLeftSegmentEnd, FirstDerivativeOnRightSegmentEnd, NumberOfPoints);
            SplineDataOutput.BuildSpline();
            NotifyPropertyChanged(nameof(SplineValues));
            if (SplineDataOutput.SplineDataItems == null)
            {
                throw new Exception("SplineDataItems property is null");
            }
            NotifyPropertyChanged("IntegralValue");
            uiServices.Plot(SplineDataOutput.SplineDataItems.Select(point => point.PointCoordinate).ToArray(),
                            SplineDataOutput.SplineDataItems.Select(point => point.SplineValue).ToArray(),
                            "spline");

            
        }
        catch (Exception ex)
        {
            uiServices.ReportError(ex.Message);
        }
    }
    private bool CanExecuteFromData(object sender)
    {
        return string.IsNullOrEmpty(this[nameof(SegmentEnds)]) 
            && string.IsNullOrEmpty(this[nameof(NumberOfInitialPoints)]) 
            && string.IsNullOrEmpty(this[nameof(NumberOfPoints)]);
    }
    private void ExecuteFromFile(object sender)
    {
        string? filename = uiServices.ChooseFileToOpen();
        if (filename != null)
        {
            try
            {
                Load(filename);
                NotifyPropertyChanged("ForceName");
                NotifyPropertyChanged("SegmentEnds");
                NotifyPropertyChanged("NumberOfInitialPoints");
                NotifyPropertyChanged("IsUniform");
                NotifyPropertyChanged("ForceValues");

            }
            catch (Exception ex)
            {
                uiServices.ReportError($"Load failed because: {ex.Message}");
            }
            try
            {
                if (RawDataSource == null)
                {
                    throw new Exception("Raw Data object is null!");
                }
                SplineDataOutput = new SplineData(RawDataSource, FirstDerivativeOnLeftSegmentEnd, FirstDerivativeOnRightSegmentEnd, NumberOfPoints);
                SplineDataOutput.BuildSpline();
                NotifyPropertyChanged("SplineValues");
                if (SplineDataOutput.SplineDataItems == null)
                {
                    throw new Exception("SplineDataItems property is null");
                }
                NotifyPropertyChanged("IntegralValue");
                uiServices.Plot(SplineDataOutput.SplineDataItems.Select(point => point.PointCoordinate).ToArray(),
                                SplineDataOutput.SplineDataItems.Select(point => point.SplineValue).ToArray(),
                                "spline");
            }
            catch (Exception ex)
            {
                uiServices.ReportError($"Interpolation failed because: {ex.Message}");
            }
        }


    }
    private bool CanExecuteFromFile(object sender)
    {
        return string.IsNullOrEmpty(this[nameof(NumberOfPoints)]);
    }
    private void SaveToFile(object sender)
    {
        string? filename = uiServices.ChooseFileToSave();
        if (filename != null)
        {
            try
            {
                Save(filename);
            }
            catch (Exception ex)
            {
                uiServices.ReportError($"Save failed because: {ex.Message}");
            }
        }

    }
    private bool CanSaveToFile(object sender)
    {
        return string.IsNullOrEmpty(this[nameof(SegmentEnds)])
            && string.IsNullOrEmpty(this[nameof(NumberOfInitialPoints)])
            && string.IsNullOrEmpty(this[nameof(NumberOfPoints)]) 
            && ForceValues != null;
    }
    public string Error => this[string.Empty];
    private RawData RawDataSource { get; set; }
    private SplineData? SplineDataOutput { get; set; }
    public ICommand ExecuteFromDataCommand { get; private set; }
    public ICommand ExecuteFromFileCommand { get; private set; }
    public ICommand SaveCommand { get; private set; }
    public MainViewModel(IUIServices uiServices)
    {
        this.uiServices = uiServices;
        ForceTypes = Enum.GetValues(typeof(FRawEnum));
        var segmentEnds = new double[] { 1, 2 };
        RawDataSource = new RawData(segmentEnds, 0, true, new FRawEnum());
        ExecuteFromDataCommand = new RelayCommand(ExecuteFromData, CanExecuteFromData);
        ExecuteFromFileCommand = new RelayCommand(ExecuteFromFile, CanExecuteFromFile);
        SaveCommand = new RelayCommand(SaveToFile, CanSaveToFile);

    }
    public void Save(string filename)
    {
        if (RawDataSource == null)
        {
            throw new Exception("You should create a raw data object before saving");
        }
        RawDataSource.Save(filename);
    }
    public void Load(string filename)
    {
        RawDataSource = new RawData(filename);
        if (RawDataSource.RawDataItems == null)
        {
            throw new Exception("The loaded raw data has no Force Values array");
        }
    }
 }