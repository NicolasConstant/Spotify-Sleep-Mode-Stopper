using System;
using SpotifyTools.Contracts;

namespace SpotifyTools.Tools
{
    public class MessageDisplayer : IMessageDisplayer
    {
        public void OutputMessage(string mess)
        {
            Console.WriteLine(mess);
        }
    }
}
