using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using System.IO;

namespace launcherDiscord
{
    public partial class preset : Form
    {
        private readonly string appName = "LauncherDiscord";
        private readonly string startupRegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

        public preset()
        {
            InitializeComponent();
        }

        // Статическое свойство для общего доступа
        public static string SelectedPreset { get; set; } = "general";

        private void btnStart_Click(object sender, EventArgs e)
        {
            SelectedPreset = cbSelected.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void preset_Load(object sender, EventArgs e)
        {
            cbSelected.Text = SelectedPreset;
            UpdateAutoStartStatus();
        }

        private void UpdateAutoStartStatus()
        {
            bool isInStartup = IsAppInStartup();
            chkAutoStart.Checked = isInStartup;
            chkAutoStart.Text = isInStartup ? "✅ Убрать из автозагрузки" : "❌ Добавить в автозагрузку";
        }

        private bool IsAppInStartup()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(startupRegistryPath))
                {
                    if (key != null)
                    {
                        object value = key.GetValue(appName);
                        return value != null;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка проверки автозагрузки: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }

        private void AddToStartup()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(startupRegistryPath, true))
                {
                    if (key != null)
                    {
                        string appPath = $"\"{Application.ExecutablePath}\"";
                        key.SetValue(appName, appPath);
                        MessageBox.Show("Программа добавлена в автозагрузку", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления в автозагрузку: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RemoveFromStartup()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(startupRegistryPath, true))
                {
                    if (key != null)
                    {
                        key.DeleteValue(appName, false);
                        MessageBox.Show("Программа убрана из автозагрузки", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления из автозагрузки: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void chkAutoStart_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAutoStart.Focused) // Только если изменение вызвано пользователем
            {
                if (chkAutoStart.Checked)
                {
                    AddToStartup();
                }
                else
                {
                    RemoveFromStartup();
                }
                UpdateAutoStartStatus();
            }
        }

        // Альтернативный метод через папку автозагрузки (раскомментировать если нужно)
        private void AddToStartupFolder()
        {
            try
            {
                string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                string shortcutPath = Path.Combine(startupFolder, $"{appName}.lnk");

                // Создание ярлыка (требуется ссылка на COM объект Windows Script Host)
                // dynamic shell = Activator.CreateInstance(Type.GetTypeFromProgID("WScript.Shell"));
                // dynamic shortcut = shell.CreateShortcut(shortcutPath);
                // shortcut.TargetPath = Application.ExecutablePath;
                // shortcut.WorkingDirectory = Path.GetDirectoryName(Application.ExecutablePath);
                // shortcut.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания ярлыка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAdvancedSettings_Click(object sender, EventArgs e)
        {
            // Дополнительные настройки автозагрузки
            var result = MessageBox.Show(
                "Выберите метод автозагрузки:\n\nДа - через реестр (рекомендуется)\nНет - через папку автозагрузки\nОтмена - текущий метод",
                "Настройки автозагрузки",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // Уже используем реестр
                MessageBox.Show("Используется метод автозагрузки через реестр", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (result == DialogResult.No)
            {
                MessageBox.Show("Метод через папку автозагрузки в разработке", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void lblAutoStartInfo_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "Автозагрузка позволяет программе запускаться автоматически при входе в Windows.\n\n" +
                "• ✅ Программа в автозагрузке\n" +
                "• ❌ Программа не в автозагрузке\n\n" +
                "Текущий статус: " + (IsAppInStartup() ? "В автозагрузке" : "Не в автозагрузке"),
                "Справка по автозагрузке",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void cbSelected_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Обновляем выбранный пресет при изменении комбобокса
            SelectedPreset = cbSelected.Text;
        }
    }
}
