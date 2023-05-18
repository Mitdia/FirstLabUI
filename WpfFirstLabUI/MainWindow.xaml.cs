using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel(this);
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

        public string? ChooseFileToOpen()
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new();
            openFileDialog.FileName = "RawData"; // Default file name
            openFileDialog.DefaultExt = ".json"; // Default file extension
            bool? result = openFileDialog.ShowDialog();
            if (result == true)
            {
                return openFileDialog.FileName;
            }
            return null;
        }
        public string? ChooseFileToSave()
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new();
            saveFileDialog.FileName = "RawData"; // Default file name
            saveFileDialog.DefaultExt = ".json"; // Default file extension
            bool? result = saveFileDialog.ShowDialog();
            if (result == true)
            {
                return saveFileDialog.FileName;
            }
            return null;
        }
    }
}
