using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GraphicalMirai
{
    public class MarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string param = ((string)(parameter ?? "0,0,0,0")).Replace(";", ",");
            if (value != null)
            {
                param = param.Replace("x", value.ToString());
            }
            return param;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// InnerMessageBox.xaml 的交互逻辑
    /// </summary>
    public partial class InnerMessageBox : UserControl
    {
        public InnerMessageBox()
        {
            InitializeComponent();
        }
    }
}
