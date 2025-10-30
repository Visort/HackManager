using System;
using System.Collections.Generic;
using System.Linq;

namespace launcherDiscord
{
    public class PresetManager
    {
        private static readonly Lazy<PresetManager> _instance = new Lazy<PresetManager>(() => new PresetManager());
        public static PresetManager Instance => _instance.Value;

        public Dictionary<string, string> PresetBatFiles { get; private set; }
        public Dictionary<string, string[]> PresetDependencies { get; private set; }

        private PresetManager()
        {
            InitializePresetDictionary();
            InitializeDependencies();
        }

        private void InitializePresetDictionary()
        {
            PresetBatFiles = new Dictionary<string, string>
            {
                {"general", "general.bat"},
                {"general(ALT)", "general_alt.bat"},
                {"general(ALT2)", "general_alt2.bat"},
                {"general(ALT3)", "general_alt3.bat"},
                {"general(ALT4)", "general_alt4.bat"},
                {"general(ALT5)", "general_alt5.bat"},
                {"general(ALT6)", "general_alt6.bat"},
                {"general(ALT7)", "general_alt7.bat"},
                {"general(FAKE TLS AUTO ALT)", "general_fake_tls_auto_alt.bat"},
                {"general(FAKE TLS AUTO ALT2)", "general_fake_tls_auto_alt2.bat"},
                {"general(FAKE TLS AUTO ALT3)", "general_fake_tls_auto_alt3.bat"},
                {"general(FAKE TLS AUTO)", "general_fake_tls_auto.bat"},
                {"general(SIMPLE FAKE)", "general_simple_fake.bat"}
            };
        }

        private void InitializeDependencies()
        {
            PresetDependencies = new Dictionary<string, string[]>
            {
                {"general", new string[] {
                    "winws.exe", "list-general.txt", "quic_initial_www_google_com.bin",
                    "ipset-all.txt", "service.bat", "WinDivert.dll",
                    "WinDivert64.sys", "cygwin1.dll"
                }},
                {"general(ALT)", new string[] {
                    "winws.exe", "list-general.txt",
                    "quic_initial_www_google_com.bin", "ipset-all.txt"
                }},
                // ... остальные пресеты с их зависимостями
            };
        }

        public string[] GetAllDependencies()
        {
            return PresetDependencies.Values
                .SelectMany(x => x)
                .Distinct()
                .ToArray();
        }

        public string[] GetPresetDependencies(string presetName)
        {
            return PresetDependencies.ContainsKey(presetName)
                ? PresetDependencies[presetName]
                : new string[0];
        }

        public bool PresetExists(string presetName)
        {
            return PresetBatFiles.ContainsKey(presetName);
        }

        public string GetBatFileName(string presetName)
        {
            return PresetBatFiles.ContainsKey(presetName)
                ? PresetBatFiles[presetName]
                : null;
        }
    }
}