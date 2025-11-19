using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;

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
            CreateFolderStructure();
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
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing temp directory: {ex.Message}");
                throw;
            }
        }

        // Создание структуры папок
        private void CreateFolderStructure()
        {
            try
            {
                // Создаем папки
                string listsDir = Path.Combine(_tempDirectory, "lists");
                string binDir = Path.Combine(_tempDirectory, "bin");

                if (!Directory.Exists(listsDir))
                    Directory.CreateDirectory(listsDir);
                if (!Directory.Exists(binDir))
                    Directory.CreateDirectory(binDir);

                Console.WriteLine("Created folder structure: lists/, bin/");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating folder structure: {ex.Message}");
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
                // В простейшем случае просто пытаемся найти и скрыть любые окна процесса
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error hiding child windows: {ex.Message}");
            }
        }

        // Запуск winws в скрытом режиме
        public Process StartWinwsHidden(string arguments = "")
        {
            string winwsPath = GetFilePath("winws.exe");

            if (!FileExists("winws.exe"))
            {
                Console.WriteLine("ERROR: winws.exe not found in bin directory");
                return null;
            }

            return StartHiddenProcess(winwsPath, arguments, Path.Combine(_tempDirectory, "bin"));
        }

        // Метод для получения файла из ресурсов
        private byte[] GetFileFromResources(string fileName)
        {
            try
            {
                if (_resourceCache.ContainsKey(fileName))
                {
                    return _resourceCache[fileName];
                }

                // Пытаемся найти ресурс в сборке
                var assembly = Assembly.GetExecutingAssembly();
                string[] resourceNames = assembly.GetManifestResourceNames();

                // Ищем ресурс по имени файла
                string fullResourceName = resourceNames.FirstOrDefault(name =>
                    name.EndsWith(fileName, StringComparison.OrdinalIgnoreCase) ||
                    name.EndsWith("." + fileName, StringComparison.OrdinalIgnoreCase) ||
                    name.Contains(fileName));

                if (fullResourceName != null)
                {
                    Console.WriteLine($"Found embedded resource: {fullResourceName}");
                    using (Stream stream = assembly.GetManifestResourceStream(fullResourceName))
                    {
                        if (stream != null)
                        {
                            byte[] buffer = new byte[stream.Length];
                            stream.Read(buffer, 0, buffer.Length);
                            _resourceCache[fileName] = buffer;
                            Console.WriteLine($"Loaded resource {fileName} - {buffer.Length} bytes");
                            return buffer;
                        }
                    }
                }

                Console.WriteLine($"Resource not found: {fileName}");
                Console.WriteLine($"Available resources: {string.Join(", ", resourceNames)}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading resource {fileName}: {ex.Message}");
                return null;
            }
        }

        // Метод для получения полного пути к файлу в структуре
        public string GetFilePath(string fileName)
        {
            // Проверяем корневые bat файлы
            var rootBatchFiles = new[]
            {
                "general.bat", "general (ALT).bat", "general (ALT2).bat", "general (ALT3).bat",
                "general (ALT4).bat", "general (ALT5).bat", "general (ALT6).bat", "general (ALT7).bat",
                "general (FAKE TLS AUTO ALT).bat", "general (FAKE TLS AUTO ALT2).bat",
                "general (FAKE TLS AUTO ALT3).bat", "general (FAKE TLS AUTO).bat",
                "general (SIMPLE FAKE ALT).bat", "general (SIMPLE FAKE).bat", "service.bat"
            };

            if (rootBatchFiles.Contains(fileName, StringComparer.OrdinalIgnoreCase))
                return Path.Combine(_tempDirectory, fileName);

            // Проверяем файлы в lists
            var listsFiles = new[]
            {
                "ipset-all.txt", "ipset-all.txt.backup", "ipset-exclude.txt",
                "list-exclude.txt", "list-general.txt", "list-google.txt"
            };

            if (listsFiles.Contains(fileName, StringComparer.OrdinalIgnoreCase))
                return Path.Combine(_tempDirectory, "lists", fileName);

            // Проверяем файлы в bin
            var binFiles = new[]
            {
                "winws.exe", "quic_initial_www_google_com.bin", "tls_clienthello_4pda_to.bin",
                "tls_clienthello_www_google_com.bin", "WinDivert.dll", "WinDivert64.sys", "cygwin1.dll"
            };

            if (binFiles.Contains(fileName, StringComparer.OrdinalIgnoreCase))
                return Path.Combine(_tempDirectory, "bin", fileName);

            // По умолчанию ищем в корне
            return Path.Combine(_tempDirectory, fileName);
        }

        // Проверка существования файла
        public bool FileExists(string fileName)
        {
            string filePath = GetFilePath(fileName);
            bool exists = File.Exists(filePath);
            if (!exists)
            {
                Console.WriteLine($"File not found: {filePath}");
            }
            return exists;
        }

        // Извлечение ресурса во временную директорию
        public bool ExtractResourceToTemp(string fileName)
        {
            try
            {
                string filePath = GetFilePath(fileName);

                // Проверяем, существует ли файл уже
                if (File.Exists(filePath))
                {
                    Console.WriteLine($"File already exists: {fileName}");
                    return true;
                }

                byte[] fileBytes = GetFileFromResources(fileName);
                if (fileBytes != null && fileBytes.Length > 0)
                {
                    // Создаем директорию, если она не существует
                    string directory = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    File.WriteAllBytes(filePath, fileBytes);
                    Console.WriteLine($"Extracted: {fileName} -> {filePath} ({fileBytes.Length} bytes)");
                    return true;
                }
                else
                {
                    Console.WriteLine($"ERROR: Resource not found or empty: {fileName}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR extracting {fileName}: {ex.Message}");
                return false;
            }
        }

        // Извлечение нескольких ресурсов
        public void ExtractMultipleResources(IEnumerable<string> fileNames)
        {
            int successCount = 0;
            int failCount = 0;

            foreach (string fileName in fileNames)
            {
                if (ExtractResourceToTemp(fileName))
                {
                    successCount++;
                }
                else
                {
                    failCount++;
                    Console.WriteLine($"FAILED to extract: {fileName}");
                }
            }

            Console.WriteLine($"Extraction completed: {successCount} successful, {failCount} failed");
        }

        // Получение пути к временной директории
        public string GetTempFilePath(string fileName)
        {
            return GetFilePath(fileName);
        }

        // Проверка существования файла во временной директории
        public bool FileExistsInTemp(string fileName)
        {
            return FileExists(fileName);
        }

        // Очистка ресурсов
        public void Cleanup()
        {
            try
            {
                // Очищаем кэш
                _resourceCache.Clear();
                Console.WriteLine("Resource cache cleared");

                // Не удаляем временную директорию, чтобы избежать ошибок доступа
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cleanup error: {ex.Message}");
            }
        }

        // Получение списка всех созданных файлов
        public List<string> GetAllCreatedFiles()
        {
            var files = new List<string>();

            try
            {
                // Корневые bat файлы
                var rootBatchFiles = new[]
                {
                    "general.bat", "general (ALT).bat", "general (ALT2).bat", "general (ALT3).bat",
                    "general (ALT4).bat", "general (ALT5).bat", "general (ALT6).bat", "general (ALT7).bat",
                    "general (FAKE TLS AUTO ALT).bat", "general (FAKE TLS AUTO ALT2).bat",
                    "general (FAKE TLS AUTO ALT3).bat", "general (FAKE TLS AUTO).bat",
                    "general (SIMPLE FAKE ALT).bat", "general (SIMPLE FAKE).bat", "service.bat"
                };

                files.AddRange(rootBatchFiles.Where(f => FileExists(f)));

                // Файлы в lists
                var listsFiles = new[]
                {
                    "ipset-all.txt", "ipset-all.txt.backup", "ipset-exclude.txt",
                    "list-exclude.txt", "list-general.txt", "list-google.txt"
                };

                files.AddRange(listsFiles.Where(f => FileExists(f)));

                // Файлы в bin
                var binFiles = new[]
                {
                    "winws.exe", "quic_initial_www_google_com.bin", "tls_clienthello_4pda_to.bin",
                    "tls_clienthello_www_google_com.bin", "WinDivert.dll", "WinDivert64.sys", "cygwin1.dll"
                };

                files.AddRange(binFiles.Where(f => FileExists(f)));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting file list: {ex.Message}");
            }

            return files;
        }

        // Проверка целостности всех необходимых файлов
        public bool VerifyAllFiles()
        {
            var allRequiredFiles = new[]
            {
                // Корневые bat файлы
                "general.bat", "service.bat",
                
                // Важные файлы в lists
                "list-general.txt", "ipset-all.txt",
                
                // Критичные файлы в bin
                "winws.exe", "WinDivert.dll", "WinDivert64.sys"
            };

            bool allFilesExist = true;

            foreach (var file in allRequiredFiles)
            {
                if (!FileExists(file))
                {
                    Console.WriteLine($"MISSING FILE: {file}");
                    allFilesExist = false;
                }
            }

            if (allFilesExist)
            {
                Console.WriteLine("All required files are present");
            }
            else
            {
                Console.WriteLine("ERROR: Some required files are missing!");
            }

            return allFilesExist;
        }
    }
}