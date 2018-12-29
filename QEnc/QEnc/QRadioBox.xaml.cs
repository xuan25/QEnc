using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for QRadioBox.xaml
    /// </summary>
    public partial class QRadioBox : UserControl
    {
        public delegate void DelSelectionChanged(object sender, int index, string value);
        public event DelSelectionChanged SelectionChanged;
        public event DelSelectionChanged PreviewSelectionChanged;

        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register("Items", typeof(List<QRadioBoxItem>), typeof(QRadioBox), new FrameworkPropertyMetadata(new List<QRadioBoxItem>()));
        public List<QRadioBoxItem> Items
        {
            get
            {
                return (List<QRadioBoxItem>)GetValue(ItemsProperty);
            }
            set
            {
                SetValue(ItemsProperty, value);
            }
        }
        private void ItemsChanged(object sender, EventArgs e)
        {
            int count = 0;
            RadioBox.ColumnDefinitions.Clear();
            foreach (QRadioBoxItem i in Items)
            {
                RadioBox.ColumnDefinitions.Add(new ColumnDefinition());
                Grid.SetColumn(i, count);
                count++;
            }
            if (RadioBox.ActualWidth > 2 && RadioBox.ActualHeight > 2)
                SelectAnimation(SelectedIndex);
        }
        DependencyPropertyDescriptor ItemsPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(ItemsProperty, typeof(QRadioBox));

        public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register("SelectedIndex", typeof(int), typeof(QRadioBox), new FrameworkPropertyMetadata(0));
        public int SelectedIndex
        {
            get
            {
                return (int)GetValue(SelectedIndexProperty);
            }
            set
            {
                SetValue(SelectedIndexProperty, value);
            }
        }
        private void SelectedIndexChanged(object sender, EventArgs e)
        {
            if (RadioBox.ActualWidth > 2 && RadioBox.ActualHeight > 2)
                SelectAnimation(SelectedIndex);
        }
        DependencyPropertyDescriptor SelectedIndexPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(SelectedIndexProperty, typeof(QRadioBox));

        public static readonly DependencyProperty SelectedBrushProperty = DependencyProperty.Register("SelectedBrush", typeof(Brush), typeof(QRadioBox), new FrameworkPropertyMetadata(Brushes.DeepSkyBlue));
        public Brush SelectedBrush
        {
            get
            {
                return (Brush)GetValue(SelectedBrushProperty);
            }
            set
            {
                SetValue(SelectedBrushProperty, value);
            }
        }
        private void SelectedBrushChanged(object sender, EventArgs e)
        {
            SelectedDrawing.Brush = SelectedBrush;
        }
        DependencyPropertyDescriptor SelectedBrushPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(SelectedBrushProperty, typeof(QRadioBox));

        public static readonly DependencyProperty InnerBrushProperty = DependencyProperty.Register("InnerBrush", typeof(Brush), typeof(QRadioBox), new FrameworkPropertyMetadata(Brushes.DeepSkyBlue));
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
        DependencyPropertyDescriptor InnerBrushPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(InnerBrushProperty, typeof(QRadioBox));

        public QRadioBox()
        {
            InitializeComponent();
            Items = new List<QRadioBoxItem>();
            ItemsPropertyDescriptor.AddValueChanged(this, ItemsChanged);
            SelectedIndexPropertyDescriptor.AddValueChanged(this, SelectedIndexChanged);
            SelectedBrushPropertyDescriptor.AddValueChanged(this, SelectedBrushChanged);
            InnerBrushPropertyDescriptor.AddValueChanged(this, InnerBrushChanged);
        }

        double currentPosition = 1;
        Thread selectAnimationThread;
        private void SelectAnimation(int to)
        {
            double oldposition = currentPosition;
            double newposition = 1 + this.ActualWidth / Items.Count * to;

            if (selectAnimationThread != null)
            {
                selectAnimationThread.Abort();
            }
            selectAnimationThread = new Thread(delegate ()
            {
                double x = 0;
                while (x < Math.PI)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        x += 0.05;
                        double y = (Math.Cos(x) + 1) / 2;
                        currentPosition = oldposition + (1 - y) * (newposition - oldposition);
                        SelectedRect.Rect = new Rect(currentPosition, 1, (RadioBox.ActualWidth - 2) / Items.Count, RadioBox.ActualHeight - 2);
                    }));
                    Thread.Sleep(5);
                }
                Dispatcher.Invoke(new Action(() =>
                {
                    SelectionChanged?.Invoke(this, to, Items[to].Value);
                }));
            });
            selectAnimationThread.Start();
        }

        public void AddSelection(string value)
        {
            QRadioBoxItem item = new QRadioBoxItem();
            item.Value = value;
            item.Selected += RadioButtonItem_Selected;
            Items.Add(item);
            RadioBox.Children.Add(item);
            ItemsChanged(null, null);
        }

        private void RadioButtonItem_Selected(int index)
        {
            SelectedIndex = index;
            PreviewSelectionChanged?.Invoke(this, index, Items[index].Value);
        }

        private void RadioBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (RadioBox.ActualWidth > 2 && RadioBox.ActualHeight > 2)
                SelectAnimation(SelectedIndex);
        }
    }
}
