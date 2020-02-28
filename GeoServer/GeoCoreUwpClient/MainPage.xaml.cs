using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Core.Preview;

using GeoStreamer;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Windows.UI.Core;
using SocketStreamer;

namespace GeoCoreUwpClient
{
    public sealed partial class MainPage : Page
    {

        private UwpClient client;

        private Random rnd = new Random();

        private ConcurrentQueue<string> debugQueue = new ConcurrentQueue<string>();

        Serializer serializer = new Serializer();
        public MainPage()
        {
            this.InitializeComponent();
            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += this.OnCloseRequest;
        }

        private async Task AddLineAsync(string s)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { debugLog.Text += DateTime.Now + " - " + s + "\n"; });
        }

        private void ConnectOnClick(object sender, RoutedEventArgs e)
        {
            if (client != null)
                return;


            // AddLine("ConnectOnClick");
            string ip = Utils.GetTestIpAdress();
            int port = Utils.GetTestPort();
            //client =  Client.Initialize("192.168.178.34", 12345, "Client UWP", ThreadingType.Task, ClientType.UWP);
            client = UwpClient.Initialize(ip, port, "Client UWP", ThreadingType.Task, ClientType.UWP);

            client.Message += RecieveMessage;

            client.DoesDllWork();

            client.Connect();

            ConnectButton.IsEnabled = false;
            DisconnectButton.IsEnabled = true;
            TestDataButton.IsEnabled = true;
            AltTestButton.IsEnabled = true;
            BroadCastButton.IsEnabled = true;
            IndexButton1.IsEnabled = true;
            IndexButton2.IsEnabled = true;
        }

        private void RecieveMessage(object sender, MessageArgs e)
        {
            AddLineAsync(e.Message);
        }

        private void DisconnectOnClick(object sender, RoutedEventArgs e)
        {
            if (client == null)
                return;

            client.Disconnect();
            client = null;

            ConnectButton.IsEnabled = true;
            DisconnectButton.IsEnabled = false;
            TestDataButton.IsEnabled = false;
            AltTestButton.IsEnabled = false;
            BroadCastButton.IsEnabled = false;
            IndexButton1.IsEnabled = false;
            IndexButton2.IsEnabled = false;
        }

        private void OnCloseRequest(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            if (client == null)
                return;

            client.Disconnect();
            client = null;
        }

        private void TestDataOnClick(object sender, RoutedEventArgs e)
        {
            if (client == null)
                return;

            TestDataMsg test = new TestDataMsg
            { number = rnd.Next(1, 200000000) };

            client.Send(test);

            UpdateScrollview();
        }

        private void AlternativeTestDataOnClick(object sender, RoutedEventArgs e)
        {
            if (client == null)
                return;

            AlternativeTestDataMsg test = new AlternativeTestDataMsg
            {
                txt = "UWPtxt",
                arr = serializer.FillArr(rnd.Next(1, 12))
            };

            client.Send(test);

            UpdateScrollview();
        }

        private void BroadCastDataOnClick(object sender, RoutedEventArgs e)
        {
            if (client == null)
                return;


            BroadCastMsg bc = new BroadCastMsg() { broadcastMsg = " Hey hier ist uwpblaaa, bei mir alles gut" };

            client.Send(bc);


            UpdateScrollview();
        }

        /// <summary>
        /// Scrolls to the End of the ScrollViewer
        /// </summary>
        private void UpdateScrollview()
        {
            scrollView.UpdateLayout();
            scrollView.ChangeView(0, scrollView.ScrollableHeight, 1, true);
        }

        private void IndexButton1_Click(object sender, RoutedEventArgs e)
        {
            if (client == null)
                return;

            BroadCastIndex bIndex = new BroadCastIndex() { index = 7 };

            client.Send(bIndex);

            UpdateScrollview();
        }

        private void IndexButton2_Click(object sender, RoutedEventArgs e)
        {
            if (client == null)
                return;

            BroadCastIndex bIndex = new BroadCastIndex() { index = 13 };

            client.Send(bIndex);

            UpdateScrollview();
        }

        private void Slider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (client == null)
                return;

            int oldValue = (int)e.OldValue;
            int newValue = (int)e.NewValue;

            if (oldValue == newValue)
                return;

            BroadCastIndex bIndex = new BroadCastIndex() { index = newValue, gateId = 2 };

            client.Send(bIndex);

            UpdateScrollview();
        }
    }
}
