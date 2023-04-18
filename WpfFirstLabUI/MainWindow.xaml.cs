using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Threading.Tasks;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using ClassLibraryUI;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
namespace WpfFirstLabUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public class StringToDoubleArrayConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double[] doubleArray)
            {
                string stringValue = string.Join(";", doubleArray);

                return stringValue;
            }

            return null;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double[] result = new double[2];
            string[]? values;
            if (value != null)
            {
                string? stringValue = value.ToString();
                if (stringValue == "Segment ends")
                {
                    return result;
                }
                if (stringValue != null)
                {
                    values = stringValue.Split(';');
                }
                else
                {
                    throw new ArgumentNullException("String can't be parsed");
                }


                if (values.Length == 2)
                {
                    if (double.TryParse(values[0], out double num1) && double.TryParse(values[1], out double num2))
                    {
                        result[0] = num1;
                        result[1] = num2;
                    }
                }
            }

            return result;
        }

    }
    public class IsNonuniformToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = (bool)value;
            if (boolValue)
            {
                return "False";
            }
            else
            {
                return "True";
            }

        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = (bool)value;
            if (boolValue)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

    }
    public class IntegralValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return "Unknown";
            }
            else
            {
                return string.Format("{0:0.00}", (double)value);
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
    public class IsNumericConverter : IValueConverter 
    {
        public object Convert(object value, Type trargetType, object parameters, CultureInfo culture)
        {
            string? stringValue = value.ToString();
            if (stringValue == null || (int) value == 0)
            {
                return "";
            }
            else
            {
                return stringValue;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? stringValue = (string) value;
            if (stringValue == null || stringValue == "")
            {
                return 0;
            } else
            {
                return Int32.Parse(stringValue);
            }
        }
    }
    public class ValidationErrorsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ReadOnlyObservableCollection<ValidationError> errors && errors.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var error in errors)
                {
                    sb.AppendLine(error.ErrorContent.ToString());
                }
                return sb.ToString().TrimEnd(Environment.NewLine.ToCharArray());
            }
            else
            {
                return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class ViewData : INotifyPropertyChanged, IDataErrorInfo
    {
        public double[] SegmentEnds
        {
            get { return RawDataSource.SegmentEnds; }
            set { RawDataSource.SegmentEnds = value; }
        }
        public int NumberOfInitialPoints
        {
            get { return RawDataSource.NumberOfPoints; }
            set { RawDataSource.NumberOfPoints = value; }
        }
        public bool IsGridUniform
        {
            get { return RawDataSource.IsUniform; }
            set { RawDataSource.IsUniform = value; }
        }
        public FRawEnum ForceName
        {
            get { return RawDataSource.ForceName; }
            set { RawDataSource.ForceName = value; }
        }
        public int NumberOfPoints { get; set; }
        public double FirstDerivativeOnLeftSegmentEnd { get; set; }
        public double FirstDerivativeOnRightSegmentEnd { get; set; }
        public ObservableCollection<RawDataItem>? ForceValues { get; set; }
        public ObservableCollection<SplineDataItem>? SplineValues { get; set; }
        public SeriesCollection ChartData { get; set; }
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
        public event PropertyChangedEventHandler? PropertyChanged;
        public void NotifyPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
        public ViewData()
        {
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
            ChartData.Add(new ScatterSeries{ Title = "Raw Data", Values = scatterPlotValues});
        }
        public void Interpolate()
        {
            string? errorMessage = null;
            if (RawDataSource == null)
            {
                MessageBox.Show("You should build a Raw Data object before interpolation!");
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
                MessageBox.Show("Interpolation failed!\n" + errorMessage);
                throw new Exception("Either Spline Data or Spline Data Items is null");
            }
            SplineValues = new ObservableCollection<SplineDataItem>(SplineDataOutput.SplineDataItems);
            NotifyPropertyChanged("IntegralValue");
            ChartValues<ObservablePoint> linePlotValues = new ChartValues<ObservablePoint>();
            foreach (var splineDataItem in SplineDataOutput.SplineDataItems)
            {
                linePlotValues.Add(new ObservablePoint(splineDataItem.PointCoordinate, splineDataItem.SplineValue));
            }
            ChartData.Add(new LineSeries { Title = "Interpolated Data", Values = linePlotValues });
        }
        public void Save(string filename)
        {
            if (RawDataSource == null)
            {
                throw new Exception("You should create a raw data object before saving");
            }
            else
            {
                MessageBox.Show(filename);
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
    public partial class MainWindow : Window
    {
        public ViewData viewData { get; set; } = new ViewData();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = viewData;
            forceInput.ItemsSource = Enum.GetValues(typeof(FRawEnum));
            Func<double, string> formatFunc = (x) => string.Format("{0:0.00}", x);
            chart.AxisY.Add(new Axis
            {
                Title = "Force",
                LabelFormatter = formatFunc,
            });
            chart.AxisX.Add(new Axis
            {
                Title = "Coordinate",
                LabelFormatter = formatFunc,
            });
        }

        private void executeFromControlsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                viewData.ComputeRawData();
                // rawDataListBox.ItemsSource = viewData.ForceValues;
                viewData.Interpolate();
                // splineDataListBox.ItemsSource = viewData.SplineValues;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new();
            saveFileDialog.FileName = "RawData"; // Default file name
            saveFileDialog.DefaultExt = ".json"; // Default file extension
            bool? result = saveFileDialog.ShowDialog();
            if (result == true)
            {
                string filename = saveFileDialog.FileName;
                try
                {
                    viewData.Save(filename);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Save failed because: {ex.Message}");
                }
            }

        }

        private void executeFromFileButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new();
            openFileDialog.FileName = "RawData"; // Default file name
            openFileDialog.DefaultExt = ".json"; // Default file extension
            bool? result = openFileDialog.ShowDialog();
            if (result == true)
            {
                string filename = openFileDialog.FileName;
                try
                {
                    viewData.Load(filename);
                    viewData.NotifyPropertyChanged("ForceName");
                    viewData.NotifyPropertyChanged("SegmentEnds");
                    viewData.NotifyPropertyChanged("NumberOfInitialPoints");
                    viewData.NotifyPropertyChanged("IsUniform");
                    // rawDataListBox.ItemsSource = viewData.ForceValues;

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Load failed because: {ex.Message}");
                }
                try
                {
                    viewData.Interpolate();
                    // splineDataListBox.ItemsSource = viewData.SplineValues;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Interpolation failed because: {ex.Message}");
                }
            }

        }

        private void CanExecuteSaveCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            if (viewData.NumberOfInitialPoints <= 2)
            {
                e.CanExecute = false;
            }
            else if (viewData.NumberOfPoints <= 2)
            {
                e.CanExecute = false;
            }
            else if (viewData.SegmentEnds[0] > viewData.SegmentEnds[1])
            {
                e.CanExecute = false;
            }
            else if (viewData.ForceValues == null)
            {
                e.CanExecute = false;
            }
            else
            {
                e.CanExecute = true;
            }

        }

        private void CanExecuteFromData(object sender, CanExecuteRoutedEventArgs e)
        {

            foreach (FrameworkElement child in inputGrid.Children)
            {
                if (Validation.GetHasError(child))
                {
                    e.CanExecute = false;
                    return;
                }
            }
            e.CanExecute = true;
        }

        private void CanExecuteFromFile(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
    }

    public static class CustomCommands
    {
        public static readonly RoutedUICommand ExecuteFromData = new RoutedUICommand
            (
                "ExecuteFromData",
                "ExecuteFromData",
                typeof(CustomCommands)
            );
        public static readonly RoutedUICommand ExecuteFromFile = new RoutedUICommand
            (
                "ExecuteFormFile",
                "ExecuteFormFile",
                typeof(CustomCommands)
            );
    }
}
