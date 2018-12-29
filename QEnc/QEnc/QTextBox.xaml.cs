using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QEnc
{
    /// <summary>
    /// Interaction logic for QTextBox.xaml
    /// </summary>
    public partial class QTextBox : UserControl
    {
        public delegate void DelTextChanged(object sender, TextChangedEventArgs e);
        public event DelTextChanged TextChanged;

        public string Text
        {
            get
            {
                return MainTextBox.Text;
            }
            set
            {
                MainTextBox.Text = value;
            }
        }

        public static readonly DependencyProperty InnerBrushProperty = DependencyProperty.Register("InnerBrush", typeof(Brush), typeof(QTextBox), new FrameworkPropertyMetadata(Brushes.White));
        public Brush InnerBrush
        {
            get
            {
                return (Brush)GetValue(InnerBrushProperty);
            }
            set
            {
                SetValue(InnerBrushProperty, value);
            }
        }
        private void InnerBrushChanged(object sender, EventArgs e)
        {
            InnerDrawing.Brush = InnerBrush;
        }
        DependencyPropertyDescriptor InnerBrushPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(InnerBrushProperty, typeof(QTextBox));

        public static readonly DependencyProperty BorderColorProperty = DependencyProperty.Register("BorderColor", typeof(Color), typeof(QTextBox), new FrameworkPropertyMetadata((Color)ColorConverter.ConvertFromString("#FFABADB3")));
        public Color BorderColor
        {
            get
            {
                return (Color)GetValue(BorderColorProperty);
            }
            set
            {
                SetValue(BorderColorProperty, value);
            }
        }

        public static readonly DependencyProperty MouseOverColorProperty = DependencyProperty.Register("MouseOverColor", typeof(Color), typeof(QTextBox), new FrameworkPropertyMetadata((Color)ColorConverter.ConvertFromString("#990078D7")));
        public Color MouseOverColor
        {
            get
            {
                return (Color)GetValue(MouseOverColorProperty);
            }
            set
            {
                SetValue(MouseOverColorProperty, value);
            }
        }

        public static readonly DependencyProperty HighLightColorProperty = DependencyProperty.Register("HighLightColor", typeof(Color), typeof(QTextBox), new FrameworkPropertyMetadata((Color)ColorConverter.ConvertFromString("#FF0078D7")));
        public Color HighLightColor
        {
            get
            {
                return (Color)GetValue(HighLightColorProperty);
            }
            set
            {
                SetValue(HighLightColorProperty, value);
            }
        }

        public QTextBox()
        {
            InitializeComponent();
        }

        private void MainTextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            highlightBrush.Color = HighLightColor;
        }

        private void MainTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            highlightBrush.Color = (Color)ColorConverter.ConvertFromString("#00000000");
        }

        private void MainTextBox_MouseEnter(object sender, MouseEventArgs e)
        {
            borderBrush.Color = MouseOverColor;
        }

        private void MainTextBox_MouseLeave(object sender, MouseEventArgs e)
        {
            borderBrush.Color = BorderColor;
        }

        private void MainTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextChanged?.Invoke(sender, e);
        }
    }

    public class ToRect : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double w = (double)values[0];
            double h = (double)values[1];

            Rect rect = new Rect(0, 0, w, h);

            return rect;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class ToBorderedRect : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double borderThickness = 1;
            double w = (double)values[0] - 2 * borderThickness;
            double h = (double)values[1] - 2 * borderThickness;

            if (w > 2 && h > 2)
            {
                Rect rect = new Rect(borderThickness, borderThickness, w, h);
                return rect;
            }
            else
            {
                return null;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class ToRadius : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double var = (double)value;
            return var / 2;
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class ToBorderedRadius : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double var = (double)value;
            return var / 2;
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
