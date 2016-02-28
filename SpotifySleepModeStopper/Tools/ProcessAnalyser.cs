using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpotifyTools.Contracts;

namespace SpotifyTools.Tools
{
    public class ProcessAnalyser : IProcessAnalyser
    {
        public bool IsAppRunning(string processName)
        {
            return Process.GetProcessesByName(processName).Any();
        }
    }
}
