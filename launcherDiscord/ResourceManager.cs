using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace launcherDiscord
{
    public class ResourceManager
    {
        private static readonly Lazy<ResourceManager> _instance = new Lazy<ResourceManager>(() => new ResourceManager());
        public static ResourceManager Instance => _instance.Value;

        private readonly string _tempDirectory;
        private readonly Dictionary<string, byte[]> _resourceCache;

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_HIDE = 0;

        public string TempDirectory => _tempDirectory;

        private ResourceManager()
        {
            _tempDirectory = Path.Combine(Path.GetTempPath(), "HackingManage");
            _resourceCache = new Dictionary<string, byte[]>();
            InitializeTempDirectory();
        }

        private void InitializeTempDirectory()
        {
            try
            {
                if (!Directory.Exists(_tempDirectory))
                {
                    Directory.CreateDirectory(_tempDirectory);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing temp directory: {ex.Message}");
                throw;
            }
        }

        public Process StartHiddenProcess(string fileName, string arguments = "", string workingDirectory = null)
        {
            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    WorkingDirectory = workingDirectory ?? _tempDirectory,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                var process = new Process
                {
                    StartInfo = processStartInfo,
                    EnableRaisingEvents = true
                };

                process.Start();
                HideProcessWindow(process);

                return process;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting hidden process {fileName}: {ex.Message}");
                return null;
            }
        }

        public void HideProcessWindow(Process process)
        {
            try
            {
                if (process == null || process.HasExited) return;

                process.WaitForInputIdle(1000);

                if (process.MainWindowHandle != IntPtr.Zero)
                {
                    ShowWindow(process.MainWindowHandle, SW_HIDE);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error hiding process window: {ex.Message}");
            }
        }

        public Process StartWinwsHidden(string arguments = "")
        {
            string winwsPath = GetTempFilePath("winws.exe");

            if (!FileExistsInTemp("winws.exe"))
            {
                Console.WriteLine("winws.exe not found in temp directory");
                return null;
            }

            return StartHiddenProcess(winwsPath, arguments);
        }

        private byte[] CreateHiddenWinwsDemo()
        {
            string batContent = @"@echo off
                                  echo WinWS Hidden Process Simulation > winws_log.txt
                                  for /l %%x in (1, 1, 10) do (
                                  echo Iteration %%x >> winws_log.txt
                                  timeout /t 1 /nobreak >nul
                                  )
                                  echo WinWS completed successfully >> winws_log.txt
                                  exit";

            return Encoding.UTF8.GetBytes(batContent);
        }

        public bool ExtractResourceToTemp(string fileName)
        {
            try
            {
                string tempPath = Path.Combine(_tempDirectory, fileName);
                if (File.Exists(tempPath))
                {
                    return true;
                }

                byte[] fileBytes = GetFileFromResources(fileName);
                if (fileBytes != null && fileBytes.Length > 0)
                {
                    File.WriteAllBytes(tempPath, fileBytes);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting {fileName}: {ex.Message}");
                return false;
            }
        }

        public void ExtractMultipleResources(IEnumerable<string> fileNames)
        {
            foreach (string fileName in fileNames)
            {
                ExtractResourceToTemp(fileName);
            }
        }

        public string GetTempFilePath(string fileName)
        {
            return Path.Combine(_tempDirectory, fileName);
        }

        public bool FileExistsInTemp(string fileName)
        {
            return File.Exists(GetTempFilePath(fileName));
        }

        private byte[] GetFileFromResources(string fileName)
        {
            try
            {
                if (_resourceCache.ContainsKey(fileName))
                {
                    return _resourceCache[fileName];
                }

                byte[] fileBytes = GetFileFromEmbeddedResources(fileName);

                if (fileName.Equals("winws.exe", StringComparison.OrdinalIgnoreCase) &&
                    (fileBytes == null || fileBytes.Length == 0))
                {
                    fileBytes = CreateHiddenWinwsDemo();
                }
                else if (fileBytes == null || fileBytes.Length == 0)
                {
                    fileBytes = CreateDemoFileContent(fileName);
                }

                _resourceCache[fileName] = fileBytes;
                return fileBytes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading resource {fileName}: {ex.Message}");
                return CreateDemoFileContent(fileName);
            }
        }

        private byte[] GetFileFromEmbeddedResources(string fileName)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                string[] resourceNames = assembly.GetManifestResourceNames();

                string fullResourceName = resourceNames.FirstOrDefault(name =>
                    name.EndsWith(fileName, StringComparison.OrdinalIgnoreCase) ||
                    name.EndsWith("." + fileName, StringComparison.OrdinalIgnoreCase));

                if (fullResourceName != null)
                {
                    using (Stream stream = assembly.GetManifestResourceStream(fullResourceName))
                    {
                        if (stream != null)
                        {
                            byte[] buffer = new byte[stream.Length];
                            stream.Read(buffer, 0, buffer.Length);
                            return buffer;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading embedded resource {fileName}: {ex.Message}");
            }

            return null;
        }

        private byte[] CreateDemoFileContent(string fileName)
        {
            if (fileName.EndsWith(".bat"))
            {
                string demoContent = $@"@echo off
                                        echo Executing preset: {fileName}
                                        echo Hacking simulation in progress...
                                        timeout /t 3 /nobreak >nul
                                        echo Operation completed successfully
                                        pause";
                return Encoding.UTF8.GetBytes(demoContent);
            }
            else if (fileName.EndsWith(".txt"))
            {
                string demoContent = $"# Demo {fileName}\n# Generated for testing purposes";
                return Encoding.UTF8.GetBytes(demoContent);
            }
            else
            {
                return new byte[0];
            }
        }

        public void Cleanup()
        {
            try
            {
                _resourceCache.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cleanup error: {ex.Message}");
            }
        }
    }
}