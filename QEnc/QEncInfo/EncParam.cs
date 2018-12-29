using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QEncInfo
{
    [Serializable]
    public class EncParam
    {
        public string VideoPath;
        public enum VideoModes { CRF, B1PASS, B2PASS, B3PASS }
        public VideoModes VideoMode;
        public double VideoCRF;
        public int VideoBitrate;
        public string VideoParam;
        public string AudioPath;
        public int AudioBitrate;
        public string AudioParam;
        public string SubtitlePath;

        public EncParam()
        {

        }

        public EncParam(string videoPath, double videoCRF, string videoParam, string audioPath, int audioBitrate, string audioParam, string subtitlePath)
        {
            VideoPath = videoPath;
            VideoMode = VideoModes.CRF;
            VideoCRF = videoCRF;
            VideoParam = videoParam;
            AudioPath = audioPath;
            AudioBitrate = audioBitrate;
            AudioParam = audioParam;
            SubtitlePath = subtitlePath;
        }

        public EncParam(string videoPath, VideoModes videoMode, int videoBitrate, string videoParam, string audioPath, int audioBitrate, string audioParam, string subtitlePath)
        {
            VideoPath = videoPath;
            VideoMode = videoMode;
            VideoBitrate = videoBitrate;
            VideoParam = videoParam;
            AudioPath = audioPath;
            AudioBitrate = audioBitrate;
            AudioParam = audioParam;
            SubtitlePath = subtitlePath;
        }

        public void SetVideoPath(string videoPath)
        {
            VideoPath = videoPath;
        }

        public void SetVideoConfig(double videoCRF, string videoParam)
        {
            VideoMode = VideoModes.CRF;
            VideoCRF = videoCRF;
            VideoParam = videoParam;
        }

        public void SetVideoConfig(VideoModes videoMode, int videoBitrate, string videoParam)
        {
            VideoMode = videoMode;
            VideoBitrate = videoBitrate;
            VideoParam = videoParam;
        }

        public void SetAudioPath(string audioPath)
        {
            AudioPath = audioPath;
        }

        public void SetAudioConfig(int audioBitrate, string audioParam)
        {
            AudioBitrate = audioBitrate;
            AudioParam = audioParam;
        }

        public void SetSubtitlePath(string subtitlePath)
        {
            SubtitlePath = subtitlePath;
        }
    }
}
