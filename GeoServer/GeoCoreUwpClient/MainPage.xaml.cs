using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Core.Preview;

using GeoStreamer;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace GeoCoreUwpClient
{
    public sealed partial class MainPage : Page
    {

        private EventClient client;

        private Random rnd = new Random();

        private ConcurrentQueue<string> debugQueue = new ConcurrentQueue<string>();

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
            //client =  Client.Initialize("192.168.178.34", 12345, "Client UWP", ThreadingType.Task, ClientType.UWP);
            client = EventClient.Initialize(ip, 12345, "Client UWP", ThreadingType.Task, ClientType.UWP);
      
            client.Message += RecieveMessage;
           // client.StartDebugging();

            client.DoesDllWork();

            client.Connect();

            ConnectButton.IsEnabled = false;
            DisconnectButton.IsEnabled = true;
            TestDataButton.IsEnabled = true;
            AltTestButton.IsEnabled = true;
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
                arr = Serialisation.FillArr(rnd.Next(1, 12))
            };

            client.Send(test);

            for (int i = 0; i < 1000; i++)
            {
                BroadCastMsg bc = new BroadCastMsg() { broadcastMsg = " Hey hier ist uwpblaaa, bei mir alles gut" };

                client.Send(bc);
            }

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
    }
}
