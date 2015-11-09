using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpotifyTools.Domain;
using SpotifyTools.Domain.AudioManagement;
using SpotifyTools.Domain.MessageManagement;
using SpotifyTools.Domain.PowerManagement;
using SpotifyTools.Tools;

namespace PreventSleep
{
    class Program
    {
        static void Main(string[] args)
        {
            var spotifyAnalyser = new SpotifySaveModeStopper(new MessageDisplayer(), new PowerRequestContextHandler(), new CsCoreSoundAnalyser());
            spotifyAnalyser.StartListening();
            
            Console.ReadKey();
            
            //var preventer = new PreventSleepScreen();
            //while (true)
            //{
            //    Console.WriteLine("Sleep Screen is Enabled");

            //    Console.ReadKey();
            //    preventer.EnableConstantDisplayAndPower(true);
            //    Console.WriteLine("Sleep Screen is Disabled");

            //    Console.ReadKey();
            //    preventer.EnableConstantDisplayAndPower(false);
            //}
        }
    }
}
