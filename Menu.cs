using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Shell;
using IWshRuntimeLibrary; // COM reference to Windows Script Host Object Model

namespace CustomHomescreen
{
    public partial class Menu : Form
    {
        private readonly ValorantWatcher watcher;
        private readonly string valorantMP4Path = @"C:\Riot Games\VALORANT\live\ShooterGame\Content\Movies\Menu\11_00_Homescreen.mp4";
        private readonly string configPath = "config.txt";
        private readonly string logPath = "startup.log";

        private NotifyIcon trayIcon = new NotifyIcon();
        private ContextMenuStrip trayMenu = new ContextMenuStrip();
        private bool reallyExit = false;
        private Icon appIcon; // Store icon to reuse

        public Menu(ValorantWatcher watcher)
        {
            InitializeComponent();
            this.Load += Menu_Load;
            this.watcher = watcher;

            try
            {
                Log("App initializing...");

                // Create the startup shortcut for the launcher (only if it doesn't exist)
                try
                {
                    StartupShortcutCreator.CreateLauncherShortcut();
                    Log("Launcher shortcut created or already exists.");
                }
                catch (Exception ex)
                {
                    Log("Failed to create launcher shortcut: " + ex.Message);
                }

                // Load icon from app directory
                string exeDir = Application.StartupPath;
                string iconPath = Path.Combine(exeDir, "icon.ico");
                if (!System.IO.File.Exists(iconPath))
                    throw new FileNotFoundException("icon.ico not found in application directory", iconPath);

                appIcon = new Icon(iconPath);
                //this.Icon = appIcon;

                trayMenu = new ContextMenuStrip();
                trayMenu.Items.Add("Open", null, OnTrayOpen);
                trayMenu.Items.Add("Exit", null, OnTrayExit);

                trayIcon = new NotifyIcon
                {
                    Text = "Valorant Menu Mod",
                    Icon = appIcon,
                    ContextMenuStrip = trayMenu,
                    Visible = true
                };

                this.FormClosing += Form1_FormClosing;
                this.KeyPreview = true;
                this.KeyDown += Menu_KeyDown;

                ApplySavedVideo();
                CheckIfValorantRunning();

                Log("App initialized successfully.");
            }
            catch (Exception ex)
            {
                Log("Initialization error: " + ex);
                MessageBox.Show("Startup error: " + ex.Message);
            }
        }

        private void Menu_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control && e.Shift && e.KeyCode == Keys.L)
            {
                if (System.IO.File.Exists(logPath))
                    Process.Start("notepad.exe", logPath);
            }
        }

        private void OnTrayOpen(object? sender, EventArgs e)
        {
            this.Show();
            
        }

        private void OnTrayExit(object? sender, EventArgs e)
        {
            
            watcher.Stop();
            reallyExit = true;
            this.Close();
        }

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (!reallyExit)
            {
                e.Cancel = true;
                this.Hide();
                
            }
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            using OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "MP4 Files (*.mp4)|*.mp4",
                Title = "Select a background MP4"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    System.IO.File.WriteAllText(configPath, ofd.FileName);
                    UpdatePreview(ofd.FileName);
                    MessageBox.Show("Video saved. It will apply the next time Valorant starts.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Log("Video path saved: " + ofd.FileName);
                }
                catch (Exception ex)
                {
                    Log("Failed to save video: " + ex);
                    MessageBox.Show("Error saving file:\n" + ex.Message);
                }
            }
        }

        private async void Menu_Load(object? sender, EventArgs e)
        {
            await Task.Delay(250); // wait 250ms to let Windows settle after UAC
            this.Icon = appIcon;
            // optionally, if you want to show here:
            // this.Show();

        }
        private void ApplySavedVideo()
        {
            try
            {
                if (System.IO.File.Exists(configPath))
                {
                    string savedPath = System.IO.File.ReadAllText(configPath);
                    if (System.IO.File.Exists(savedPath))
                        UpdatePreview(savedPath);
                    else
                        picPreview.Image = null;
                }
                else
                {
                    picPreview.Image = null;
                }
            }
            catch (Exception ex)
            {
                Log("Failed to apply saved video: " + ex);
            }
        }

        private void UpdatePreview(string videoPath)
        {
            if (!System.IO.File.Exists(videoPath))
            {
                picPreview.Image = null;
                return;
            }

            try
            {
                var shellFile = ShellFile.FromFilePath(videoPath);
                Image thumbnail = shellFile.Thumbnail.LargeBitmap;

                if (picPreview.Image != null)
                    picPreview.Image.Dispose();

                picPreview.Image = thumbnail;
            }
            catch (Exception ex)
            {
                Log("Thumbnail error: " + ex);
                picPreview.Image = null;
            }
        }

        private void CheckIfValorantRunning()
        {
            bool isRunning = Process.GetProcessesByName("VALORANT").Any();
            Log("VALORANT running: " + isRunning);
        }

        private void Log(string message)
        {
            try
            {
                System.IO.File.AppendAllText(logPath, $"{DateTime.Now}: {message}\n");
            }
            catch { }
        }
    }
}
