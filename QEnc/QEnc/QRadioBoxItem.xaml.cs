using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for QRadioBoxItem.xaml
    /// </summary>
    public partial class QRadioBoxItem : UserControl
    {
        public delegate void DelSelected(int index);
        public event DelSelected Selected;

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(string), typeof(QRadioBoxItem), new FrameworkPropertyMetadata("Radio Item"));
        public string Value
        {
            get
            {
                return (string)GetValue(ValueProperty);
            }
            set
            {
                SetValue(ValueProperty, value);
            }
        }
        private void ValueChanged(object sender, EventArgs e)
        {
            TextArea.Text = Value;
        }
        DependencyPropertyDescriptor ValuePropertyDescriptor = DependencyPropertyDescriptor.FromProperty(ValueProperty, typeof(QRadioBoxItem));

        public QRadioBoxItem()
        {
            InitializeComponent();
            ValuePropertyDescriptor.AddValueChanged(this, ValueChanged);
        }

        private void RadioBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Selected?.Invoke(Grid.GetColumn(this));
        }

        private void RadioBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                Selected?.Invoke(Grid.GetColumn(this));
        }
    }
}
