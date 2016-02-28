using System;
using SpotifyTools.Contracts;

namespace SpotifyTools.DomainLayer.MessageManagement
{
    public class MessageDisplayer : IMessageDisplayer
    {
        public void OutputMessage(string mess)
        {
            Console.WriteLine(mess);
        }
    }
}
