using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Interaction logic for QProgressBar.xaml
    /// </summary>
    public partial class QProgressBar : UserControl
    {
        public delegate void DelReseted();
        public event DelReseted Reseted;
        public delegate void DelProgressChanged(double value);
        public event DelProgressChanged ProgressChanged;

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(QProgressBar), new FrameworkPropertyMetadata(0.0));
        public double Value
        {
            get
            {
                return (double)GetValue(ValueProperty);
            }
            set
            {
                SetValue(ValueProperty, value);
            }
        }
        private void ValueChanged(object sender, EventArgs e)
        {
            if(ActualWidth != 0 && ActualHeight != 0)
                ShowProgress(Value);
            if (Value <= 25 || Value >= 75)
                OpacityBrush.AlignmentY = AlignmentY.Bottom;
            else
                OpacityBrush.AlignmentY = AlignmentY.Top;
            if (Value >= 0 && Value <= 50)
                OpacityBrush.AlignmentX = AlignmentX.Left;
            else
                OpacityBrush.AlignmentX = AlignmentX.Right;
            if (Value > 75)
            {
                HeaderOrigin.Visibility = Visibility.Hidden;
                PathR.Visibility = Visibility.Hidden;
            }
            else
            {
                HeaderOrigin.Visibility = Visibility.Visible;
                PathR.Visibility = Visibility.Visible;
            }
                
        }
        DependencyPropertyDescriptor ValuePropertyDescriptor = DependencyPropertyDescriptor.FromProperty(ValueProperty, typeof(QProgressBar));


        public static readonly DependencyProperty TrackBrushProperty = DependencyProperty.Register("TrackBrush", typeof(Brush), typeof(QProgressBar), new FrameworkPropertyMetadata(Brushes.Transparent));
        public Brush TrackBrush
        {
            get
            {
                return (Brush)GetValue(TrackBrushProperty);
            }
            set
            {
                SetValue(TrackBrushProperty, value);
            }
        }
        private void TrackBrushChanged(object sender, EventArgs e)
        {
            Track.Fill = TrackBrush;
        }
        DependencyPropertyDescriptor TrackBrushPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(TrackBrushProperty, typeof(QProgressBar));

        public static readonly DependencyProperty OutterBrushProperty = DependencyProperty.Register("OutterBrush", typeof(Brush), typeof(QProgressBar), new FrameworkPropertyMetadata(Brushes.DeepSkyBlue));
        public Brush OutterBrush
        {
            get
            {
                return (Brush)GetValue(OutterBrushProperty);
            }
            set
            {
                SetValue(OutterBrushProperty, value);
            }
        }
        private void OutterBrushChanged(object sender, EventArgs e)
        {
            Header.Fill = OutterBrush;
            PathR.Fill = OutterBrush;
            PathL.Fill = OutterBrush;
        }
        DependencyPropertyDescriptor OutterBrushPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(OutterBrushProperty, typeof(QProgressBar));

        public static readonly DependencyProperty BarOpacityProperty = DependencyProperty.Register("BarOpacity", typeof(double), typeof(QProgressBar), new FrameworkPropertyMetadata(1.0));
        public double BarOpacity
        {
            get
            {
                return (double)GetValue(BarOpacityProperty);
            }
            set
            {
                SetValue(BarOpacityProperty, value);
            }
        }
        private void BarOpacityChanged(object sender, EventArgs e)
        {
            Bar.Opacity = BarOpacity;
        }
        DependencyPropertyDescriptor BarOpacityPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(BarOpacityProperty, typeof(QProgressBar));

        public static readonly DependencyProperty InnerRadiusProperty = DependencyProperty.Register("InnerRadius", typeof(double), typeof(QProgressBar), new FrameworkPropertyMetadata(0.0));
        public double InnerRadius
        {
            get
            {
                return (double)GetValue(InnerRadiusProperty);
            }
            set
            {
                SetValue(InnerRadiusProperty, value);
            }
        }
        private void InnerRadiusChanged(object sender, EventArgs e)
        {
            MaskInner.RadiusX = InnerRadius;
            MaskInner.RadiusY = InnerRadius;
        }
        DependencyPropertyDescriptor InnerRadiusPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(InnerRadiusProperty, typeof(QProgressBar));

        public QProgressBar()
        {
            InitializeComponent();

            ValuePropertyDescriptor.AddValueChanged(this, ValueChanged);
            TrackBrushPropertyDescriptor.AddValueChanged(this, TrackBrushChanged);
            OutterBrushPropertyDescriptor.AddValueChanged(this, OutterBrushChanged);
            BarOpacityPropertyDescriptor.AddValueChanged(this, BarOpacityChanged);
            InnerRadiusPropertyDescriptor.AddValueChanged(this, InnerRadiusChanged);
        }

        private Thread barAnimationThread, opacityAnimationThread;

        private void ShowProgress()
        {
            if (opacityAnimationThread != null)
                opacityAnimationThread.Abort();

            opacityAnimationThread = new Thread(delegate ()
            {
                double i = 0;
                Dispatcher.Invoke(new Action(() =>
                {
                    i = BarOpacity;
                }));
                while (i < 1)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        BarOpacity = i;
                    }));
                    i = i + 0.1;
                    Thread.Sleep(20);
                }
                Dispatcher.Invoke(new Action(() =>
                {
                    BarOpacity = 1;
                }));
            });
            opacityAnimationThread.Start();
        }

        private void ResetProgress()
        {
            if (opacityAnimationThread != null)
                opacityAnimationThread.Abort();

            opacityAnimationThread = new Thread(delegate ()
            {
                double i = 1;
                Dispatcher.Invoke(new Action(() =>
                {
                    i = BarOpacity;
                }));
                while (i > 0)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        BarOpacity = i;
                    }));
                    i = i - 0.1;
                    Thread.Sleep(20);
                }
                Dispatcher.Invoke(new Action(() =>
                {
                    BarOpacity = 0;
                }));
                Dispatcher.Invoke(new Action(() =>
                {
                    Value = 0;
                }));
                Reseted?.Invoke();
            });
            opacityAnimationThread.Start();
        }

        public void UpdateProgress(double newValue)
        {
            double oldValue = Value;

            double opacity = 0;
            Dispatcher.Invoke(new Action(() =>
            {
                opacity = BarOpacity;
            }));

            if (newValue == -1 && opacity > 0)
            {
                new Thread(delegate ()
                {
                    while (true)
                    {
                        if (barAnimationThread.ThreadState == ThreadState.Stopped)
                            break;
                        Thread.Sleep(20);
                    }
                    ResetProgress();
                }).Start();
            }
            else if(newValue == 100)
            {
                ChangeValueANM(oldValue, newValue, true);
                if (opacity < 1)
                {
                    ShowProgress();
                }
            }
            else
            {
                ChangeValueANM(oldValue, newValue, false);
                if (opacity < 1)
                {
                    ShowProgress();
                }
            }

        }

        private void ChangeValueANM(double from, double to, bool force)
        {
            if (!force && barAnimationThread != null && barAnimationThread.ThreadState != ThreadState.Stopped)
                return;
            else if (force && barAnimationThread != null)
                barAnimationThread.Abort();
            

            barAnimationThread = new Thread(delegate ()
            {
                double x = 0;
                while (x < Math.PI)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        x += 0.015;
                        double y = (Math.Cos(x) + 1) / 2;
                        Value = from + (1 - y) * (to - from);
                        ProgressChanged?.Invoke(Value);
                    }));
                    Thread.Sleep(5);
                }
                Dispatcher.Invoke(new Action(() =>
                {
                    Value = to;
                    ProgressChanged?.Invoke(Value);
                }));
            });
            barAnimationThread.Start();
        }

        private void ShowProgress(double per)
        {
            double radius, radiusHeader;
            if (this.ActualWidth < this.ActualHeight)
            {
                radius = this.ActualWidth / 2;
            }
            else
            {
                radius = this.ActualHeight / 2;
            }
            radiusHeader = (radius - MaskInner.RadiusX) / 2;

            //定义变量
            Point endPoint = new Point();
            double x, y, angle;
            if (per > 100)
            {
                per = 100;
            }
            if (per < 0)
            {
                per = 0;
            }
            //换算角度
            angle = (per / 100) * (2 * Math.PI);
            //换算节点坐标
            x = Math.Cos(angle - Math.PI / 2);
            y = Math.Sin(angle - Math.PI / 2);
            //判断角度范围
            if (angle <= Math.PI)
            {
                //半圆1
                endPoint.X = x * radius + radius;
                endPoint.Y = y * radius + radius;
                EndR.Point = endPoint;
                //半圆2
                endPoint.X = radius;
                endPoint.Y = 2 * radius;
                EndL.Point = endPoint;
            }
            else
            {
                //半圆1
                endPoint.X = radius;
                endPoint.Y = 2 * radius;
                EndR.Point = endPoint;  //颜色图层
                //半圆2
                endPoint.X = x * radius + radius;
                endPoint.Y = y * radius + radius;
                EndL.Point = endPoint;  //颜色图层
            }

            double xHeader = x * (radius - radiusHeader) + radius - radiusHeader;
            double yHeader = y * (radius - radiusHeader) + radius - radiusHeader;
            Header.Margin = new Thickness(xHeader, yHeader, 0, 0);
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ShowProgress(Value);
        }
    }

    public class BackgroundDiameter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if ((double)values[0] < (double)values[1])
            {
                return (double)values[0];
            }
            else
            {
                return (double)values[1];
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class CenterPoint : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Point P = new Point();

            if ((double)values[0] < (double)values[1])
            {
                P.X = (double)values[0] / 2;
                P.Y = (double)values[0] / 2;
            }
            else
            {
                P.X = (double)values[1] / 2;
                P.Y = (double)values[1] / 2;
            }

            return P;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class StartPointR : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Point PCenter = (Point)value;

            Point P = new Point();
            P.X = PCenter.X;
            P.Y = 0;
            return P;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class StartPointL : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Point PCenter = (Point)value;

            Point P = new Point();
            P.X = PCenter.X;
            P.Y = PCenter.Y * 2;
            return P;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class EndPointR : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Point PCenter = (Point)value;

            Point P = new Point();
            P.X = PCenter.X;
            P.Y = PCenter.Y * 2;
            return P;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class EndPointL : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Point PCenter = (Point)value;

            Point P = new Point();
            P.X = PCenter.X;
            P.Y = 0;
            return P;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class EndSizeR : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Point PCenter = (Point)value;

            Size S = new Size();
            S.Width = PCenter.X;
            S.Height = PCenter.X;
            return S;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class EndSizeL : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Point PCenter = (Point)value;

            Size S = new Size();
            S.Width = PCenter.X;
            S.Height = PCenter.X;
            return S;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class HeaderDiameter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double outerRadius, innerRadius;

            if ((double)values[0] < (double)values[1])
            {
                outerRadius = (double)values[0] / 2;
            }
            else
            {
                outerRadius = (double)values[1] / 2;
            }

            innerRadius = (double)values[2];
            double diameter = outerRadius - innerRadius;
            if (diameter > 0)
            {
                return diameter;
            }
            return 0.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class HeaderMargin : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Thickness T = new Thickness();

            double outerRadius, innerRadius;

            if ((double)values[0] < (double)values[1])
            {
                outerRadius = (double)values[0] / 2;
            }
            else
            {
                outerRadius = (double)values[1] / 2;
            }

            innerRadius = (double)values[2];

            T.Left = outerRadius - (outerRadius - innerRadius) / 2;

            return T;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class MaskMargin : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Thickness T = new Thickness();

            double outerRadius, innerRadius;

            if ((double)values[0] < (double)values[1])
            {
                outerRadius = (double)values[0] / 2;
            }
            else
            {
                outerRadius = (double)values[1] / 2;
            }

            innerRadius = (double)values[2] / 2;

            T.Top = outerRadius - innerRadius;
            T.Left = outerRadius - innerRadius;

            return T;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class MaskInner : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double innerRadius;
            innerRadius = (double)values[2] / 2;
            return innerRadius;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class MaskOutter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double outerRadius;
            if ((double)values[0] < (double)values[1])
            {
                outerRadius = (double)values[0] / 2;
            }
            else
            {
                outerRadius = (double)values[1] / 2;
            }
            return outerRadius;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class Smaller : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double r;
            if ((double)values[0] < (double)values[1])
            {
                r = (double)values[0];
            }
            else
            {
                r = (double)values[1];
            }
            return r;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
