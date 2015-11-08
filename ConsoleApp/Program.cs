using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpotifyTools.Domain;
using SpotifyTools.Tools;

namespace PreventSleep
{
    class Program
    {
        static void Main(string[] args)
        {
            var spotifyAnalyser = new SpotifySaveModeStopper(new MessageDisplayer(), new PreventSleepScreen(), new SoundAnalyser());
            spotifyAnalyser.StartListening();

            while (true)
                Task.Delay(3000).Wait();



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
