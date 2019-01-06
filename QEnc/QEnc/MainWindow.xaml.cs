using QEncInfo;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QEnc
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //API
        [DllImport("kernel32.dll")]
        static extern uint SetThreadExecutionState(uint esFlags);
        const uint ES_SYSTEM_REQUIRED = 0x00000001;
        const uint ES_DISPLAY_REQUIRED = 0x00000002;
        const uint ES_CONTINUOUS = 0x80000000;

        [DllImport("USER32.DLL")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("USER32.DLL")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        public static int GWL_STYLE = -16;
        public static int WS_CHILD = 0x40000000; //child window
        public static int WS_BORDER = 0x00800000; //window with border
        public static int WS_DLGFRAME = 0x00400000; //window with double border but no title
        public static int WS_CAPTION = WS_BORDER | WS_DLGFRAME; //window with a title bar

        public MainWindow()
        {
            InitializeComponent();

            this.Opacity = 0;

            Binding minWidthBinding = new Binding();
            minWidthBinding.ElementName = "QEnc";
            minWidthBinding.Path = new PropertyPath("ActualHeight");
            minWidthBinding.Converter = new MinWidthConverter();
            this.SetBinding(Window.MinWidthProperty, minWidthBinding);

            VideoModeBox.AddSelection(Application.Current.FindResource("VCRF").ToString());
            VideoModeBox.AddSelection(Application.Current.FindResource("B1Pass").ToString());
            VideoModeBox.AddSelection(Application.Current.FindResource("B2Pass").ToString());
            VideoModeBox.AddSelection(Application.Current.FindResource("B3Pass").ToString());
            VideoModeBox.AddSelection(Application.Current.FindResource("Auto").ToString());


            StartBox.AddSelection(Application.Current.FindResource("Config").ToString());
            StartBox.AddSelection(Application.Current.FindResource("Start").ToString());

            ProcessModeBox.AddSelection(Application.Current.FindResource("Single").ToString());
            ProcessModeBox.AddSelection(Application.Current.FindResource("Multi").ToString());

            JoinQueueBox.AddSelection(Application.Current.FindResource("Not_Queue").ToString());
            JoinQueueBox.AddSelection(Application.Current.FindResource("Queue").ToString());

            QueueList.Items.Add(new ListBoxItem() { Content = Application.Current.FindResource("New_Queue_Item").ToString() });
            QueueList.SelectedIndex = 0;

            QueueBoxLeft.Width = new GridLength(0, GridUnitType.Star);
            QueueBoxRight.Width = new GridLength(1, GridUnitType.Star);

            VideoBitrateBox.Text = "10000";
            VideoCRFBox.Text = "23.0";
            AudioBitrateBox.Text = "192";

            encParam = new EncParam();
            LoadConfig();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            IntPtr windowHandle = new WindowInteropHelper(this).Handle;
            int style = GetWindowLong(windowHandle, GWL_STYLE);
            SetWindowLong(windowHandle, GWL_STYLE, (style | WS_CAPTION));

            ProgressBar.BarOpacity = 0;
            ProgressBar.Value = 0;
            ProgressBar.UpdateProgress(100);

            ((Storyboard)Resources["ShowWindow"]).Completed += delegate
            {
                 Dispatcher.Invoke(new Action(() =>
                 {
                     ((Storyboard)Resources["ShowProgressBox"]).Begin();
                 }));
            };
            ((Storyboard)Resources["ShowWindow"]).Begin();
            
        }

        // About Header control

        private enum TitleFlag
        {
            DragMove = 0,
            Minimize = 1,
            Close = 2
        }
        private TitleFlag titleflag;

        private void Header_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (CloseBtn.IsMouseOver == false && MinimizeBtn.IsMouseOver == false)
            {
                this.DragMove();
                titleflag = TitleFlag.DragMove;
            }
            else if (MinimizeBtn.IsMouseOver == true)
            {
                titleflag = TitleFlag.Minimize;
            }
            else if (CloseBtn.IsMouseOver == true)
            {
                titleflag = TitleFlag.Close;
            }
        }

        private void Header_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (MinimizeBtn.IsMouseOver == true && titleflag == TitleFlag.Minimize)
            {
                this.WindowState = WindowState.Minimized;
            }
            else if (CloseBtn.IsMouseOver == true && titleflag == TitleFlag.Close)
            {
                this.Close();
            }
        }

        // About input checking

        private void NumberBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                Match match = Regex.Match(text, "[0-9]+\\.?[0-9]*");
                if (match.Success)
                {
                    ((QTextBox)sender).Text = match.Value;
                }
                e.CancelCommand();
            }
        }

        private void NumberBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
                e.Handled = true;
        }

        private void NumberBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!(isNumberic(e.Text) || (e.Text == "." && !((QTextBox)sender).Text.Contains("."))))
                e.Handled = true;
        }

        public bool isNumberic(string _string)
        {
            if (string.IsNullOrEmpty(_string))
                return false;
            foreach (char c in _string)
            {
                if (!char.IsDigit(c))
                    return false;
            }
            return true;
        }

        // About video mode

        private void VideoModeBox_SelectionChanged(object sender, int index, string value)
        {
            if(index == 0 || index == 4)
                VideoCRFBox.IsEnabled = true;
            else
                VideoCRFBox.IsEnabled = false;
            if(index != 0)
                VideoBitrateBox.IsEnabled = true;
            else
                VideoBitrateBox.IsEnabled = false;
        }

        // About file import and checking

        private void Grid_PreviewDragEnter(object sender, DragEventArgs e)
        {
            Grid grid = (Grid)sender;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Move;
                ((Storyboard)Resources[grid.Name + "DragEnter"]).Begin();
                e.Handled = true;
            }
            else
            {
                e.Effects = DragDropEffects.None;
                e.Handled = false;
            }
        }

        private void Grid_PreviewDragOver(object sender, DragEventArgs e)
        {
            Grid grid = (Grid)sender;
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

        private void Grid_PreviewDragLeave(object sender, DragEventArgs e)
        {
            Grid grid = (Grid)sender;
            ((Storyboard)Resources[ grid.Name + "DragLeave"]).Begin();
        }

        private void ProcessGrid_PreviewDrop(object sender, DragEventArgs e)
        {
            Grid_PreviewDragLeave(sender, e);
            if (e.Data.GetData(DataFormats.FileDrop) == null)
                return;
            e.Handled = true;
            string filename = ((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();

            SetVideo(filename);
            SetAudio(filename);
            SetSubtitle(filename);
        }

        Thread videoCheckThread;
        private void VideoGrid_PreviewDrop(object sender, DragEventArgs e)
        {
            Grid_PreviewDragLeave(sender, e);
            if (e.Data.GetData(DataFormats.FileDrop) == null)
                return;
            e.Handled = true;
            string filename = ((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            SetVideo(filename);
        }

        private void SetVideo(string path)
        {
            encParam.SetVideoPath(path);
            VideoNote.State = StateNote.States.Processing;
            if (videoCheckThread != null)
                videoCheckThread.Abort();
            videoCheckThread = new Thread(delegate ()
            {
                if (Enc.IsVideoAvailable(path))
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        VideoNote.State = StateNote.States.Loaded;
                    }));
                }
                else
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        VideoNote.State = StateNote.States.Unavailable;
                    }));
                }
            });
            videoCheckThread.Start();
        }

        Thread audioCheckThread;
        private void AudioGrid_PreviewDrop(object sender, DragEventArgs e)
        {
            Grid_PreviewDragLeave(sender, e);
            if (e.Data.GetData(DataFormats.FileDrop) == null)
                return;
            e.Handled = true;
            string filename = ((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            SetAudio(filename);
        }

        private void SetAudio(string path)
        {
            encParam.SetAudioPath(path);
            AudioNote.State = StateNote.States.Processing;
            if (audioCheckThread != null)
                audioCheckThread.Abort();
            audioCheckThread = new Thread(delegate ()
            {
                if (Enc.IsAudioAvailable(path))
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        AudioNote.State = StateNote.States.Loaded;
                    }));
                }
                else
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        AudioNote.State = StateNote.States.Unavailable;
                    }));
                }
            });
            audioCheckThread.Start();
        }

        Thread subtitleCheckThread;
        private void SubtitleGrid_PreviewDrop(object sender, DragEventArgs e)
        {
            Grid_PreviewDragLeave(sender, e);
            if (e.Data.GetData(DataFormats.FileDrop) == null)
                return;
            e.Handled = true;
            string filename = ((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            SetSubtitle(filename);
        }

        private void SetSubtitle(string path)
        {
            encParam.SetSubtitlePath(path);
            SubtitleNote.State = StateNote.States.Processing;
            if (subtitleCheckThread != null)
                subtitleCheckThread.Abort();
            subtitleCheckThread = new Thread(delegate ()
            {
                if (Enc.IsSubtitleAvailable(path))
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        SubtitleNote.State = StateNote.States.Loaded;
                    }));
                }
                else
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        SubtitleNote.State = StateNote.States.Unavailable;
                    }));
                }
            });
            subtitleCheckThread.Start();
        }

        // About switching status

        private void Note_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            StateNote stateNote = (StateNote)sender;
            if (stateNote.State == StateNote.States.Loaded)
                stateNote.State = StateNote.States.Copy;
            else if (stateNote.State == StateNote.States.Copy)
                stateNote.State = StateNote.States.Loaded;
        }

        private void Note_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            StateNote stateNote = (StateNote)sender;
            if (stateNote.State != StateNote.States.Processing && stateNote.State != StateNote.States.Unseted)
            {
                stateNote.State = StateNote.States.Unseted;
                if (stateNote.Name == "VideoNote")
                    encParam.VideoPath = null;
                else if (stateNote.Name == "AudioNote")
                    encParam.AudioPath = null;
                else if (stateNote.Name == "SubtitleNote")
                    encParam.SubtitlePath = null;
            } 
        }

        // About Start Processing

        private Enc enc;
        private EncParam encParam;
        private void StartBox_SelectionChanged(object sender, int index, string value)
        {
            if(index == 1)
            {
                if (VideoNote.State != StateNote.States.Processing && AudioNote.State != StateNote.States.Processing && SubtitleNote.State != StateNote.States.Processing)
                    if (VideoNote.State == StateNote.States.Loaded || VideoNote.State == StateNote.States.Copy || AudioNote.State == StateNote.States.Loaded || AudioNote.State == StateNote.States.Copy)
                        if (ProcessModeBox.SelectedIndex == 0)
                            RunProcess();
                        else
                            RunQueue();
                    else
                        StartBox.SelectedIndex = 0;
                else
                    StartBox.SelectedIndex = 0;
            }
            else
            {
                if (queueThread != null)
                    queueThread.Abort();
                if (enc != null)
                    new Thread(delegate ()
                    {
                        enc.Abort();
                        Enc_Finished();
                    }).Start();
            }
        }

        private void RunProcess()
        {
            SetThreadExecutionState(ES_CONTINUOUS | ES_DISPLAY_REQUIRED | ES_SYSTEM_REQUIRED);
            ProgressBar.UpdateProgress(-1);
            ProgressBar.ProgressChanged += ProgressBar_ProgressChanged;

            switch (VideoModeBox.SelectedIndex)
            {
                case 0:
                    encParam.SetVideoConfig(EncParam.VideoModes.CRF, double.Parse(VideoCRFBox.Text), int.Parse(VideoBitrateBox.Text), VideoParamBox.Text);
                    break;
                case 1:
                    encParam.SetVideoConfig(EncParam.VideoModes.B1PASS, double.Parse(VideoCRFBox.Text), int.Parse(VideoBitrateBox.Text), VideoParamBox.Text);
                    break;
                case 2:
                    encParam.SetVideoConfig(EncParam.VideoModes.B2PASS, double.Parse(VideoCRFBox.Text), int.Parse(VideoBitrateBox.Text), VideoParamBox.Text);
                    break;
                case 3:
                    encParam.SetVideoConfig(EncParam.VideoModes.B3PASS, double.Parse(VideoCRFBox.Text), int.Parse(VideoBitrateBox.Text), VideoParamBox.Text);
                    break;
                case 4:
                    encParam.SetVideoConfig(EncParam.VideoModes.AUTO, double.Parse(VideoCRFBox.Text), int.Parse(VideoBitrateBox.Text), VideoParamBox.Text);
                    break;
            }
            encParam.SetAudioConfig(int.Parse(AudioBitrateBox.Text), AudioParamBox.Text);

            enc = new Enc(encParam);
            enc.ProgressUpdate += Enc_ProgressUpdate;
            enc.Finished += Enc_Finished;

            enc.Start(ConvertFromNote(VideoNote, AudioNote, SubtitleNote), false);

            if(VideoNote.State == StateNote.States.Loaded || VideoNote.State == StateNote.States.Copy)
                VideoNote.State = StateNote.States.Processing;
            if (AudioNote.State == StateNote.States.Loaded || AudioNote.State == StateNote.States.Copy)
                AudioNote.State = StateNote.States.Processing;
            if (SubtitleNote.State == StateNote.States.Loaded || SubtitleNote.State == StateNote.States.Copy)
                SubtitleNote.State = StateNote.States.Processing;
            Overlay.Visibility = Visibility.Visible;
        }

        private void Enc_ProgressUpdate(double value)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                ProgressBar.UpdateProgress(value);
            }));
        }

        private void Enc_Finished()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                SetThreadExecutionState(ES_CONTINUOUS);
                enc = null;
                ProgressBar.UpdateProgress(100);
                StartBox.SelectedIndex = 0;

                if(encParam.VideoPath != null)
                    SetVideo(encParam.VideoPath);
                if (encParam.AudioPath != null)
                    SetAudio(encParam.AudioPath);
                if (encParam.SubtitlePath != null)
                    SetSubtitle(encParam.SubtitlePath);
                Overlay.Visibility = Visibility.Hidden;
            }));
        }

        private EncNote ConvertFromNote(StateNote video, StateNote audio, StateNote subtitle)
        {
            EncNote encState = new EncNote();
            if (video.State == StateNote.States.Loaded)
                encState.Video = EncNote.TrackStates.Process;
            else if (video.State == StateNote.States.Copy)
                encState.Video = EncNote.TrackStates.Copy;
            if (audio.State == StateNote.States.Loaded)
                encState.Audio = EncNote.TrackStates.Process;
            else if (audio.State == StateNote.States.Copy)
                encState.Audio = EncNote.TrackStates.Copy;
            if (subtitle.State == StateNote.States.Loaded)
                encState.Subtitle = EncNote.TrackStates.Process;
            else if (subtitle.State == StateNote.States.Copy)
                encState.Subtitle = EncNote.TrackStates.Copy;
            return encState;
        }

        // About Queue

        private void ProcessModeBox_SelectionChanged(object sender, int index, string value)
        {
            if (index == 1)
                ShowQueueBox(true);
            else
                ShowQueueBox(false);
        }

        Thread queueBoxAnimationThread;
        private void ShowQueueBox(bool isShow)
        {
            if (queueBoxAnimationThread != null)
                queueBoxAnimationThread.Abort();
            GridLength oldWidth = QueueBoxLeft.Width;
            GridLength newWidth;
            if(isShow)
                newWidth = new GridLength(1, GridUnitType.Star);
            else
                newWidth = new GridLength(0, GridUnitType.Star);
            queueBoxAnimationThread = new Thread(delegate ()
            {
                double x = 0;
                while (x < Math.PI)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        x += 0.05;
                        double y = (Math.Cos(x) + 1) / 2;
                        double currentWidth = oldWidth.Value + (1 - y) * (newWidth.Value - oldWidth.Value);
                        QueueBoxLeft.Width = new GridLength(currentWidth, GridUnitType.Star);
                        QueueBoxRight.Width = new GridLength(1 - currentWidth, GridUnitType.Star);
                    }));
                    Thread.Sleep(5);
                }
            });
            queueBoxAnimationThread.Start();
        }

        private void JoinQueueBox_SelectionChanged(object sender, int index, string value)
        {
            if ((QueueList.SelectedIndex == QueueList.Items.Count - 1 && index == 0) || (QueueList.SelectedIndex != QueueList.Items.Count - 1 && index == 1))
                return;
            if(index == 0)
            {
                QueueList.Items.RemoveAt(QueueList.SelectedIndex);
                QueueList.SelectedIndex = QueueList.Items.Count - 1;
            }
            else
            {
                if (VideoNote.State != StateNote.States.Processing && AudioNote.State != StateNote.States.Processing && SubtitleNote.State != StateNote.States.Processing)
                    if (VideoNote.State == StateNote.States.Loaded || VideoNote.State == StateNote.States.Copy || AudioNote.State == StateNote.States.Loaded || AudioNote.State == StateNote.States.Copy)
                        AddToQueue();
                    else
                        JoinQueueBox.SelectedIndex = 0;
                else
                    JoinQueueBox.SelectedIndex = 0;
            }
        }

        [Serializable]
        class ConfigTag
        {
            public StateNote.States VideoState, AudioState, SubtitleState;
            public int VideoMode;
            public string VideoPath, VideoCRF, VideoBitrate, VideoParam, AudioPath, AudioBitrate, AudioParam, SubtitlePath;
            public bool CRFIsEnabled, BitrateIsEnabled;
        }

        private ConfigTag GetConfigTag()
        {
            ConfigTag tag = new ConfigTag
            {
                VideoState = VideoNote.State,
                AudioState = AudioNote.State,
                SubtitleState = SubtitleNote.State,
                VideoMode = VideoModeBox.SelectedIndex,
                VideoPath = encParam.VideoPath,
                VideoCRF = VideoCRFBox.Text,
                VideoBitrate = VideoBitrateBox.Text,
                VideoParam = VideoParamBox.Text,
                AudioPath = encParam.AudioPath,
                AudioBitrate = AudioBitrateBox.Text,
                AudioParam = AudioParamBox.Text,
                SubtitlePath = encParam.SubtitlePath,
                CRFIsEnabled = VideoCRFBox.IsEnabled,
                BitrateIsEnabled = VideoBitrateBox.IsEnabled
            };
            return tag;
        }

        private void ApplyConfigTag(ConfigTag tag)
        {
            VideoNote.State = tag.VideoState;
            AudioNote.State = tag.AudioState;
            SubtitleNote.State = tag.SubtitleState;
            VideoModeBox.SelectedIndex = tag.VideoMode;
            encParam.VideoPath = tag.VideoPath;
            VideoCRFBox.Text = tag.VideoCRF;
            VideoBitrateBox.Text = tag.VideoBitrate;
            VideoParamBox.Text = tag.VideoParam;
            encParam.AudioPath = tag.AudioPath;
            AudioBitrateBox.Text = tag.AudioBitrate;
            AudioParamBox.Text = tag.AudioParam;
            encParam.SubtitlePath = tag.SubtitlePath;
            VideoCRFBox.IsEnabled = tag.CRFIsEnabled;
            VideoBitrateBox.IsEnabled = tag.BitrateIsEnabled;
        }

        private void AddToQueue()
        {
            ListBoxItem listBoxItem = new ListBoxItem
            {
                Content = ""
            };
            EncNote encNote = ConvertFromNote(VideoNote, AudioNote, SubtitleNote);
            if (encNote.Video != EncNote.TrackStates.None)
                listBoxItem.Content += encParam.VideoPath.Substring(encParam.VideoPath.LastIndexOf('\\') + 1);
            else
                listBoxItem.Content += Application.Current.FindResource("No_Video").ToString();
            listBoxItem.Content += "\r\n";
            if (encNote.Audio != EncNote.TrackStates.None)
                listBoxItem.Content += encParam.AudioPath.Substring(encParam.AudioPath.LastIndexOf('\\') + 1);
            else
                listBoxItem.Content += Application.Current.FindResource("No_Audio").ToString();
            listBoxItem.Content += "\r\n";
            if (encNote.Subtitle != EncNote.TrackStates.None)
                listBoxItem.Content += encParam.SubtitlePath.Substring(encParam.SubtitlePath.LastIndexOf('\\') + 1);
            else
                listBoxItem.Content += Application.Current.FindResource("No_Subtitle").ToString();

            ConfigTag tag = GetConfigTag();

            listBoxItem.Tag = tag;

            QueueList.Items.Insert(QueueList.Items.Count - 1, listBoxItem);
            JoinQueueBox.SelectedIndex = 0;
        }

        private void QueueList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(QueueList.SelectedIndex == QueueList.Items.Count - 1)
            {
                JoinQueueBox.SelectedIndex = 0;
                ConfigOverlay.Visibility = Visibility.Hidden;
            }
            else
            {
                JoinQueueBox.SelectedIndex = 1;
                ConfigOverlay.Visibility = Visibility.Visible;

                if(QueueList.SelectedItem != null)
                {
                    ConfigTag tag = (ConfigTag)((ListBoxItem)(QueueList.SelectedItem)).Tag;
                    ApplyConfigTag(tag);
                }
            }
        }

        Thread queueThread;
        int totalQueue = 0, currentQueue = 0;
        private void RunQueue()
        {
            SetThreadExecutionState(ES_CONTINUOUS | ES_DISPLAY_REQUIRED | ES_SYSTEM_REQUIRED);
            ProgressBar.UpdateProgress(-1);
            ProgressBar.ProgressChanged += ProgressBar_ProgressChanged;
            Overlay.Visibility = Visibility.Visible;

            totalQueue = QueueList.Items.Count - 1;
            currentQueue = 0;
            queueThread = new Thread(delegate ()
            {
                while (QueueList.Items.Count > 1)
                {
                    ConfigTag tag = null;
                    Dispatcher.Invoke(new Action(() =>
                    {
                        QueueList.SelectedIndex = 0;
                        tag = (ConfigTag)((ListBoxItem)(QueueList.SelectedItem)).Tag;

                        encParam.VideoPath = tag.VideoPath;
                        encParam.AudioPath = tag.AudioPath;
                        encParam.SubtitlePath = tag.SubtitlePath;

                        switch (tag.VideoMode)
                        {
                            case 0:
                                encParam.SetVideoConfig(EncParam.VideoModes.CRF, double.Parse(tag.VideoCRF), int.Parse(tag.VideoBitrate), tag.VideoParam);
                                break;
                            case 1:
                                encParam.SetVideoConfig(EncParam.VideoModes.B1PASS, double.Parse(tag.VideoCRF), int.Parse(tag.VideoBitrate), tag.VideoParam);
                                break;
                            case 2:
                                encParam.SetVideoConfig(EncParam.VideoModes.B2PASS, double.Parse(tag.VideoCRF), int.Parse(tag.VideoBitrate), tag.VideoParam);
                                break;
                            case 3:
                                encParam.SetVideoConfig(EncParam.VideoModes.B3PASS, double.Parse(tag.VideoCRF), int.Parse(tag.VideoBitrate), tag.VideoParam);
                                break;
                            case 4:
                                encParam.SetVideoConfig(EncParam.VideoModes.AUTO, double.Parse(tag.VideoCRF), int.Parse(tag.VideoBitrate), tag.VideoParam);
                                break;
                        }
                        encParam.SetAudioConfig(int.Parse(tag.AudioBitrate), tag.AudioParam);

                        enc = new Enc(encParam);
                        enc.ProgressUpdate += Enc_QueueProgressUpdate;

                        if (tag.VideoState == StateNote.States.Loaded || tag.VideoState == StateNote.States.Copy)
                            VideoNote.State = StateNote.States.Processing;
                        if (tag.AudioState == StateNote.States.Loaded || tag.AudioState == StateNote.States.Copy)
                            AudioNote.State = StateNote.States.Processing;
                        if (tag.SubtitleState == StateNote.States.Loaded || tag.SubtitleState == StateNote.States.Copy)
                            SubtitleNote.State = StateNote.States.Processing;

                    }));

                    EncNote encState = new EncNote();
                    if (tag.VideoState == StateNote.States.Loaded)
                        encState.Video = EncNote.TrackStates.Process;
                    else if (tag.VideoState == StateNote.States.Copy)
                        encState.Video = EncNote.TrackStates.Copy;
                    if (tag.AudioState == StateNote.States.Loaded)
                        encState.Audio = EncNote.TrackStates.Process;
                    else if (tag.AudioState == StateNote.States.Copy)
                        encState.Audio = EncNote.TrackStates.Copy;
                    if (tag.SubtitleState == StateNote.States.Loaded)
                        encState.Subtitle = EncNote.TrackStates.Process;
                    else if (tag.SubtitleState == StateNote.States.Copy)
                        encState.Subtitle = EncNote.TrackStates.Copy;
                    enc.Start(encState, true);

                    Dispatcher.Invoke(new Action(() =>
                    {
                        QueueList.Items.RemoveAt(0);
                    }));
                    currentQueue++;
                }
                Dispatcher.Invoke(new Action(() =>
                {
                    QueueList.SelectedIndex = 0;
                }));
                Enc_Finished();
                queueThread = null;
            });
            queueThread.Start();
        }

        private void Enc_QueueProgressUpdate(double value)
        {
            double queueProgress = 100 * currentQueue / totalQueue + value / totalQueue;
            Dispatcher.Invoke(new Action(() =>
            {
                ProgressBar.UpdateProgress(queueProgress);
            }));
        }

        // About progress display

        private void ProgressBar_ProgressChanged(double value)
        {
            if (value == 100)
            {
                ProgressBar.ProgressChanged -= ProgressBar_ProgressChanged;
                ProgressBox.Text = Application.Current.FindResource("QEnc").ToString();
                ((Storyboard)Resources["ShowProgressBox"]).Begin();
            }
            else
            {
                if (ProgressBox.Text == Application.Current.FindResource("QEnc").ToString())
                    ((Storyboard)Resources["ShowProgressBox"]).Begin();
                ProgressBox.Text = value.ToString("0.00") + "%";
            }

        }

        //About exit

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (enc != null)
            {
                enc.Abort();
                enc = null;
            }
            SaveConfig();
            if (ProgressBar.BarOpacity != 0)
            {
                ProgressBar.UpdateProgress(-1);
                ProgressBar.Reseted += delegate ()
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        ((Storyboard)Resources["HideWindow"]).Completed += delegate
                        {
                            Dispatcher.Invoke(new Action(() =>
                            {
                                ProgressBar.Stop();
                                this.Close();
                            }));
                        };
                        ((Storyboard)Resources["HideWindow"]).Begin();
                    }));
                };
                e.Cancel = true;
            }
        }

        // About config saving/loading

        private void SaveConfig()
        {
            ConfigTag tag = GetConfigTag();
            tag.VideoState = StateNote.States.Unseted;
            tag.VideoPath = null;
            tag.AudioState = StateNote.States.Unseted;
            tag.AudioPath = null;
            tag.SubtitleState = StateNote.States.Unseted;
            tag.SubtitlePath = null;

            string fileDirectory = System.IO.Path.GetTempPath() + "QEnc\\";
            if (!Directory.Exists(fileDirectory))
                Directory.CreateDirectory(fileDirectory);
            string fileName = "config";
            Stream stream = new FileStream(fileDirectory + fileName + ".dat", FileMode.Create, FileAccess.ReadWrite);
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(stream, tag);
            stream.Close();
        }

        private void LoadConfig()
        {
            string fileDirectory = System.IO.Path.GetTempPath() + "QEnc\\";
            string fileName = "config";
            try
            {
                Stream stream = new FileStream(fileDirectory + fileName + ".dat", FileMode.Open, FileAccess.Read);
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                ConfigTag config = (ConfigTag)binaryFormatter.Deserialize(stream);
                stream.Close();
                ApplyConfigTag(config);
            }
            catch (Exception)
            {

            }
        }
    }

    public class OpacityMaskRectConverter : IMultiValueConverter
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

    public class InnerRadiusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value / 2 * 2/3;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class MinWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value + 350;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
