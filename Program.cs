using System;
using System.Threading;
using System.Windows.Forms;
using CustomHomescreen;
using System.IO;

static class Program
{
    static string LoadValorantFolderPath()
    {
        string configFile = "valorantpath.txt";
        try
        {
            if (File.Exists(configFile))
            {
                string path = File.ReadAllText(configFile).Trim();
                if (Directory.Exists(path))
                    return path;
            }
        }
        catch { }
        return ""; // fallback empty if none found
    }

    [STAThread]
    static void Main()
    {
        bool createdNew;
        using (Mutex mutex = new Mutex(true, "ValorantMenuReplacerMutex", out createdNew))
        {
            if (!createdNew)
            {
                MessageBox.Show("The application is already running.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var configPath = "config.txt";
            var valorantFolderPath = LoadValorantFolderPath();

            var watcher = new ValorantWatcher(configPath, valorantFolderPath);

            Thread watcherThread = new Thread(() => watcher.Start())
            {
                IsBackground = true
            };
            watcherThread.Start();

            Application.Run(new Menu(watcher));
        }
    }
}
