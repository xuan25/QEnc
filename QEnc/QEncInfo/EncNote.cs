using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QEncInfo
{
    [Serializable]
    public class EncNote
    {
        public enum TrackStates { None, Process, Copy };
        public TrackStates Video, Audio, Subtitle;
        public EncNote()
        {
            Video = TrackStates.None;
            Audio = TrackStates.None;
            Subtitle = TrackStates.None;
        }

        public EncNote(TrackStates video, TrackStates audio, TrackStates subtitle)
        {
            Video = video;
            Audio = audio;
            Subtitle = subtitle;
        }
    }
}
