using System;
using System.Windows.Forms;
using Microsoft.Win32;

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
            chkAutoStart.Text = isInStartup ? "Remove from startup" : "Add to startup";
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
                MessageBox.Show($"Startup check error: {ex.Message}", "Error",
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
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Startup add error: {ex.Message}", "Error",
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
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Startup remove error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void chkAutoStart_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAutoStart.Focused)
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

        private void cbSelected_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedPreset = cbSelected.Text;
        }
    }
}