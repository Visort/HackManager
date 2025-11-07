using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace launcherDiscord
{
    public partial class Form1 : Form
    {
        private string selectedPreset = "general";
        private ResourceManager resourceManager;
        private Process runningProcess;
        private bool isClosingFromTray = false;
        private System.Timers.Timer processMonitorTimer;

        private Dictionary<string, string> presetBatFiles;
        private List<string> allResourceFiles;

        // Исправленный массив - ровно 20 элементов
        private readonly string[] loadouts = new string[20] {
            "\n> INITIATING SYSTEM BYPASS...",
            "\n> ACCESSING MAINFRAME...",
            "\n> ENCRYPTION PROTOCOL: AES-256",
            "\n> Firewall: [####_____] 40%",
            "\n> Firewall: [########_] 80%",
            "\n> Firewall: [##########] 100% - BYPASSED",
            "\n> ROOT ACCESS GRANTED",
            "\n> Downloading: ██████████ 100%",
            "\n> Injecting payload...",
            "\n> Establishing backdoor...",
            "\n> CORE SYSTEMS COMPROMISED",
            "\n> Data exfiltration in progress...",
            "\n> Wiping logs...",
            "\n> Trace: 0x7F3A2C1B",
            "\n> Connection: ENCRYPTED [TOR]",
            "\n> IP Spoofing: ACTIVATED",
            "\n> Encryption keys rotated",
            "\n> Ghost protocol: ENGAGED",
            "\n> Mission completed: SUCCESS",
            "\n> All traces erased"
        };

        private int currentStage = 0;

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;

        public Form1()
        {
            InitializeComponent();
            InitializePresetDictionary();
            InitializeResourceFiles();
            InitializeManagers();
            InitializeTrayIcon();
            InitializeProcessMonitor();
            LoadAllResources();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
                processMonitorTimer?.Dispose();
                runningProcess?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeTrayIcon()
        {
            notifyIcon1.Visible = false;
            notifyIcon1.Text = "Launcher Discord";
            notifyIcon1.Icon = this.Icon;
        }

        private void InitializeProcessMonitor()
        {
            processMonitorTimer = new System.Timers.Timer(1000);
            processMonitorTimer.Elapsed += (s, e) => CheckWinwsProcess();
            processMonitorTimer.AutoReset = true;
        }

        private void CheckWinwsProcess()
        {
            if (btnStart.Text == "stop hacking")
            {
                var winwsProcesses = Process.GetProcessesByName("winws");
                if (winwsProcesses.Length == 0)
                {
                    this.Invoke(new Action(() =>
                    {
                        if (btnStart.Text == "stop hacking")
                        {
                            Stopping();
                            checkStatus.ForeColor = Color.Crimson;
                            currentStage = 0;
                            btnStart.Text = "start hacking";
                            tmrStarting.Stop();
                            progressStart.Value = 0;
                            listCode.Text += "\n> HACKING PROCESS TERMINATED UNEXPECTEDLY!";
                        }
                    }));
                }
                else
                {
                    foreach (var process in winwsProcesses)
                    {
                        HideProcessWindow(process);
                    }
                }
            }
        }

        private void HideProcessWindow(Process process)
        {
            try
            {
                if (process == null || process.HasExited) return;

                if (process.MainWindowHandle == IntPtr.Zero)
                {
                    process.WaitForInputIdle(500);
                }

                if (process.MainWindowHandle != IntPtr.Zero)
                {
                    ShowWindow(process.MainWindowHandle, SW_HIDE);
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void InitializePresetDictionary()
        {
            presetBatFiles = new Dictionary<string, string>
            {
                {"general", "general.bat"},
                {"general (ALT)", "general (ALT).bat"},
                {"general (ALT2)", "general (ALT2).bat"},
                {"general (ALT3)", "general (ALT3).bat"},
                {"general (ALT4)", "general (ALT4).bat"},
                {"general (ALT5)", "general (ALT5).bat"},
                {"general (ALT6)", "general (ALT6).bat"},
                {"general (ALT7)", "general (ALT7).bat"},
                {"general (FAKE TLS AUTO ALT)", "general (FAKE TLS AUTO ALT).bat"},
                {"general (FAKE TLS AUTO ALT2)", "general (FAKE TLS AUTO ALT2).bat"},
                {"general (FAKE TLS AUTO)", "general (FAKE TLS AUTO).bat"},
            };
        }

        private void InitializeResourceFiles()
        {
            allResourceFiles = new List<string>
            {
                "general.bat",
                "general (ALT).bat",
                "general (ALT2).bat",
                "general (ALT3).bat",
                "general (ALT4).bat",
                "general (ALT5).bat",
                "general (ALT6).bat",
                "general (ALT7).bat",
                "general (FAKE TLS AUTO ALT).bat",
                "general (FAKE TLS AUTO ALT2).bat",
                "general (FAKE TLS AUTO).bat",

                "winws.exe",
                "list-general.txt",
                "quic_initial_www_google_com.bin",
                "tls_clienthello_www_google_com.bin",
                "ipset-all.txt",
                "service.bat",
                "WinDivert.dll",
                "WinDivert64.sys",
                "cygwin1.dll"
            };
        }

        private void InitializeManagers()
        {
            resourceManager = ResourceManager.Instance;
        }

        private void LoadAllResources()
        {
            try
            {
                resourceManager.ExtractMultipleResources(allResourceFiles);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading resources: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            progressStart.Minimum = 0;
            progressStart.Maximum = 100;
            progressStart.Value = 0;
            UpdateSelectedPresetDisplay();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && IsHackingActive())
            {
                if (!isClosingFromTray)
                {
                    e.Cancel = true;
                    this.Hide();
                    notifyIcon1.Visible = true;
                    return;
                }
            }

            if (isClosingFromTray || e.CloseReason != CloseReason.UserClosing)
            {
                PerformSafeShutdown();
            }

            if (processMonitorTimer != null)
            {
                processMonitorTimer.Stop();
                processMonitorTimer.Dispose();
            }
        }

        private void PerformSafeShutdown()
        {
            try
            {
                Stopping();
                System.Threading.Thread.Sleep(2000);
                CleanupResourcesWithRetry();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Safe shutdown error: {ex.Message}");
            }
        }

        private void CleanupResourcesWithRetry()
        {
            int maxRetries = 5;
            int retryDelay = 1000;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    resourceManager.Cleanup();
                    return;
                }
                catch (UnauthorizedAccessException ex)
                {
                    if (attempt < maxRetries)
                    {
                        System.Threading.Thread.Sleep(retryDelay);
                        ForceKillAllProcesses();
                    }
                    else
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    break;
                }
            }
        }

        private void ForceKillAllProcesses()
        {
            try
            {
                var winwsProcesses = Process.GetProcessesByName("winws");
                foreach (var process in winwsProcesses)
                {
                    try
                    {
                        if (!process.HasExited)
                        {
                            process.Kill();
                            process.WaitForExit(1000);
                        }
                    }
                    catch
                    {
                    }
                }

                var cmdProcesses = Process.GetProcessesByName("cmd");
                foreach (var process in cmdProcesses)
                {
                    try
                    {
                        if (!process.HasExited && process.MainWindowTitle.Contains("general"))
                        {
                            process.Kill();
                            process.WaitForExit(1000);
                        }
                    }
                    catch
                    {
                    }
                }

                System.Threading.Thread.Sleep(500);
            }
            catch
            {
            }
        }

        private bool IsHackingActive()
        {
            return btnStart.Text == "stop hacking" || Process.GetProcessesByName("winws").Length > 0;
        }

        private void UpdateSelectedPresetDisplay()
        {
        }

        private void btnChg_Click(object sender, EventArgs e)
        {
            preset preset = new preset();
            preset.SelectedPreset = selectedPreset;
            if (preset.ShowDialog() == DialogResult.OK)
            {
                selectedPreset = preset.SelectedPreset;
                UpdateSelectedPresetDisplay();
                listCode.Text += $"\n> Preset changed to: {selectedPreset}";
            }
        }

        private void tmrStarting_Tick(object sender, EventArgs e)
        {
            if (currentStage < loadouts.Length)
            {
                listCode.Text += loadouts[currentStage];
                int progressPercentage = (int)((double)(currentStage + 1) / loadouts.Length * 100);
                progressStart.Value = progressPercentage;
                currentStage++;
            }
            else
            {
                tmrStarting.Stop();
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (btnStart.Text == "start hacking")
            {
                if (Starting())
                {
                    listCode.Text = string.Empty;
                    btnStart.Text = "stop hacking";
                    tmrStarting.Start();
                    checkStatus.ForeColor = Color.LawnGreen;
                    processMonitorTimer.Start();
                }
                else
                {
                    checkStatus.ForeColor = Color.Crimson;
                    listCode.Text = string.Empty;
                    listCode.Text += "\n> HACKING FAILED - PRESET NOT FOUND!";
                }
            }
            else
            {
                Stopping();
                checkStatus.ForeColor = Color.Crimson;
                currentStage = 0;
                btnStart.Text = "start hacking";
                tmrStarting.Stop();
                processMonitorTimer.Stop();
                progressStart.Value = 0;
                listCode.Text = string.Empty;
                listCode.Text += "\n> HACKING TERMINATED!!!";
            }
        }

        private bool Starting()
        {
            if (string.IsNullOrEmpty(selectedPreset))
            {
                MessageBox.Show("NO PRESET SELECTED!\nPLEASE SELECT A PRESET FIRST", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!presetBatFiles.ContainsKey(selectedPreset))
            {
                MessageBox.Show($"INVALID PRESET: {selectedPreset}\nPLEASE SELECT A VALID PRESET", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            string batFileName = presetBatFiles[selectedPreset];
            return RunBatFileHidden(batFileName);
        }

        private bool RunBatFileHidden(string batFileName)
        {
            try
            {
                string batFilePath = resourceManager.GetTempFilePath(batFileName);

                if (!resourceManager.FileExistsInTemp(batFileName))
                {
                    listCode.Text += $"\n> ERROR: BAT file not found: {batFileName}";
                    return false;
                }

                listCode.Text += $"\n> Starting preset: {selectedPreset}";
                listCode.Text += $"\n> Executing: {batFileName}";
                listCode.Text += $"\n> MODE: HIDDEN PROCESS";

                Environment.SetEnvironmentVariable("GameFilter", "8080,27015-27030,37015-37030", EnvironmentVariableTarget.Process);
                Environment.SetEnvironmentVariable("LISTS", resourceManager.TempDirectory + "\\", EnvironmentVariableTarget.Process);

                return ExecuteBatFileHidden(batFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error executing BAT file: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private bool ExecuteBatFileHidden(string batFilePath)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C \"{batFilePath}\"",
                    WorkingDirectory = resourceManager.TempDirectory,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                runningProcess = new Process();
                runningProcess.StartInfo = startInfo;
                runningProcess.EnableRaisingEvents = true;

                runningProcess.Exited += (sender, e) =>
                {
                    this.Invoke(new Action(() =>
                    {
                        listCode.Text += $"\n> Command process exited with code: {runningProcess.ExitCode}";
                    }));
                };

                runningProcess.Start();

                listCode.Text += $"\n> Process started HIDDEN";
                listCode.Text += $"\n> PID: {runningProcess.Id}";
                listCode.Text += $"\n> WinWS will run in background (hidden)";

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error executing BAT file: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void Stopping()
        {
            try
            {
                listCode.Text += "\n> Terminating processes...";

                var winwsProcesses = Process.GetProcessesByName("winws");
                foreach (var process in winwsProcesses)
                {
                    try
                    {
                        if (!process.HasExited)
                        {
                            process.Kill();
                            process.WaitForExit(3000);
                        }
                    }
                    catch (Exception ex)
                    {
                        listCode.Text += $"\n> Error killing winws process: {ex.Message}";
                    }
                }

                if (runningProcess != null && !runningProcess.HasExited)
                {
                    try
                    {
                        runningProcess.Kill();
                        runningProcess.WaitForExit(2000);
                    }
                    catch (Exception ex)
                    {
                        listCode.Text += $"\n> Error killing command process: {ex.Message}";
                    }
                    finally
                    {
                        runningProcess.Dispose();
                        runningProcess = null;
                    }
                }

                System.Threading.Thread.Sleep(1000);
                listCode.Text += "\n> Cleanup completed";
            }
            catch (Exception ex)
            {
                listCode.Text += $"\n> Cleanup error: {ex.Message}";
            }
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        private void contextMenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem == showToolStripMenuItem)
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                notifyIcon1.Visible = false;
            }
            else if (e.ClickedItem == exitToolStripMenuItem)
            {
                isClosingFromTray = true;
                this.Close();
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                notifyIcon1.Visible = true;
            }
        }
    }
}