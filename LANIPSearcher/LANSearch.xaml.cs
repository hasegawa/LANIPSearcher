using LANIPSearcher.Bean;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace LANIPSearcher
{
    /// <summary>
    /// LANSearch.xaml の相互作用ロジック
    /// </summary>
    public partial class LANSearch : Window
    {
        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        private static extern int SendARP(int destinationIP, int sourceIP, byte[] macAddressPointer, ref int physicalAddressLength);

        private LANSearchBean IPBean;

        public LANSearch()
        {
            InitializeComponent();
            IPBean = new LANSearchBean();
        }

        private void SearchLAN(object sender, RoutedEventArgs e)
        {
            // 自分のIPを取得
            SetSelfLocalIP();
            // LAN内のすべてのホストへARP送信
            SendARP();
            // バインド
            this.DataContext = this.IPBean;
            
            this.SearchIP.IsEnabled = false;
        }

        private void SetSelfLocalIP()
        {
            // WiFi,LANアダプタ等のインタフェースをすべて取得
            NetworkInterface[] nis = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var interfaces in nis)
            {
                // 接続できて、ループバックインタフェース、トンネルインタフェースを除く
                if (interfaces.OperationalStatus == OperationalStatus.Up
                    && interfaces.NetworkInterfaceType != NetworkInterfaceType.Loopback
                    && interfaces.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                {
                    // インタフェースの中からさらにIPを見ていく
                    UnicastIPAddressInformationCollection unicastIPs = interfaces.GetIPProperties().UnicastAddresses;
                    foreach (var ip in unicastIPs)
                    {
                        // このPCのIPv4を登録
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            IPBean.AddSelfIP(Convert.ToString(ip.Address));
                        }
                    }
                }
            }
        }

        #region ARP送信メソッド
        private void SendARP()
        {
            // デフォルトだとスレッド起動に時間がかかるため
            int workThreadsMin;
            int ioThreadsMin;
            ThreadPool.GetMinThreads(out workThreadsMin, out ioThreadsMin);
            ThreadPool.SetMinThreads(260, ioThreadsMin);

            // 1～254のホストへARP送信
            List<Task> allTasks = new List<Task>();

            // 自ネットワークを取得
            List<string> networkPartList = new List<string>();
            for (int i = 0; i < this.IPBean.SelfIPList.Count; i++)
            {
                List<string> separateIP = this.IPBean.SelfIPList[i].Split('.').ToList();
                separateIP.RemoveAt(3);
                string network = string.Join(".", separateIP);
                networkPartList.Add(network);
            }

            for (int i = 0; i < networkPartList.Count; i++)
            {
                string networkPart = networkPartList[i];
                for (int j = 1; j <= 254; j++)
                {
                    int hostPart = j;
                    allTasks.Add(Task.Run(() =>
                    {

                    // ネットワーク部 + ホスト部でLocalIPへARPを投げる
                    string destinationIP = networkPart + "." + hostPart;
                        int destinationIPBytes = BitConverter.ToInt32(IPAddress.Parse(destinationIP).GetAddressBytes(), 0);
                        byte[] macAddressPointer = new byte[6];
                        int physicalAddressLength = macAddressPointer.Length;

                    // ARP
                    int ret = SendARP(destinationIPBytes, 0, macAddressPointer, ref physicalAddressLength);
                        if (ret == 0)
                        {
                        // デバッグ用
                        Debug.WriteLine(destinationIP);
                        this.IPBean.AddLANIP(destinationIP);
                        }
                    }
                    ));
                }

                Task t = Task.WhenAll(allTasks);
                try
                {
                    t.Wait();
                }
                catch
                {
                    MessageBox.Show("エラー終了");
                }

                if (t.Status == TaskStatus.RanToCompletion)
                {
                    // デバッグ用
                    Debug.WriteLine("end.");
                }
                else if (t.Status == TaskStatus.Faulted)
                {
                    //Debug.WriteLine("erro end.");
                }
            }
        }
        #endregion

        // リセット
        private void Reset(object sender, RoutedEventArgs e)
        {
            // テキストボックス、ViewModelをクリア。ボタンも復活させる。
            this.IPBean = new LANSearchBean();
            this.SearchResult.Clear();
            this.SearchIP.IsEnabled = true;
        }
    }
}
