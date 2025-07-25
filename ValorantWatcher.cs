using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Windows.Forms;

public class ValorantWatcher
{
    private ManagementEventWatcher startWatcher = null!;
    private ManagementEventWatcher stopWatcher = null!;
    private readonly string configPath;
    private readonly string valorantMoviesFolder = @"C:\Riot Games\VALORANT\live\ShooterGame\Content\Movies\Menu";

    public ValorantWatcher(string configPath)
    {
        this.configPath = configPath;
    }

    private string? GetCurrentHomescreenPath()
    {
        if (!Directory.Exists(valorantMoviesFolder))
            return null;

        var mp4Files = Directory.GetFiles(valorantMoviesFolder, "*.mp4");
        if (mp4Files.Length == 0)
            return null;

        // If multiple mp4s exist, choose the largest
        return mp4Files.Length == 1 ? mp4Files[0] :
               mp4Files.OrderByDescending(f => new FileInfo(f).Length).FirstOrDefault();
    }

    public void Start()
    {
        StartWatching();
    }

    public void ApplySavedVideo()
    {
        try
        {
            string? currentMP4 = GetCurrentHomescreenPath();
            if (currentMP4 == null)
            {
                Console.WriteLine("[ApplySavedVideo] No .mp4 file found in the Valorant Movies folder.");
                return;
            }

            if (File.Exists(configPath))
            {
                string savedPath = File.ReadAllText(configPath);
                if (File.Exists(savedPath))
                {
                    File.Copy(savedPath, currentMP4, true);
                    Console.WriteLine($"[ApplySavedVideo] Applied custom video to: {currentMP4}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ApplySavedVideo] Error: {ex.Message}");
        }
    }

    public void StartWatching()
    {
        try
        {
            // Watch for Valorant start
            var startQuery = new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace WHERE ProcessName = 'VALORANT.exe'");
            startWatcher = new ManagementEventWatcher(startQuery);
            startWatcher.EventArrived += (s, e) =>
            {
                try
                {
                    ApplySavedVideo();
                    MessageBox.Show("Valorant has launched and the background was applied.", "Menu Replaced", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[StartWatcher] Error: {ex.Message}");
                }
            };
            startWatcher.Start();

            // Watch for Valorant stop
            var stopQuery = new WqlEventQuery("SELECT * FROM Win32_ProcessStopTrace WHERE ProcessName = 'VALORANT.exe'");
            stopWatcher = new ManagementEventWatcher(stopQuery);
            stopWatcher.EventArrived += (s, e) =>
            {
                try
                {
                    string? currentMP4 = GetCurrentHomescreenPath();
                    if (currentMP4 != null && File.Exists(currentMP4))
                    {
                        File.Delete(currentMP4);
                        Console.WriteLine($"[StopWatcher] Deleted modified file: {currentMP4}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[StopWatcher] Error deleting file: {ex.Message}");
                }
            };
            stopWatcher.Start();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Watcher Init] Error: {ex.Message}");
        }
    }

    public void Stop()
    {
        startWatcher?.Stop();
        startWatcher?.Dispose();

        stopWatcher?.Stop();
        stopWatcher?.Dispose();
    }
}
