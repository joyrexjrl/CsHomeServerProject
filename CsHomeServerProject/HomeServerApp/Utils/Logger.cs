using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HomeServerApp.Utils
{
    public static class Logger
    {
        static readonly string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

        public static void Log(string message, TextBox logTextBox = null)
        {
            try
            {
                string timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string fullMessage = $"[{timeStamp}] {message}";

                if(logTextBox != null)
                {
                    logTextBox.Invoke(new Action(() =>
                    {
                        logTextBox.AppendText(fullMessage + Environment.NewLine);
                    }));
                }

                if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);

                string filePath = Path.Combine(logDir, $"server_log_{DateTime.Now:yyyy-MM-dd}.txt");
                File.AppendAllText(filePath, fullMessage + Environment.NewLine);
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Logging error: {ex.Message}");
            }
        }
    }
}
