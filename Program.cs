using System;
using System.Threading;
using System.Windows.Forms;
using CustomHomescreen;

static class Program
{
    [STAThread]
    static void Main()
    {
        bool createdNew;
        using (Mutex mutex = new Mutex(true, "ValorantMenuReplacerMutex", out createdNew))
        {
            if (!createdNew)
            {
                // Another instance is already running
                MessageBox.Show("The application is already running.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var configPath = "config.txt";
          

            var watcher = new ValorantWatcher(configPath);

            Thread watcherThread = new Thread(() => watcher.Start())
            {
                IsBackground = true
            };
            watcherThread.Start();

            Application.Run(new Menu(watcher));
        }
    }
}
