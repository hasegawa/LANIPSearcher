using System;
using System.Windows;

namespace LANIPSearcher
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            this.StartupUri = new Uri("LANSearch.xaml", UriKind.Relative);
        }
    }
}
