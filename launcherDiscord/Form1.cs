using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
                            btnChg.Enabled = true;
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
            }
            catch (Exception ex)
            {
                // Игнорируем ошибки скрытия
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
                "service.bat",
                
                // Файлы в папке lists
                "ipset-all.txt",
                "ipset-all.txt.backup",
                "ipset-exclude.txt",
                "list-exclude.txt",
                "list-general.txt",
                "list-google.txt",
                
                // Файлы в папке bin
                "winws.exe",
                "quic_initial_www_google_com.bin",
                "tls_clienthello_4pda_to.bin",
                "tls_clienthello_www_google_com.bin",
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
                Console.WriteLine("Loading all resources from embedded resources...");

                resourceManager.ExtractMultipleResources(allResourceFiles);

                // Проверяем целостность файлов после загрузки
                if (!resourceManager.VerifyAllFiles())
                {
                    MessageBox.Show("Some required files are missing from resources!\nThe application may not work properly.",
                        "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                // Показываем список созданных файлов
                var createdFiles = resourceManager.GetAllCreatedFiles();
                listCode.Items.Add($"> Loaded {createdFiles.Count} files from resources");
                listCode.Items.Add($"> Temp directory: {resourceManager.TempDirectory}");

                // Проверяем конкретно BAT файлы
                CheckBatFiles();

                listCode.TopIndex = listCode.Items.Count - 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading resources: {ex.Message}");
                MessageBox.Show($"Error loading resources: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CheckBatFiles()
        {
            listCode.Items.Add("> Checking BAT files...");
            foreach (var batFile in presetBatFiles.Values)
            {
                string batPath = resourceManager.GetTempFilePath(batFile);
                bool exists = File.Exists(batPath);
                long fileSize = exists ? new FileInfo(batPath).Length : 0;
                listCode.Items.Add($"> {batFile}: {(exists ? $"EXISTS ({fileSize} bytes)" : "MISSING")}");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            progressStart.Minimum = 0;
            progressStart.Maximum = 100;
            progressStart.Value = 0;
            

            // Устанавливаем начальный статус
            checkStatus.ForeColor = Color.Crimson;
            listCode.Items.Add("> System ready - Select preset and start hacking");
            listCode.TopIndex = listCode.Items.Count - 1;
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
                    notifyIcon1.ShowBalloonTip(3000, "Launcher Discord",
                        "Application minimized to tray. Hacking processes are still running.",
                        ToolTipIcon.Info);
                    return;
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

                // Очищаем ресурсы
                resourceManager.Cleanup();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Safe shutdown error: {ex.Message}");
            }
        }

        private bool IsHackingActive()
        {
            return btnStart.Text == "stop hacking" || Process.GetProcessesByName("winws").Length > 0;
        }

        

        private void btnChg_Click(object sender, EventArgs e)
        {
            if (IsHackingActive())
            {
                MessageBox.Show("Cannot change preset while hacking is active!\nStop hacking first.",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            preset presetForm = new preset();
            
            if (presetForm.ShowDialog() == DialogResult.OK)
            {
                

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
                listCode.Items.Add("> HACKING MODE: ACTIVE");
                listCode.Items.Add("> All processes running in background");
                listCode.TopIndex = listCode.Items.Count - 1;
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

                    // Блокируем кнопку смены пресета во время работы
                    btnChg.Enabled = false;
                }
                else
                {
                    checkStatus.ForeColor = Color.Crimson;
                    listCode.Items.Clear();
                    listCode.Items.Add("> HACKING FAILED - CHECK CONFIGURATION!");
                    listCode.TopIndex = listCode.Items.Count - 1;
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
                listCode.Items.Add("> All processes stopped");
                listCode.TopIndex = listCode.Items.Count - 1;

                // Разблокируем кнопку смены пресета
                btnChg.Enabled = true;
            }
        }

        private bool Starting()
        {
            listCode.Items.Add($"> Starting hacking process...");
            listCode.Items.Add($"> Selected preset: '{selectedPreset}'");

            if (string.IsNullOrEmpty(selectedPreset))
            {
                MessageBox.Show("NO PRESET SELECTED!\nPLEASE SELECT A PRESET FIRST", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!presetBatFiles.ContainsKey(selectedPreset))
            {
                listCode.Items.Add($"> ERROR: Invalid preset: {selectedPreset}");
                MessageBox.Show($"INVALID PRESET: {selectedPreset}\nPLEASE SELECT A VALID PRESET", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            string batFileName = presetBatFiles[selectedPreset];
            listCode.Items.Add($"> BAT file: '{batFileName}'");

            // Проверяем наличие всех необходимых файлов
            if (!resourceManager.VerifyAllFiles())
            {
                MessageBox.Show("Some required files are missing from resources!\nCannot start hacking process.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Дополнительная проверка BAT файла
            string batFilePath = resourceManager.GetTempFilePath(batFileName);
            if (!File.Exists(batFilePath))
            {
                listCode.Items.Add($"> ERROR: BAT file not found: {batFilePath}");
                return false;
            }

            // Проверяем размер BAT файла
            FileInfo batFileInfo = new FileInfo(batFilePath);
            listCode.Items.Add($"> BAT file size: {batFileInfo.Length} bytes");

            if (batFileInfo.Length == 0)
            {
                listCode.Items.Add($"> ERROR: BAT file is empty: {batFileName}");
                return false;
            }

            return RunBatFileHidden(batFileName);
        }

        private bool RunBatFileHidden(string batFileName)
        {
            try
            {
                string batFilePath = resourceManager.GetTempFilePath(batFileName);

                listCode.Items.Add($"> Starting preset: {selectedPreset}");
                listCode.Items.Add($"> Executing: {batFileName}");
                listCode.Items.Add($"> Full path: {batFilePath}");
                listCode.Items.Add($"> Temp directory: {resourceManager.TempDirectory}");
                listCode.Items.Add($"> MODE: HIDDEN PROCESS");

                // Устанавливаем переменные окружения
                Environment.SetEnvironmentVariable("GameFilter", "8080,27015-27030,37015-37030", EnvironmentVariableTarget.Process);
                Environment.SetEnvironmentVariable("LISTS", Path.Combine(resourceManager.TempDirectory, "lists"), EnvironmentVariableTarget.Process);
                Environment.SetEnvironmentVariable("BIN", Path.Combine(resourceManager.TempDirectory, "bin"), EnvironmentVariableTarget.Process);

                return ExecuteBatFileHidden(batFilePath);
            }
            catch (Exception ex)
            {
                listCode.Items.Add($"> ERROR executing BAT file: {ex.Message}");
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

                // Обработка вывода
                runningProcess.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        this.Invoke(new Action(() =>
                        {
                            listCode.Items.Add($"> [OUTPUT] {e.Data}");
                            listCode.TopIndex = listCode.Items.Count - 1;
                        }));
                    }
                };

                runningProcess.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        this.Invoke(new Action(() =>
                        {
                            listCode.Items.Add($"> [ERROR] {e.Data}");
                            listCode.TopIndex = listCode.Items.Count - 1;
                        }));
                    }
                };

                runningProcess.Exited += (sender, e) =>
                {
                    this.Invoke(new Action(() =>
                    {
                        listCode.Items.Add($"> Command process exited with code: {runningProcess.ExitCode}");
                        if (btnStart.Text == "stop hacking")
                        {
                            listCode.Items.Add($"> Process terminated unexpectedly");
                            Stopping();
                            btnStart.Text = "start hacking";
                            checkStatus.ForeColor = Color.Crimson;
                            btnChg.Enabled = true;
                        }
                    }));
                };

                bool started = runningProcess.Start();
                listCode.Items.Add($"> Process start result: {started}");

                if (started)
                {
                    runningProcess.BeginOutputReadLine();
                    runningProcess.BeginErrorReadLine();

                    listCode.Items.Add($"> Process started HIDDEN");
                    listCode.Items.Add($"> PID: {runningProcess.Id}");
                    listCode.Items.Add($"> Working directory: {resourceManager.TempDirectory}");
                    listCode.Items.Add($"> WinWS will run in background (hidden)");

                    // Немедленно начинаем скрывать связанные процессы
                    Task.Run(async () =>
                    {
                        await Task.Delay(2000);
                        HideAllWinwsWindows();
                    });

                    listCode.TopIndex = listCode.Items.Count - 1;
                    return true;
                }
                else
                {
                    listCode.Items.Add($"> ERROR: Failed to start process");
                    return false;
                }
            }
            catch (Exception ex)
            {
                listCode.Items.Add($"> ERROR in ExecuteBatFileHidden: {ex.Message}");
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
                listCode.Items.Add($"> Found {processes.Length} winws processes to hide");

                foreach (var process in processes)
                {
                    HideProcessWindow(process);
                }

                // Также скрываем возможные cmd окна
                var cmdProcesses = Process.GetProcessesByName("cmd");
                foreach (var process in cmdProcesses)
                {
                    if (process.MainWindowTitle.Contains("general"))
                    {
                        HideProcessWindow(process);
                    }
                }
            }
            catch (Exception ex)
            {
                listCode.Items.Add($"> Error hiding windows: {ex.Message}");
            }
        }

        private void Stopping()
        {
            try
            {
                listCode.Items.Add("> Terminating processes...");

                // Сначала завершаем процессы winws.exe
                var winwsProcesses = Process.GetProcessesByName("winws");
                listCode.Items.Add($"> Found {winwsProcesses.Length} winws processes to terminate");

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
                listCode.TopIndex = listCode.Items.Count - 1;
            }
            catch (Exception ex)
            {
                listCode.Items.Add($"> Cleanup error: {ex.Message}");
                listCode.TopIndex = listCode.Items.Count - 1;
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
                notifyIcon1.ShowBalloonTip(1000, "Launcher Discord",
                    "Application minimized to tray", ToolTipIcon.Info);
            }
        }

        private void listCode_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Автопрокрутка вниз при добавлении новых элементов
            listCode.TopIndex = listCode.Items.Count - 1;
        }
    }
}