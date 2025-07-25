using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CustomHomescreen
{
    public static class StartupShortcutCreator
    {
        public static void CreateLauncherShortcut()
        {
            string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string shortcutPath = System.IO.Path.Combine(startupFolder, "CustomHomescreenLauncher.lnk");

            if (System.IO.File.Exists(shortcutPath))
            {
                // Shortcut already exists, no need to recreate
                return;
            }

            string launcherExePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Launch.vbs");

            if (!System.IO.File.Exists(launcherExePath))
                throw new FileNotFoundException("Launcher executable not found.", launcherExePath);

            var shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
            shortcut.TargetPath = launcherExePath;
            shortcut.WorkingDirectory = System.IO.Path.GetDirectoryName(launcherExePath);
            shortcut.Description = "Launch Valorant Menu Mod (Launcher)";
            shortcut.Save();
        }
    }
}
