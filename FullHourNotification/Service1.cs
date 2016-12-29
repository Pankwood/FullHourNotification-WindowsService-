using System;
using System.ServiceProcess;
using System.Timers;

namespace FullHourNotification
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        private void SetupTimer()
        {
            //The component Timer doesn't work, then we need to create one
            //http://stackoverflow.com/questions/4561479/timer-tick-event-is-not-called-in-windows-service
            Timer _timer = new Timer();
            // In miliseconds 60000 = 1 minute
            _timer.Interval += 1000; //1 second
            // Activate the timer
            _timer.Enabled = true;
            // When timer "tick"
            _timer.Elapsed += new ElapsedEventHandler(_timer_Elapsed);
        }

        protected override void OnStart(string[] args)
        {
            SetupTimer();
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //Alert me each full hour(e.g 12h00, 13h00...)
            if (DateTime.Now.Minute == 00)
            {
                AudioService playMyAudio = new AudioService();
                playMyAudio.WASAPI(AppDomain.CurrentDomain.BaseDirectory + "alert.wav");
            }
        }

        protected override void OnStop()
        {
        }

    }
}
