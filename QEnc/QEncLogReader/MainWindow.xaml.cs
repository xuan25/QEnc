using QEncInfo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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

namespace QEncLogReader
{
    //using QEnc;
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public EncLog LoadEncLog(string path)
        {
            Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            EncLog encLog = null;
            try
            {
                encLog = (EncLog)binaryFormatter.Deserialize(stream);
                encLog.CmdLog = DecodeBase64(encLog.CmdLog);
                encLog.FFLog = DecodeBase64(encLog.FFLog);
            }
            catch (Exception)
            {
                MessageBox.Show("The file is not Supported");
            }
            stream.Close();
            return encLog;
        }

        public void ParseEncLog(EncLog encLog)
        {
            VideoStateBox.Text = encLog.Note.Video.ToString();
            if(encLog.Param.VideoMode == EncParam.VideoModes.CRF)
                VideoBitrateBox.Text = encLog.Param.VideoCRF.ToString();
            else
                VideoBitrateBox.Text = encLog.Param.VideoBitrate.ToString();
            VideoModeBox.Text = encLog.Param.VideoMode.ToString();
            VideoPathBox.Text = encLog.Param.VideoPath;
            VideoParamBox.Text = encLog.Param.VideoParam;

            AudioStateBox.Text = encLog.Note.Audio.ToString();
            AudioBitrateBox.Text = encLog.Param.AudioBitrate.ToString();
            AudioPathBox.Text = encLog.Param.AudioPath;
            AudioParamBox.Text = encLog.Param.AudioParam;

            SubtitleStateBox.Text = encLog.Note.Subtitle.ToString();
            SubtitlePathBox.Text = encLog.Param.SubtitlePath;
            SubtitleParamBox.Text = "";

            CmdLogBox.Text = encLog.CmdLog;
            FFLogBox.Text = encLog.FFLog;
            TimeBox.Text = encLog.StartTime.ToString();
        }

        public string DecodeBase64(string code)
        {
            string decode = "";
            byte[] bytes = Convert.FromBase64String(code);
            try
            {
                decode = Encoding.Default.GetString(bytes);
            }
            catch
            {
                decode = code;
            }
            return decode;
        }

        private void Window_PreviewDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Move;
                e.Handled = true;
            }
            else
            {
                e.Effects = DragDropEffects.None;
                e.Handled = false;
            }
        }

        private void Window_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Move;
                e.Handled = true;
            }
            else
            {
                e.Effects = DragDropEffects.None;
                e.Handled = false;
            }
        }

        private void Window_PreviewDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop) == null)
                return;
            e.Handled = true;
            string filename = ((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            EncLog encLog = LoadEncLog(filename);
            if(encLog != null)
                ParseEncLog(encLog);
        }
    }
}
