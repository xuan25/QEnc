using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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

namespace GZCompressor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop) == null)
                return;
            e.Handled = true;
            Array files = (Array)e.Data.GetData(DataFormats.FileDrop);
            foreach(var file in files)
            {
                string filename = file.ToString();
                FileStream input = new FileStream(filename, FileMode.Open);
                FileStream output = new FileStream(filename+".gz", FileMode.Create);
                GZipStream gZipStream = new GZipStream(output, CompressionMode.Compress);

                byte[] temp = new byte[input.Length];
                input.Read(temp, 0, (int)input.Length);
                gZipStream.Write(temp, 0, (int)input.Length);

                input.Close();
                gZipStream.Close();
                output.Close();
                
            }
            MessageBox.Show("压缩成功!");
        }
    }
}
