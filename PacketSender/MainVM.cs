﻿using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Newtonsoft.Json;
using PacketSender.Model;

namespace PacketSender
{
    public class MainVM : NotifyPropertyChanged
    {
        private string _configParameterFilePath = "DefaultParameter.json";
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private TcpListener _tcpServer;
        private PacketSenderParameterModel _parameterModel = new PacketSenderParameterModel();
        private bool _isListening = false;

        public bool IsListening
        {
            get => _isListening;
            set
            {
                _isListening = value;
                OnPropertyChanged();
            }
        }

        public PacketSenderParameterModel ParameterModel
        {
            get => _parameterModel;
            set
            {
                _parameterModel = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<LogItem> LogItemList { get; set; } = new ObservableCollection<LogItem>();

        public MainVM()
        {
            StartListenCommand = new RelayCommand(StartListen);
            ClearLogCommand = new RelayCommand(ClearLog);
            ClearAllConnCommand = new RelayCommand(ClearAllConnection);
            LoadDefaultParamter();
        }

        public ICommand StartListenCommand { get; }

        public ICommand ClearLogCommand { get; }

        public ICommand ClearAllConnCommand { get; }

        private void LoadDefaultParamter()
        {
            try
            {
                if (!File.Exists(_configParameterFilePath))
                {
                    return;
                }

                var configJson = System.IO.File.ReadAllText(_configParameterFilePath);
                var configModel = JsonConvert.DeserializeObject<PacketSenderParameterModel>(configJson);
                ParameterModel = configModel;
            }
            catch (Exception ex)
            {
                ParameterModel = new PacketSenderParameterModel();
                AddLog(LogLevel.Error, ex.Message, LogDestination.DispalyAndLogFile);
            }
        }

        /// <summary>
        /// Save setting into json files
        /// </summary>
        /// <param name="configFilePath"></param>
        public void SaveDefaultParamter()
        {
            try
            {
                var json = JsonConvert.SerializeObject(ParameterModel);
                File.WriteAllText(_configParameterFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async void StartListen(object para)
        {
            try
            {
                int tcpPort = 12306;
                if (IsListening)
                {
                    AddLog(LogLevel.Info, $"TCP listener started on port {tcpPort}", LogDestination.DispalyAndLogFile);
                    return;
                }

                _tcpServer = new TcpListener(IPAddress.Parse(_parameterModel.TcpListenerIp), tcpPort);
                _tcpServer.Start();
                AddLog(LogLevel.Info, $"TCP listener started on port {tcpPort}", LogDestination.DispalyAndLogFile);
                IsListening = true;
                _cts?.Token.ThrowIfCancellationRequested();

                // wait for a TCP client connection
                TcpClient tcpClient = await _tcpServer.AcceptTcpClientAsync();
                var lep = tcpClient.Client.RemoteEndPoint as IPEndPoint;
                AddLog(LogLevel.Warning, $"[{lep.Address}:{lep.Port}] request connection", LogDestination.DispalyAndLogFile);
                AddLog(LogLevel.Warning, $"{lep.Address}:{lep.Port}] connected", LogDestination.DispalyAndLogFile);
                await HandleTcpClientAsync(tcpClient, _parameterModel.TargetUdpIp, _parameterModel.TargetUdpPort);
            }
            catch (Exception ex)
            {
                AddLog(LogLevel.Info, $"[{_parameterModel.TcpListenerIp}] {ex.Message}", LogDestination.DispalyAndLogFile);
                ClearAllConnection(null);
            }
        }

        async Task HandleTcpClientAsync(TcpClient tcpClient, string udpTargetIp, int udpTargetPort)
        {
            try
            {

                NetworkStream networkStream = tcpClient.GetStream();
                byte[] buffer = new byte[1024];
                int bytesRead;

                while ((bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    _cts?.Token.ThrowIfCancellationRequested();
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    AddLog(LogLevel.Info, $"Received message: {message}", LogDestination.DispalyAndLogFile);
                    var cmdInfo = message.Trim();
                    if (string.IsNullOrEmpty(cmdInfo))
                    {
                        AddLog(LogLevel.Error, "Invalid command", LogDestination.DispalyAndLogFile);
                        continue;
                    }
                    var cmdInfoArr = cmdInfo.Split('#');
                    if (cmdInfoArr.Length != 2)
                    {
                        AddLog(LogLevel.Error, "Invalid command", LogDestination.DispalyAndLogFile);
                        continue;
                    }

                    if (cmdInfoArr[0].Equals(PacketSenderCommandType.SendPacket, StringComparison.OrdinalIgnoreCase))
                    {
                        await SendUdpPacketsAsync(udpTargetIp, udpTargetPort);
                        string response = PacketSenderCommandType.SendPacket + "#OK";
                        byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                        await networkStream.WriteAsync(responseBytes, 0, responseBytes.Length);
                        AddLog(LogLevel.Info, "Sent response to TCP client", LogDestination.DispalyAndLogFile);
                    }
                    else if (cmdInfoArr[0].Equals(PacketSenderCommandType.SetPacketCount, StringComparison.OrdinalIgnoreCase))
                    {
                        var configModel = JsonConvert.DeserializeObject<ConfigModel>(cmdInfoArr[1]);
                        if (configModel == null)
                        {
                            AddLog(LogLevel.Error, "Invalid command", LogDestination.DispalyAndLogFile);
                            continue;
                        }

                        _parameterModel.PacketCount = configModel.PacketCount;
                        string response = PacketSenderCommandType.SetPacketCount + "#OK";
                        byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                        await networkStream.WriteAsync(responseBytes, 0, responseBytes.Length);
                        AddLog(LogLevel.Info, "Sent response to TCP client", LogDestination.DispalyAndLogFile);
                    }
                }

                tcpClient.Close();
                AddLog(LogLevel.Warning, "TCP client disconnected", LogDestination.DispalyAndLogFile);
                ClearAllConnection(null);
            }
            catch (Exception ex)
            {
                var lep = tcpClient.Client.RemoteEndPoint as IPEndPoint;
                AddLog(LogLevel.Info, $"[{lep.Address}:{lep.Port}] {ex.Message}", LogDestination.DispalyAndLogFile);
                ClearAllConnection(null);
            }
        }

        async Task SendUdpPacketsAsync(string ipAddress, int port)
        {
            using (UdpClient udpClient = new UdpClient())
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
                byte[] packetData = Encoding.UTF8.GetBytes("Packet data");

                for (int i = 0; i < _parameterModel.PacketCount; i++)
                {
                    await udpClient.SendAsync(packetData, packetData.Length, endPoint);
                    await Task.Delay(_parameterModel.PacketInterval);
                    AddLog(LogLevel.Info, $"Sent UDP packet {i + 1}", LogDestination.DispalyAndLogFile);
                }
            }
        }

        /// <summary>
        /// 延时
        /// </summary>
        /// <param name="millisecs">延时：毫秒</param>
        public static void DoBusyWait(int millisecs)
        {
            if (millisecs < 0)
            {
                millisecs = 0;
            }

            if (millisecs == 0)
            {
                return;
            }

            if (millisecs > 3600000)
            {
                millisecs = 3600000;
            }

            System.Threading.SpinWait.SpinUntil(new Func<bool>(() =>
            {
                return false;
            }), millisecs);
        }
        public void AddLog(LogItem logItem)
        {
            if (LogItemList.Count > 200)
            {
                LogItemList.Clear();
            }

            LogItemList.Add(logItem);
        }

        public void ClearLog(object para)
        {
            LogItemList.Clear();
        }

        public void ClearAllConnection(object para)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            _tcpServer?.Stop();
            IsListening = false;
            _tcpServer = null;
            AddLog(LogLevel.Info, "All connection cleared", LogDestination.DispalyAndLogFile);
        }

        private void AddLog(LogLevel logLevel, string message, LogDestination destination)
        {
            switch (destination)
            {
                case LogDestination.DispalyAndLogFile:
                    AddLog(new LogItem()
                    {
                        TimeStamp = DateTime.Now.ToShortTimeString(),
                        LogLevel = logLevel,
                        LogContent = message,
                        Destination = destination
                    });
                    LogHelper.AddLog(logLevel, message);
                    break;

                case LogDestination.OnlyLogFile:
                    LogHelper.AddLog(logLevel, message);
                    break;

                case LogDestination.OnlyDispaly:
                    AddLog(new LogItem()
                    {
                        TimeStamp = DateTime.Now.ToShortTimeString(),
                        LogLevel = logLevel,
                        LogContent = message,
                        Destination = destination
                    });
                    break;
                default:
                    throw new ArgumentException("Unknown log destination");
            }

        }
    }

    /// <summary>
    /// PacketSenderCommandType
    /// </summary>
    internal class PacketSenderCommandType
    {
        /// <summary>
        /// Send packet
        /// </summary>
        public const string SendPacket = "send packet";

        /// <summary>
        /// Set packet count
        /// </summary>
        public const string SetPacketCount = "set packet count";
    }

    /// <summary>
    /// ConfigModel
    /// </summary>
    public class ConfigModel
    {
        /// <summary>
        /// Packet count
        /// </summary>
        public int PacketCount { get; set; }

        /// <summary>
        /// Packet interval
        /// </summary>
        public int PacketInterval { get; set; }
    }

    public class LogItem
    {
        public string TimeStamp { get; set; }

        public LogLevel LogLevel { get; set; }

        public string LogContent { get; set; }

        public LogDestination Destination { get; set; }
    }

    public enum LogLevel
    {
        Info,

        Warning,

        Error,

        Debug
    }
    public enum LogDestination
    {
        OnlyDispaly,

        OnlyLogFile,

        DispalyAndLogFile,
    }
}
