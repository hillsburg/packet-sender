namespace PacketSender.Model
{
    public class PacketSenderParameterModel : NotifyPropertyChanged
    {
        private string _targetUdpIp = "192.168.56.1";
        public string TargetUdpIp
        {
            get => _targetUdpIp;
            set
            {
                _targetUdpIp = value;
                OnPropertyChanged();
            }
        }

        private int _tcpPort = 12306;
        public int TcpPort
        {
            get => _tcpPort;
            set
            {
                _tcpPort = value;
                OnPropertyChanged();
            }
        }

        private int _targetUdpPort = 12309;
        public int TargetUdpPort
        {
            get => _targetUdpPort;
            set
            {
                _targetUdpPort = value;
                OnPropertyChanged();
            }
        }

        private int _packetCount = 100;
        public int PacketCount
        {
            get => _packetCount;
            set
            {
                _packetCount = value;
                OnPropertyChanged();
            }
        }

        private int _packetInterval = 100;
        public int PacketInterval
        {
            get => _packetInterval;
            set
            {
                _packetInterval = value;
                OnPropertyChanged();
            }
        }

        private string _message = "12345678901234567890";
        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged();
            }
        }

        private string _tcpListenerIp = "192.168.56.1";
        public string TcpListenerIp
        {
            get => _tcpListenerIp;
            set
            {
                _tcpListenerIp = value;
                OnPropertyChanged();
            }
        }
    }
}
