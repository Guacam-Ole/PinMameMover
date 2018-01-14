using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PinMameMover
{
    class Program
    {
        public static string VpProcess = System.Configuration.ConfigurationManager.AppSettings["PlayerName"];
        public static string MameProcess = System.Configuration.ConfigurationManager.AppSettings["MameName"];
        public static int X = int.Parse(System.Configuration.ConfigurationManager.AppSettings["X"]);
        public static int Y = int.Parse(System.Configuration.ConfigurationManager.AppSettings["Y"]);
        public static int Width = int.Parse(System.Configuration.ConfigurationManager.AppSettings["Width"]);
        public static int Height = int.Parse(System.Configuration.ConfigurationManager.AppSettings["Height"]);
        public static int Delay = int.Parse(System.Configuration.ConfigurationManager.AppSettings["Delay"]);
        public static int Interval = int.Parse(System.Configuration.ConfigurationManager.AppSettings["Interval"]);


        static void Main(string[] args)
        {
            ProcessDetection detector;
            if (args.Length > 0)
            {
                if (args[0] == "NOMOVE")
                {
                    Console.WriteLine("NOMOVE: Press any key to re-read position from Mame-Window");
                    detector = new ProcessDetection(VpProcess, MameProcess, Delay);
                }
                else if (args[0] == "MANUAL")
                {
                    detector = new ProcessDetection(VpProcess, MameProcess, Delay, X, Y, Width, Height);
                    detector.Manual();
                    return;
                }
                else
                {
                    Console.WriteLine("Aufruf ohne parameter zum verschieben, aufruf mit 'NOMOVE' um nur zu loggen, 'MANUAL' um die Einstellungen bei laufendem PinMame zu testen");
                    return;
                }
            }
            else
            {
                detector = new ProcessDetection(VpProcess, MameProcess, Delay, X, Y, Width, Height);
            }
            if (Interval > 0)
            {
                System.Threading.Timer recheckTimer = new System.Threading.Timer(RecheckCallBack, detector, 0, Interval*1000 );
            }
            detector.DetectChanges();

            while (true)
            {
                System.Threading.Thread.Sleep(1000);
            }
        }

        private static void RecheckCallBack(object detector)
        {

            ((ProcessDetection)detector).Manual();
        }
    }
}
