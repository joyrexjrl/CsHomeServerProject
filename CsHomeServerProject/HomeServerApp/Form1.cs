using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using HomeServerApp.Utils;
using System.IO;

namespace HomeServerApp
{
    public partial class Form1 : Form
    {
        HttpListener _listener;
        Thread _listenerThread;
        bool _serverRunning = false;
        static readonly string webRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\WebRoot"));

        public Form1()
        {
            InitializeComponent();
        }

        void ServerStartButton_Click(object sender, EventArgs e)
        {
            if (!HttpListener.IsSupported)
            {
                MessageBox.Show("HttpListener is not supported on this system.");
                return;
            }
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://*:5000/");
            _listener.Start();

            Logger.Log("Server Starting...", ServerLogsInfoTextBox);
            Logger.Log("Listening on: http://*:5000/", ServerLogsInfoTextBox);
            Logger.Log($"Machine Name: {Environment.MachineName}", ServerLogsInfoTextBox);

            string timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string localIP = Dns.GetHostEntry(Dns.GetHostName())
                .AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)?.ToString() ?? "Unavailable";

            string logText =
                $"[{timeStamp}] Server Starting...\n" +
                $"[{timeStamp}] Listening on: http://*:5000/\n" +
                $"[{timeStamp}] Machine Name: {Environment.MachineName}\n" +
                $"[{timeStamp}] Local IP Address: {localIP}\n" +
                $"[{timeStamp}] Server Status: RUNNING\n\n";

            ServerLogsInfoTextBox.AppendText(logText);
            AppendLogSeparator();

            RouteHandlers.RegisterServerRoutes();

            _listenerThread = new Thread(HandleRequests);
            _listenerThread.Start();

            ServerStartButton.Enabled = false;
            ServerStopButton.Enabled = true;
        }        

        void ServerStopButton_Click(object sender, EventArgs e)
        {
            Logger.Log("Server stopped.", ServerLogsInfoTextBox);
            _serverRunning = false;
            _listener?.Stop();
            _listenerThread?.Join();

            ServerStartButton.Enabled = true;
            ServerStopButton.Enabled = false;
        }

        void HandleRequests()
        {
            _serverRunning = true;
            while (_serverRunning)
            {
                try
                {
                    var context = _listener.GetContext();
                    var request = context.Request;
                    var response = context.Response;

                    string clientIP = context.Request.RemoteEndPoint?.Address.ToString() ?? "Unknown IP";
                    string requestUrl = request.RawUrl;
                    string method = request.HttpMethod;
                    string userAgent = request.UserAgent ?? "Unknown User Agent";
                    if (request.RawUrl == "/favicon.ico")
                    {
                        response.StatusCode = 204;
                        response.OutputStream.Close();
                        return;
                    }                    
                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    string logEntry = $"[{timestamp}] {clientIP} - {method} {requestUrl} - UserAgent: {userAgent}";
                    Logger.Log($"Client Connected - {logEntry}", ServerLogsInfoTextBox);
                    AppendLogSeparator();

                    var startTime = DateTime.Now;

                    bool handled = RouteRegistry.TryHandleRequest(request, response);

                    if (!handled)
                    {
                        string rawUrl = request.RawUrl.Split('?')[0];
                        string localPath = rawUrl == "/" ? "/index.html" : rawUrl;
                        string filePath = Path.Combine(webRoot, localPath.TrimStart('/'));

                        if (File.Exists(filePath))
                        {
                            byte[] buffer = File.ReadAllBytes(filePath);
                            response.ContentType = GetContentType(filePath);
                            response.ContentLength64 = buffer.Length;
                            response.OutputStream.Write(buffer, 0, buffer.Length);
                            response.StatusCode = 200;
                        }
                        else
                        {
                            response.StatusCode = 404;
                            string notFound = "<h1>404 - Not Found</h1>";
                            byte[] buffer = Encoding.UTF8.GetBytes(notFound);
                            response.ContentType = "text/html";
                            response.ContentLength64 = buffer.Length;
                            response.OutputStream.Write(buffer, 0, buffer.Length);
                        }                        
                    }

                    response.OutputStream.Close();

                    var duration = DateTime.Now - startTime;
                    string statusCode = response.StatusCode.ToString();
                    logEntry = $"[{timestamp}] Response Sent - Status: {statusCode}, Duration: {duration.TotalMilliseconds}ms";
                    Logger.Log(logEntry, ServerLogsInfoTextBox);
                    AppendLogSeparator();
                }
                catch (Exception ex)
                {
                    string errorLog = $"[ERROR] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {ex.Message}\n{ex.StackTrace}";
                    Console.WriteLine(errorLog);
                    Logger.Log(errorLog, ServerLogsInfoTextBox);
                    AppendLogSeparator();
                }
            }
        }

        string GetContentType(string filePath)
        {
            string ext = Path.GetExtension(filePath).ToLower();
            switch (ext)
            {
                case ".html": return "text/html";
                case ".css": return "text/css";
                case ".js": return "application/javascript";
                case ".json": return "application/json";
                case ".png": return "image/png";
                case ".jpg": return "image/jpeg";
                case ".jpeg": return "image/jpeg";
                case ".gif": return "image/gif";
                case ".svg": return "image/svg+xml";
                default: return "application/octet-stream";
            }
        }

        void AppendLogSeparator()
        {
            if (ServerLogsInfoTextBox.InvokeRequired) ServerLogsInfoTextBox.Invoke(new Action(AppendLogSeparator));
            else
            {
                ServerLogsInfoTextBox.AppendText(Environment.NewLine);
                ServerLogsInfoTextBox.AppendText("--------------------------------------");
                ServerLogsInfoTextBox.AppendText(Environment.NewLine + Environment.NewLine);
            }            
        }
    }
}
