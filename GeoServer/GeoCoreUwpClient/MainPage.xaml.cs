using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Core.Preview;

using GeoServer;
using System;
using System.Threading;

namespace GeoCoreUwpClient
{
    public sealed partial class MainPage : Page
    {
        private Client client;

        private Random rnd = new Random();

        public MainPage()
        {
            this.InitializeComponent();
            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += this.OnCloseRequest;
        }

        private void AddLine(string s)
        {
            debugLog.Text += DateTime.Now + " - " + s + "\n";
        }

        private void ConnectOnClick(object sender, RoutedEventArgs e)
        {
            if (client != null)
                return;

            AddLine("ConnectOnClick");

            client = new Client("192.168.178.34", 12345, "Client UWP", ThreadingType.Task, ClientType.UWP);

            client.Message += RecieveMessage;

            client.DoesDllWork();

            client.Connect();

            ConnectButton.IsEnabled = false;
            DisconnectButton.IsEnabled = true;
            TestDataButton.IsEnabled = true;
            AltTestButton.IsEnabled = true;
        }

        private void RecieveMessage(object sender, MessageArgs e)
        {
            AddLine(e.Message);
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

            TestData test = new TestData
            { number = rnd.Next(1, 200000000) };

            client.Send(test);

            UpdateScrollview();
        }

        private void AlternativeTestDataOnClick(object sender, RoutedEventArgs e)
        {
            if (client == null)
                return;

            AlternativeTestData test = new AlternativeTestData
            {
                txt = "UWPtxt",
                arr = Serialisation.FillArr(rnd.Next(1, 12))
            };

            client.Send(test);

            UpdateScrollview();
        }

        /// <summary>
        /// Scrolls to the End of the ScrollViewer
        /// </summary>
        private void UpdateScrollview()
        {
            scrollView.UpdateLayout();
            scrollView.ChangeView(0,scrollView.ScrollableHeight,1,true);
        }
    }
}
