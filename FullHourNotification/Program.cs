using System.ServiceProcess;

namespace FullHourNotification
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
#if DEBUG
            //if I need to test something, I used this way.
            Service1 service = new Service1();
            System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
#else
            
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new Service1() 
            };
            ServiceBase.Run(ServicesToRun);
#endif
        }
    }
}
