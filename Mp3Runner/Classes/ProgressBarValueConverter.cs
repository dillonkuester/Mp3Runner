
using System.Globalization;
using System.Windows.Data;


namespace Mp3Runner
{
    public class ProgressBarValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double progressValue)
            {
                return progressValue * 2; 
            }
            return 0; 
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
