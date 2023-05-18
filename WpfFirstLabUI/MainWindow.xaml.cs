using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using ClassLibraryUI;
using ViewModelFirstLabUI;
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
    public class DoubleToStringConverter : IValueConverter
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
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
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
    
 
    public partial class MainWindow : Window, IUIServices
    {
        public MainViewModel ViewData { get; set; }
        public MainWindow()
        {
            ViewData = new MainViewModel(this);
            InitializeComponent();
            DataContext = ViewData;
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

        public void PlotScatterSeries(ScatterSeries series)
        {
            chart.Series.Add(series);
        }

        public void PlotLineSeries(LineSeries series)
        {
            chart.Series.Add(series);
        }
        private void executeFromControlsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewData.ComputeRawData();
                rawDataListBox.ItemsSource = ViewData.ForceValues;
                ViewData.Interpolate();
                splineDataListBox.ItemsSource = ViewData.SplineValues;
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
                    ViewData.Save(filename);
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
                    ViewData.Load(filename);
                    ViewData.NotifyPropertyChanged("ForceName");
                    ViewData.NotifyPropertyChanged("SegmentEnds");
                    ViewData.NotifyPropertyChanged("NumberOfInitialPoints");
                    ViewData.NotifyPropertyChanged("IsUniform");
                    rawDataListBox.ItemsSource = ViewData.ForceValues;

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Load failed because: {ex.Message}");
                }
                try
                {
                    ViewData.Interpolate();
                    splineDataListBox.ItemsSource = ViewData.SplineValues;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Interpolation failed because: {ex.Message}");
                }
            }

        }

        private void CanExecuteSaveCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ViewData.NumberOfInitialPoints <= 2)
            {
                e.CanExecute = false;
            }
            else if (ViewData.NumberOfPoints <= 2)
            {
                e.CanExecute = false;
            }
            else if (ViewData.SegmentEnds[0] > ViewData.SegmentEnds[1])
            {
                e.CanExecute = false;
            }
            else if (ViewData.ForceValues == null)
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
            if (ViewData == null || ViewData.NumberOfPoints <= 2)
            {
                e.CanExecute = false;
            }
            else 
            { 
                e.CanExecute = true; 
            }
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
