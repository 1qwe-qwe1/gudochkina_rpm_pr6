using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;


namespace gudochkina_pr3.Pages
{
public partial class EmployeeEditPage
    {
        public class BoolToColorConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is bool isActive)
                {
                    return isActive ? Brushes.Green : Brushes.Red;
                }
                return Brushes.Gray;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    }
}
