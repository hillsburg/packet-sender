using System.Windows;

namespace PacketSender
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Closing += MainWindow_OnClosed;
        }

        private void MainWindow_OnClosed(object sender, System.ComponentModel.CancelEventArgs e)
        {
            VM.SaveDefaultParamter();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
