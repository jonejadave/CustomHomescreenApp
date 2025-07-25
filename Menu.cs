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

        // Single config file paths
        private readonly string configPath = "config.txt";
        private readonly string logPath = "startup.log";

        private readonly string valorantFolderPathConfig = "valorantpath.txt"; // Stores Valorant folder path
        private string valorantFolderPath = ""; // Loaded folder path

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
                string iconPath = System.IO.Path.Combine(exeDir, "icon.ico");
                if (!System.IO.File.Exists(iconPath))
                    throw new FileNotFoundException("icon.ico not found in application directory", iconPath);

                appIcon = new Icon(iconPath);

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

                // Load saved Valorant folder path here:
                LoadValorantFolderPath();

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

        // Existing button to select the MP4 video file
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

        // New button event to select Valorant menu folder
        private void btnSelectValorantFolder_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (!string.IsNullOrEmpty(valorantFolderPath) && Directory.Exists(valorantFolderPath))
                    fbd.SelectedPath = valorantFolderPath;

                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    string expectedMP4 = System.IO.Path.Combine(fbd.SelectedPath, "11_00_Homescreen.mp4");
                    if (!System.IO.File.Exists(expectedMP4))
                    {
                        var result = MessageBox.Show(
                            "Warning: The selected folder does not contain 11_00_Homescreen.mp4.\nAre you sure this is the correct folder?",
                            "Folder check", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                        if (result == DialogResult.No)
                            return;
                    }

                    SaveValorantFolderPath(fbd.SelectedPath);
                    MessageBox.Show("Valorant folder saved. It will be used next time.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Log("Valorant folder path selected: " + fbd.SelectedPath);
                }
            }
        }

        private async void Menu_Load(object? sender, EventArgs e)
        {
            await System.Threading.Tasks.Task.Delay(250);
            this.Icon = appIcon;
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

        // Load the saved Valorant folder path from file
        private void LoadValorantFolderPath()
        {
            try
            {
                if (System.IO.File.Exists(valorantFolderPathConfig))
                {
                    valorantFolderPath = System.IO.File.ReadAllText(valorantFolderPathConfig).Trim();
                    if (!Directory.Exists(valorantFolderPath))
                    {
                        Log("Saved Valorant folder path does not exist: " + valorantFolderPath);
                        valorantFolderPath = "";
                    }
                    else
                    {
                        Log("Loaded saved Valorant folder path: " + valorantFolderPath);
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Error loading Valorant folder path: " + ex);
                valorantFolderPath = "";
            }
        }

        // Save Valorant folder path to file
        private void SaveValorantFolderPath(string path)
        {
            try
            {
                System.IO.File.WriteAllText(valorantFolderPathConfig, path);
                valorantFolderPath = path;
                Log("Saved Valorant folder path: " + path);
            }
            catch (Exception ex)
            {
                Log("Failed to save Valorant folder path: " + ex);
                MessageBox.Show("Failed to save Valorant folder path:\n" + ex.Message);
            }
        }

        // Returns the full path to the MP4 in the user-selected Valorant folder or fallback
        public string GetValorantMP4Path()
        {
            if (!string.IsNullOrEmpty(valorantFolderPath))
            {
                string path = System.IO.Path.Combine(valorantFolderPath, "11_00_Homescreen.mp4");
                if (System.IO.File.Exists(path))
                    return path;
            }

            // fallback hardcoded path
            return @"C:\Riot Games\VALORANT\live\ShooterGame\Content\Movies\Menu\11_00_Homescreen.mp4";
        }
    }
}
