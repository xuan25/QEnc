using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QEncInfo
{
    [Serializable]
    public class EncLog
    {
        public DateTime StartTime;
        public EncNote Note;
        public EncParam Param;
        public string CmdLog;
        public string FFLog;

        public EncLog()
        {
            StartTime = DateTime.Now;
            Note = null;
            Param = null;
            CmdLog = "";
            FFLog = "";
        }

        public EncLog(EncNote encNote, EncParam encParam)
        {
            StartTime = DateTime.Now;
            Note = encNote;
            Param = encParam;
            CmdLog = "";
            FFLog = "";
        }

        public void AppendCmdLog(string message)
        {
            CmdLog += message + "\r\n";
        }

        public void AppendFFLog(string message)
        {
            FFLog += message + "\r\n";
        }
    }
}
