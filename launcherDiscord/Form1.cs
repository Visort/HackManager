using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        private readonly string[] loadouts = new string[20] {
            "> INITIATING SYSTEM BYPASS...",
            "> ACCESSING MAINFRAME...",
            "> ENCRYPTION PROTOCOL: AES-256",
            "> Firewall: [####_____] 40%", "> Firewall: [########_] 80%",
            "> Firewall: [##########] 100% - BYPASSED",
            "> ROOT ACCESS GRANTED", "> Downloading: ██████████ 100%",
            "> Injecting payload...",
            "> Establishing backdoor...",
            "> CORE SYSTEMS COMPROMISED",
            "> Data exfiltration in progress...",
            "> Wiping logs...", "> Trace: 0x7F3A2C1B",
            "> Connection: ENCRYPTED [TOR]",
            "> IP Spoofing: ACTIVATED",
            "> Encryption keys rotated",
            "> Ghost protocol: ENGAGED",
            "> Mission completed: SUCCESS",
            "> All traces erased"
        };

        private int currentStage = 0;

        // Windows API для скрытия окон
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

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
                            listCode.Items.Add("> HACKING PROCESS TERMINATED UNEXPECTEDLY!");
                            listCode.TopIndex = listCode.Items.Count - 1;
                        }
                    }));
                }
                else
                {
                    // Скрываем все окна winws процессов
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

                // Ждем немного для инициализации окна
                if (process.MainWindowHandle == IntPtr.Zero)
                {
                    process.WaitForInputIdle(500);
                }

                // Скрываем главное окно
                if (process.MainWindowHandle != IntPtr.Zero)
                {
                    ShowWindow(process.MainWindowHandle, SW_HIDE);
                }

                // Скрываем все дочерние окна
                HideChildProcessWindows(process.Id);
            }
            catch (Exception ex)
            {
                // Игнорируем ошибки скрытия
            }
        }

        private void HideChildProcessWindows(int parentProcessId)
        {
            try
            {
                // Получаем все процессы с тем же именем
                var allProcesses = Process.GetProcesses();
                foreach (var proc in allProcesses)
                {
                    try
                    {
                        if (proc.Id != parentProcessId && proc.ProcessName.ToLower().Contains("winws"))
                        {
                            if (proc.MainWindowHandle != IntPtr.Zero)
                            {
                                ShowWindow(proc.MainWindowHandle, SW_HIDE);
                            }
                        }
                    }
                    catch
                    {
                        // Пропускаем процессы, к которым нет доступа
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки
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
                {"general (FAKE TLS AUTO ALT3)", "general (FAKE TLS AUTO ALT3).bat"},
                {"general (FAKE TLS AUTO)", "general (FAKE TLS AUTO).bat"},
                {"general (SIMPLE FAKE ALT)", "general (SIMPLE FAKE ALT).bat"},
                {"general (SIMPLE FAKE)", "general (SIMPLE FAKE).bat"}
            };
        }

        private void InitializeResourceFiles()
        {
            allResourceFiles = new List<string>
            {
                // BAT файлы
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
                "general (FAKE TLS AUTO ALT3).bat",
                "general (FAKE TLS AUTO).bat",
                "general (SIMPLE FAKE ALT).bat",
                "general (SIMPLE FAKE).bat",
                
                // Все зависимости
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
                Console.WriteLine("Loading all resources...");
                resourceManager.ExtractMultipleResources(allResourceFiles);
                Console.WriteLine("All resources loaded successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading resources: {ex.Message}");
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
                    return; // Не завершаем приложение, просто скрываем
                }
            }

            // При реальном закрытии останавливаем процессы и очищаем ресурсы
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
                // Останавливаем все процессы
                Stopping();

                // Даем время процессам завершиться
                System.Threading.Thread.Sleep(2000);

                // Очищаем ресурсы с повторными попытками
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
            int retryDelay = 1000; // 1 секунда

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    resourceManager.Cleanup();
                    Console.WriteLine($"Cleanup successful on attempt {attempt}");
                    return;
                }
                catch (UnauthorizedAccessException ex)
                {
                    Console.WriteLine($"Cleanup attempt {attempt} failed: {ex.Message}");

                    if (attempt < maxRetries)
                    {
                        Console.WriteLine($"Waiting {retryDelay}ms before retry...");
                        System.Threading.Thread.Sleep(retryDelay);

                        // Дополнительная попытка завершить процессы
                        ForceKillAllProcesses();
                    }
                    else
                    {
                        Console.WriteLine("Max retries reached, skipping cleanup");
                        // На последней попытке просто пропускаем ошибку
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected cleanup error on attempt {attempt}: {ex.Message}");
                    break;
                }
            }
        }

        private void ForceKillAllProcesses()
        {
            try
            {
                // Завершаем все winws процессы
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
                        // Игнорируем ошибки при завершении
                    }
                }

                // Завершаем cmd процессы, которые могут использовать наши файлы
                var cmdProcesses = Process.GetProcessesByName("cmd");
                foreach (var process in cmdProcesses)
                {
                    try
                    {
                        // Проверяем, относится ли процесс к нашему приложению
                        if (!process.HasExited && process.MainWindowTitle.Contains("general"))
                        {
                            process.Kill();
                            process.WaitForExit(1000);
                        }
                    }
                    catch
                    {
                        // Игнорируем ошибки
                    }
                }

                System.Threading.Thread.Sleep(500);
            }
            catch
            {
                // Игнорируем общие ошибки
            }
        }

        private bool IsHackingActive()
        {
            return btnStart.Text == "stop hacking" || Process.GetProcessesByName("winws").Length > 0;
        }

        private void UpdateSelectedPresetDisplay()
        {
            // Ваш код обновления отображения выбранного пресета
        }

        private void btnChg_Click(object sender, EventArgs e)
        {
            preset preset = new preset();
            preset.SelectedPreset = selectedPreset;
            if (preset.ShowDialog() == DialogResult.OK)
            {
                selectedPreset = preset.SelectedPreset;
                UpdateSelectedPresetDisplay();

                listCode.Items.Add($"> Preset changed to: {selectedPreset}");
                listCode.TopIndex = listCode.Items.Count - 1;
            }
        }

        private void tmrStarting_Tick(object sender, EventArgs e)
        {
            if (currentStage < loadouts.Length)
            {
                listCode.Items.Add(loadouts[currentStage]);
                listCode.TopIndex = listCode.Items.Count - 1;

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
                    listCode.Items.Clear();
                    btnStart.Text = "stop hacking";
                    tmrStarting.Start();
                    checkStatus.ForeColor = Color.LawnGreen;
                    processMonitorTimer.Start();
                }
                else
                {
                    checkStatus.ForeColor = Color.Crimson;
                    listCode.Items.Clear();
                    listCode.Items.Add("> HACKING FAILED - PRESET NOT FOUND!");
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
                listCode.Items.Clear();
                listCode.Items.Add("> HACKING TERMINATED!!!");
            }
        }

        private bool Starting()
        {
            listCode.Items.Add($"> Debug: selectedPreset = '{selectedPreset}'");

            if (string.IsNullOrEmpty(selectedPreset))
            {
                MessageBox.Show("NO PRESET SELECTED!\nPLEASE SELECT A PRESET FIRST", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!presetBatFiles.ContainsKey(selectedPreset))
            {
                listCode.Items.Add($"> Debug: Available presets: {string.Join(", ", presetBatFiles.Keys)}");
                MessageBox.Show($"INVALID PRESET: {selectedPreset}\nPLEASE SELECT A VALID PRESET", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            string batFileName = presetBatFiles[selectedPreset];
            listCode.Items.Add($"> Debug: batFileName = '{batFileName}'");

            return RunBatFileHidden(batFileName);
        }

        private bool RunBatFileHidden(string batFileName)
        {
            try
            {
                string batFilePath = resourceManager.GetTempFilePath(batFileName);

                if (!resourceManager.FileExistsInTemp(batFileName))
                {
                    listCode.Items.Add($"> ERROR: BAT file not found: {batFileName}");
                    listCode.Items.Add($"> Available files in temp: {string.Join(", ", Directory.GetFiles(resourceManager.TempDirectory).Select(Path.GetFileName))}");
                    return false;
                }

                listCode.Items.Add($"> Starting preset: {selectedPreset}");
                listCode.Items.Add($"> Executing: {batFileName}");
                listCode.Items.Add($"> Temp directory: {resourceManager.TempDirectory}");
                listCode.Items.Add($"> MODE: HIDDEN PROCESS");

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
                        listCode.Items.Add($"> Command process exited with code: {runningProcess.ExitCode}");
                    }));
                };

                runningProcess.Start();

                // Немедленно начинаем скрывать связанные процессы
                Task.Run(async () =>
                {
                    await Task.Delay(2000); // Ждем запуска winws
                    HideAllWinwsWindows();
                });

                listCode.Items.Add($"> Process started HIDDEN");
                listCode.Items.Add($"> PID: {runningProcess.Id}");
                listCode.Items.Add($"> Working directory: {resourceManager.TempDirectory}");
                listCode.Items.Add($"> WinWS will run in background (hidden)");

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error executing BAT file: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void HideAllWinwsWindows()
        {
            try
            {
                var processes = Process.GetProcessesByName("winws");
                foreach (var process in processes)
                {
                    HideProcessWindow(process);
                }
            }
            catch
            {
                // Игнорируем ошибки
            }
        }

        private void Stopping()
        {
            try
            {
                listCode.Items.Add("> Terminating processes...");

                // Сначала завершаем процессы winws.exe
                var winwsProcesses = Process.GetProcessesByName("winws");
                foreach (var process in winwsProcesses)
                {
                    try
                    {
                        if (!process.HasExited)
                        {
                            process.Kill();
                            process.WaitForExit(3000);
                            listCode.Items.Add($"> Killed process: winws.exe (PID: {process.Id})");
                        }
                    }
                    catch (Exception ex)
                    {
                        listCode.Items.Add($"> Error killing winws process: {ex.Message}");
                    }
                }

                // Затем завершаем основной процесс cmd если он еще запущен
                if (runningProcess != null && !runningProcess.HasExited)
                {
                    try
                    {
                        runningProcess.Kill();
                        runningProcess.WaitForExit(2000);
                        listCode.Items.Add($"> Killed command process (PID: {runningProcess.Id})");
                    }
                    catch (Exception ex)
                    {
                        listCode.Items.Add($"> Error killing command process: {ex.Message}");
                    }
                    finally
                    {
                        runningProcess.Dispose();
                        runningProcess = null;
                    }
                }

                // Даем время системе освободить ресурсы
                System.Threading.Thread.Sleep(1000);

                listCode.Items.Add("> Cleanup completed");
            }
            catch (Exception ex)
            {
                listCode.Items.Add($"> Cleanup error: {ex.Message}");
            }
        }

        private void Form1_Leave(object sender, EventArgs e)
        {
            // Не останавливаем автоматически при потере фокуса
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

        private void listCode_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Ваш код
        }
    }
}