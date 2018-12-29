using QEncInfo;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace QEnc
{
    /// <summary>
    /// Time class for store and convert time
    /// </summary>
    class Time
    {
        public int Hour, Minute, Second, Millisecond;

        public Time()
        {
            Hour = 0;
            Minute = 0;
            Second = 0;
            Millisecond = 0;
        }

        public Time(int hour, int minute, int second, int millisecond)
        {
            Hour = hour;
            Minute = minute;
            Second = second;
            Millisecond = millisecond;
        }

        public double ToSeconds()
        {
            return (double)Millisecond / 1000 + Second + Minute * 60 + Hour * 3600;
        }

        //00:00:00.000
        public static Time ConvertFromString(string str)
        {
            Match match = Regex.Match(str, "(?<Hour>[0-9]{2}):(?<Minute>[0-9]{2}):(?<Second>[0-9]{2})\\.(?<Millisecond>[0-9]{0,3})");
            return new Time(int.Parse(match.Groups["Hour"].Value), int.Parse(match.Groups["Minute"].Value), int.Parse(match.Groups["Second"].Value), (int)(double.Parse("0." + match.Groups["Millisecond"].Value) * 1000));
        }
    }

    /// <summary>
    /// Enc class, Encoding core
    /// </summary>
    class Enc
    {
        public delegate void DelProgressUpdate(double value);
        public event DelProgressUpdate ProgressUpdate;
        public delegate void DelFinished();
        public event DelFinished Finished;

        EncParam encParam;

        public Enc()
        {
        }

        public Enc(EncParam param)
        {
            encParam = param;
        }

        public void SetParam(EncParam param)
        {
            encParam = param;
        }

        private static MatchCollection GetFileInfo(string cmd)
        {
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = "/k \"@echo off\"";
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.Start();
            process.StandardInput.WriteLine(cmd + " & exit");
            string result = process.StandardError.ReadToEnd();
            return Regex.Matches(result, 
                "(Stream #(?<FileNum>[0-9]+):(?<TrackNum>[0-9]+)(\\((?<TrackName>.+)\\))?: (?<Type>Video): )|" +
                "(Stream #(?<FileNum>[0-9]+):(?<TrackNum>[0-9]+)(\\((?<TrackName>.+)\\))?: (?<Type>Audio): )|" +
                "(Stream #(?<FileNum>[0-9]+):(?<TrackNum>[0-9]+)(\\((?<TrackName>.+)\\))?: (?<Type>Subtitle): )");
        }

        public static bool IsVideoAvailable(string path)
        {
            string ffmpegPath = Path.GetTempPath() + "QEnc\\bin\\ffmpeg.exe";
            MatchCollection info = GetFileInfo("\"" + ffmpegPath + "\" -i \"" + path + "\"");
            foreach(Match match in info)
            {
                if (match.Groups["Type"].Value == "Video")
                    return true;
            }
            return false;
        }

        public static bool IsAudioAvailable(string path)
        {
            string ffmpegPath = Path.GetTempPath() + "QEnc\\bin\\ffmpeg.exe";
            MatchCollection info = GetFileInfo("\"" + ffmpegPath + "\" -i \"" + path + "\"");
            foreach (Match match in info)
            {
                if (match.Groups["Type"].Value == "Audio")
                    return true;
            }
            return false;
        }

        public static bool IsSubtitleAvailable(string path)
        {
            string ffmpegPath = Path.GetTempPath() + "QEnc\\bin\\ffmpeg.exe";
            MatchCollection info = GetFileInfo("\"" + ffmpegPath + "\" -i \"" + path + "\"");
            foreach (Match match in info)
            {
                if (match.Groups["Type"].Value == "Subtitle")
                    return true;
            }
            return false;
        }

        private string workingDirectory;
        private EncLog encLog;
        public void Start(EncNote encNote, bool waitForExit)
        {
            encLog = new EncLog(encNote, encParam);
            string ffmpegPath = Path.GetTempPath() + "QEnc\\bin\\ffmpeg.exe";

            List<string> cmdList = new List<string>();
            string finalCmd = "\"" + ffmpegPath + "\" -y";
            int loadIndex = 0;
            int videoIndex = -1, audioIndex = -1;

            //input
            if (encNote.Video != EncNote.TrackStates.None)
            {
                finalCmd += " -i \"" + encParam.VideoPath + "\"";
                if (encParam.VideoMode == EncParam.VideoModes.B2PASS)
                {
                    if(encParam.VideoParam.Trim() == "")
                        cmdList.Add("\"" + ffmpegPath + "\" -y -i \"" + encParam.VideoPath + "\" -map 0:v -c:v libx264 -b:v " + encParam.VideoBitrate + "k -x264opts \"pass=1\" \"" + encParam.VideoPath + "_temp.mp4\"");
                    else
                        cmdList.Add("\"" + ffmpegPath + "\" -y -i \"" + encParam.VideoPath + "\" -map 0:v -c:v libx264 -b:v " + encParam.VideoBitrate + "k -x264opts \"pass=1:" + encParam.VideoParam + "\" \"" + encParam.VideoPath + "_temp.mp4\"");
                }
                else if (encParam.VideoMode == EncParam.VideoModes.B3PASS)
                {
                    if(encParam.VideoParam.Trim() == "")
                    {
                        cmdList.Add("\"" + ffmpegPath + "\" -y -i \"" + encParam.VideoPath + "\" -map 0:v -c:v libx264 -b:v " + encParam.VideoBitrate + "k -x264opts \"pass=1\" \"" + encParam.VideoPath + "_temp.mp4\"");
                        cmdList.Add("\"" + ffmpegPath + "\" -y -i \"" + encParam.VideoPath + "\" -map 0:v -c:v libx264 -b:v " + encParam.VideoBitrate + "k -x264opts \"pass=3\" \"" + encParam.VideoPath + "_temp.mp4\"");
                    }
                    else
                    {
                        cmdList.Add("\"" + ffmpegPath + "\" -y -i \"" + encParam.VideoPath + "\" -map 0:v -c:v libx264 -b:v " + encParam.VideoBitrate + "k -x264opts \"pass=1:" + encParam.VideoParam + "\" \"" + encParam.VideoPath + "_temp.mp4\"");
                        cmdList.Add("\"" + ffmpegPath + "\" -y -i \"" + encParam.VideoPath + "\" -map 0:v -c:v libx264 -b:v " + encParam.VideoBitrate + "k -x264opts \"pass=3:" + encParam.VideoParam + "\" \"" + encParam.VideoPath + "_temp.mp4\"");
                    }
                }
                videoIndex = loadIndex;
                loadIndex++;
            }
            if (encNote.Audio != EncNote.TrackStates.None)
            {
                finalCmd += " -i \"" + encParam.AudioPath + "\"";
                audioIndex = loadIndex;
                loadIndex++;
            }

            //codec config
            if (encNote.Video == EncNote.TrackStates.Process)
            {
                if (encParam.VideoParam.Trim() == "")
                {
                    if (encParam.VideoMode == EncParam.VideoModes.CRF)
                        finalCmd += " -c:v libx264 -crf " + encParam.VideoCRF;
                    else if (encParam.VideoMode == EncParam.VideoModes.B1PASS)
                        finalCmd += " -c:v libx264 -b:v " + encParam.VideoBitrate;
                    else
                        finalCmd += " -c:v libx264 -b:v " + encParam.VideoBitrate + "k -x264opts \"pass=2\"";
                }
                else
                {
                    if (encParam.VideoMode == EncParam.VideoModes.CRF)
                        finalCmd += " -c:v libx264 -crf " + encParam.VideoCRF + " -x264opts \"" + encParam.VideoParam.Trim() + "\"";
                    else if (encParam.VideoMode == EncParam.VideoModes.B1PASS)
                        finalCmd += " -c:v libx264 -b:v " + encParam.VideoBitrate + "k -x264opts \"" + encParam.VideoParam.Trim() + "\"";
                    else
                        finalCmd += " -c:v libx264 -b:v " + encParam.VideoBitrate + "k -x264opts \"pass=2:" + encParam.VideoParam.Trim() + "\"";
                }
            }
            else if (encNote.Video == EncNote.TrackStates.Copy)
                finalCmd += " -c:v copy";
            if (encNote.Audio == EncNote.TrackStates.Process)
                finalCmd += " -c:a aac -b:a " + encParam.AudioBitrate + "k";
            else if (encNote.Audio == EncNote.TrackStates.Copy)
                finalCmd += " -c:a copy";
            if (encNote.Subtitle != EncNote.TrackStates.None)
                finalCmd += " -vf subtitles=subtitle" + encParam.SubtitlePath.Substring(encParam.SubtitlePath.LastIndexOf('.'));

            //map
            if (encNote.Video != EncNote.TrackStates.None)
                finalCmd += " -map " + videoIndex + ":v";
            if (encNote.Audio != EncNote.TrackStates.None)
                finalCmd += " -map " + audioIndex + ":a";

            //output
            string outout = "";
            if (encNote.Video != EncNote.TrackStates.None)
            {
                outout = encParam.VideoPath + "_QEnc.mp4";
                
            }
            else if (encNote.Audio != EncNote.TrackStates.None)
                outout = encParam.AudioPath + "_QEnc.m4a";
            else if (encNote.Subtitle != EncNote.TrackStates.None)
                outout = encParam.SubtitlePath + "_QEnc.ass";

            int i = 1;
            string filename = outout.Substring(0, outout.LastIndexOf('.'));
            string extention = outout.Substring(outout.LastIndexOf('.'));
            while (File.Exists(outout))
            {
                outout = filename + " (" + i + ")" + extention;
                i++;
            }
            finalCmd += " \"" + outout + "\"";

            //working directory
            if (encNote.Audio != EncNote.TrackStates.None)
            {
                workingDirectory = encParam.AudioPath + "_QEncTemp\\";
                if (!Directory.Exists(workingDirectory))
                    Directory.CreateDirectory(workingDirectory);
                Environment.CurrentDirectory = workingDirectory;
            }
            else if (encNote.Video != EncNote.TrackStates.None)
            {
                workingDirectory = encParam.VideoPath + "_QEncTemp\\";
                if (!Directory.Exists(workingDirectory))
                    Directory.CreateDirectory(workingDirectory);
                Environment.CurrentDirectory = workingDirectory;
            }
            else if (encNote.Subtitle != EncNote.TrackStates.None)
            {
                workingDirectory = encParam.SubtitlePath + "_QEncTemp\\";
                if (!Directory.Exists(workingDirectory))
                    Directory.CreateDirectory(workingDirectory);
                Environment.CurrentDirectory = workingDirectory;
            }

            if (encNote.Subtitle != EncNote.TrackStates.None)
            {
                File.Copy(encParam.SubtitlePath, workingDirectory + "subtitle" + encParam.SubtitlePath.Substring(encParam.SubtitlePath.LastIndexOf('.')), true);
            }

            cmdList.Add(finalCmd);
            string[] cmdArray = cmdList.ToArray();
            ProcessMultiCommand(cmdArray, waitForExit);
        }

        private Thread processThread;
        private int currentStage = 0, totalStage = 0;
        private void ProcessMultiCommand(string[] cmdArray, bool waitForExit)
        {
            totalStage = cmdArray.Length;
            currentStage = 0;
            if (waitForExit)
            {
                foreach (string cmd in cmdArray)
                {
                    ProcessSingleCommand(cmd);
                    currentStage++;
                }
                SaveLog();
                Environment.CurrentDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                Directory.Delete(workingDirectory, true);
                Finished?.Invoke();
            }
            else
            {
                processThread = new Thread(delegate ()
                {
                    foreach (string cmd in cmdArray)
                    {
                        ProcessSingleCommand(cmd);
                        currentStage++;
                    }
                    SaveLog();
                    Environment.CurrentDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                    Directory.Delete(workingDirectory, true);
                    Finished?.Invoke();
                });
                processThread.Start();
            }
        }

        private Process encProcess;
        private double totalSecond = 0;
        private void ProcessSingleCommand(string cmd)
        {
            totalSecond = 0;
            encProcess = new Process();
            encProcess.StartInfo.FileName = "cmd.exe";
            encProcess.StartInfo.Arguments = "/k \"@echo off\"";
            encProcess.StartInfo.RedirectStandardInput = true;
            encProcess.StartInfo.RedirectStandardOutput = true;
            encProcess.StartInfo.RedirectStandardError = true;
            encProcess.StartInfo.CreateNoWindow = true;
            encProcess.StartInfo.UseShellExecute = false;
            encProcess.OutputDataReceived += Process_OutputDataReceived;
            encProcess.ErrorDataReceived += Process_ErrorDataReceived;
            encProcess.Start();
            encProcess.BeginOutputReadLine();
            encProcess.BeginErrorReadLine();
            encProcess.StandardInput.WriteLine(cmd + " & exit");
            encProcess.WaitForExit();
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                encLog.AppendFFLog(e.Data);
                if (e.Data.Contains("Duration:") && Time.ConvertFromString(e.Data).ToSeconds() > totalSecond)
                    totalSecond = Time.ConvertFromString(e.Data).ToSeconds();
                if (e.Data.Contains("time="))
                {
                    double currentSecond = Time.ConvertFromString(e.Data).ToSeconds();
                    ProgressUpdate(100.0 * currentStage / totalStage + 100.0 / totalStage * currentSecond / totalSecond);
                }
            }
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
                encLog.AppendCmdLog(e.Data);
        }

        public void Abort()
        {
            if (processThread != null)
                processThread.Abort();
            if (encProcess != null && !encProcess.HasExited)
            {
                KillProcessAndChildren(encProcess.Id);
            }
            SaveLog();
            Environment.CurrentDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            Finished?.Invoke();
        }

        public void KillProcessAndChildren(int pid)
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection moc = searcher.Get();
            foreach (ManagementObject mo in moc)
            {
                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            }
            try
            {
                Process proc = Process.GetProcessById(pid);
                proc.Kill();
            }
            catch (ArgumentException)
            {

            }
        }

        private void SaveLog()
        {
            encLog.CmdLog = EncodeBase64(encLog.CmdLog);
            encLog.FFLog = EncodeBase64(encLog.FFLog);

            string fileDirectory = Path.GetTempPath() + "QEnc\\log\\";
            if (!Directory.Exists(fileDirectory))
                Directory.CreateDirectory(fileDirectory);
            string fileName = DateTime.Now.ToString().Replace(':', '_').Replace('/', '-') + " " + Environment.CurrentDirectory.Substring(Environment.CurrentDirectory.LastIndexOf('\\') + 1);
            Stream stream = new FileStream(fileDirectory + fileName + ".dat", FileMode.Create, FileAccess.ReadWrite);
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(stream, encLog);
            stream.Close();
        }

        public string EncodeBase64(string code)
        {
            string encode = "";
            byte[] bytes = Encoding.Default.GetBytes(code);
            try
            {
                encode = Convert.ToBase64String(bytes);
            }
            catch
            {
                encode = code;
            }
            return encode;
        }
    }
}
