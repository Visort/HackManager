using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.ComponentModel;

namespace launcherDiscord
{
    public class ResourceManager
    {
        private static readonly Lazy<ResourceManager> _instance = new Lazy<ResourceManager>(() => new ResourceManager());
        public static ResourceManager Instance => _instance.Value;

        private readonly string _tempDirectory;
        private readonly Dictionary<string, byte[]> _resourceCache;

        // Импорт Windows API для скрытия окна
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;

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
                // Создаем директорию, если не существует
                if (!Directory.Exists(_tempDirectory))
                {
                    Directory.CreateDirectory(_tempDirectory);
                    Console.WriteLine($"Temp directory created: {_tempDirectory}");
                }
                else
                {
                    Console.WriteLine($"Temp directory already exists: {_tempDirectory}");
                    // Не удаляем существующую директорию, чтобы избежать ошибок доступа
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing temp directory: {ex.Message}");
                throw;
            }
        }

        // Метод для запуска процесса в скрытом режиме
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
                Console.WriteLine($"Started hidden process: {fileName} {arguments}");

                // Дополнительно скрываем окно, если оно появилось
                HideProcessWindow(process);

                return process;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting hidden process {fileName}: {ex.Message}");
                return null;
            }
        }

        // Метод для скрытия окна процесса
        public void HideProcessWindow(Process process)
        {
            try
            {
                if (process == null || process.HasExited) return;

                // Ждем немного, чтобы процесс успел создать окно
                process.WaitForInputIdle(1000);

                // Пытаемся скрыть главное окно процесса
                if (process.MainWindowHandle != IntPtr.Zero)
                {
                    ShowWindow(process.MainWindowHandle, SW_HIDE);
                    Console.WriteLine($"Hidden window for process: {process.ProcessName}");
                }

                // Также скрываем все дочерние окна
                HideChildWindows(process);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error hiding process window: {ex.Message}");
            }
        }

        // Метод для скрытия дочерних окон
        private void HideChildWindows(Process process)
        {
            try
            {
                // Этот метод можно расширить для скрытия всех окон процесса
                // В простейшем случае просто скрываем главное окно
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error hiding child windows: {ex.Message}");
            }
        }

        // Запуск winws в скрытом режиме
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

        // Метод для создания демо-версии winws, которая работает в фоне
        private byte[] CreateHiddenWinwsDemo()
        {
            string demoContent = @"using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

class Program
{
    [DllImport(""user32.dll"")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport(""kernel32.dll"")]
    static extern IntPtr GetConsoleWindow();

    const int SW_HIDE = 0;
    const int SW_SHOW = 5;

    static void Main(string[] args)
    {
        // Скрываем консоль сразу при запуске
        var handle = GetConsoleWindow();
        ShowWindow(handle, SW_HIDE);

        // Имитация работы в фоне
        for (int i = 0; i < 10; i++)
        {
            // Логируем в файл вместо консоли
            System.IO.File.AppendAllText(""winws_log.txt"", $""WinWS working... iteration {i}\n"");
            Thread.Sleep(1000);
        }

        System.IO.File.AppendAllText(""winws_log.txt"", ""WinWS completed successfully\n"");
    }
}";

            // Компилируем C# код в exe
            return CompileCSharpCode(demoContent, "winws.exe");
        }

        private byte[] CompileCSharpCode(string code, string outputName)
        {
            try
            {
                // В реальной реализации здесь должна быть компиляция кода
                // Для демо версии создаем простой .bat файл, который работает скрыто
                string batContent = @"@echo off
chcp 65001 > nul
echo WinWS Hidden Process Simulation > winws_log.txt
for /l %%x in (1, 1, 10) do (
    echo Iteration %%x >> winws_log.txt
    timeout /t 1 /nobreak >nul
)
echo WinWS completed successfully >> winws_log.txt
exit";

                return Encoding.UTF8.GetBytes(batContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Compilation error: {ex.Message}");
                return CreateSimpleHiddenBatch();
            }
        }

        private byte[] CreateSimpleHiddenBatch()
        {
            string content = @"@echo off
chcp 65001 > nul
echo Hidden process simulation > hidden_log.txt
ping 127.0.0.1 -n 10 > nul
echo Process completed >> hidden_log.txt
exit";
            return Encoding.UTF8.GetBytes(content);
        }

        public bool ExtractResourceToTemp(string fileName)
        {
            try
            {
                // Проверяем, существует ли файл уже в temp директории
                string tempPath = Path.Combine(_tempDirectory, fileName);
                if (File.Exists(tempPath))
                {
                    Console.WriteLine($"File already exists in temp: {fileName}");
                    return true;
                }

                byte[] fileBytes = GetFileFromResources(fileName);
                if (fileBytes != null && fileBytes.Length > 0)
                {
                    File.WriteAllBytes(tempPath, fileBytes);
                    Console.WriteLine($"Extracted: {fileName} -> {tempPath} ({fileBytes.Length} bytes)");
                    return true;
                }
                else
                {
                    Console.WriteLine($"Resource not found or empty: {fileName}");
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

                var resourceManager = Properties.Resources.ResourceManager;
                string resourceName = Path.GetFileNameWithoutExtension(fileName);

                byte[] fileBytes = null;

                try
                {
                    object resourceObject = resourceManager.GetObject(resourceName);
                    if (resourceObject is byte[] bytes)
                    {
                        fileBytes = bytes;
                        Console.WriteLine($"Loaded {fileName} as byte[] ({fileBytes.Length} bytes)");
                    }
                    else if (resourceObject is string stringContent)
                    {
                        if (fileName.EndsWith(".bat") || fileName.EndsWith(".txt"))
                        {
                            fileBytes = Encoding.UTF8.GetBytes(stringContent);
                            Console.WriteLine($"Loaded {fileName} as string ({fileBytes.Length} bytes)");
                        }
                        else
                        {
                            try
                            {
                                fileBytes = Convert.FromBase64String(stringContent);
                                Console.WriteLine($"Loaded {fileName} from Base64 string ({fileBytes.Length} bytes)");
                            }
                            catch (FormatException)
                            {
                                fileBytes = Encoding.UTF8.GetBytes(stringContent);
                                Console.WriteLine($"Loaded {fileName} as UTF8 string ({fileBytes.Length} bytes)");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error getting resource object for {fileName}: {ex.Message}");
                }

                if (fileBytes == null || fileBytes.Length == 0)
                {
                    fileBytes = GetFileFromEmbeddedResources(fileName);
                    if (fileBytes != null)
                    {
                        Console.WriteLine($"Loaded {fileName} from embedded resources ({fileBytes.Length} bytes)");
                    }
                }

                // Особый случай для winws.exe - создаем демо, которое работает скрыто
                if (fileName.Equals("winws.exe", StringComparison.OrdinalIgnoreCase) &&
                    (fileBytes == null || fileBytes.Length == 0))
                {
                    Console.WriteLine($"Creating hidden demo version for: {fileName}");
                    fileBytes = CreateHiddenWinwsDemo();
                }
                else if (fileBytes == null || fileBytes.Length == 0)
                {
                    Console.WriteLine($"Creating demo content for: {fileName}");
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
                    name.EndsWith("." + fileName, StringComparison.OrdinalIgnoreCase) ||
                    name.Contains(fileName) ||
                    name.EndsWith(Path.GetFileNameWithoutExtension(fileName), StringComparison.OrdinalIgnoreCase));

                if (fullResourceName != null)
                {
                    Console.WriteLine($"Found embedded resource: {fullResourceName}");
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
                else
                {
                    Console.WriteLine($"Embedded resource not found: {fileName}");
                    Console.WriteLine("Available resources:");
                    foreach (var name in resourceNames)
                    {
                        Console.WriteLine($"  - {name}");
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
chcp 65001 > nul
echo Executing preset: {fileName}
echo Temp directory: {_tempDirectory}
echo Files available:
dir ""{_tempDirectory}""
echo Hacking simulation in progress...
timeout /t 3 /nobreak >nul
echo Operation completed successfully
pause";
                return Encoding.UTF8.GetBytes(demoContent);
            }
            else if (fileName.EndsWith(".txt"))
            {
                string demoContent = $"# Demo {fileName}\n# Generated for testing purposes\n127.0.0.1\n8.8.8.8";
                return Encoding.UTF8.GetBytes(demoContent);
            }
            else if (fileName.EndsWith(".exe") || fileName.EndsWith(".dll") || fileName.EndsWith(".sys"))
            {
                byte[] peHeader = new byte[1024];
                Encoding.UTF8.GetBytes("This is a demo " + fileName).CopyTo(peHeader, 0);
                Console.WriteLine($"Created demo binary file: {fileName} ({peHeader.Length} bytes)");
                return peHeader;
            }
            else if (fileName.EndsWith(".bin"))
            {
                byte[] binContent = new byte[512];
                Encoding.UTF8.GetBytes("Demo binary content for " + fileName).CopyTo(binContent, 0);
                Console.WriteLine($"Created demo bin file: {fileName} ({binContent.Length} bytes)");
                return binContent;
            }
            else
            {
                Console.WriteLine($"Unknown file type: {fileName}, creating empty file");
                return new byte[0];
            }
        }

        public void Cleanup()
        {
            try
            {
                // Очищаем кэш, но не удаляем файлы из временной директории
                _resourceCache.Clear();
                Console.WriteLine("Resource cache cleared");

                // Не удаляем временную директорию, чтобы избежать ошибок доступа
                // Directory.Delete(_tempDirectory, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cleanup error: {ex.Message}");
            }
        }
    }
}