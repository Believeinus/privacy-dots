using System;
using System.Threading;
using System.Windows.Forms;

namespace PrivacyDots
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            bool createdNew;
            using (Mutex mutex = new Mutex(true, "PrivacyDotsSingleInstance", out createdNew))
            {
                if (!createdNew)
                {
                    return; // already running
                }
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new TrayContext(args));
                GC.KeepAlive(mutex);
            }
        }
    }
}
