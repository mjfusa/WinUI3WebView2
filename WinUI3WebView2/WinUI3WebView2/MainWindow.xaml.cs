using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.Runtime.InteropServices;
using Microsoft.Web.WebView2.Core;
using Windows.Services.Store;
using System.Diagnostics;
using WinRT;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUI3WebView2
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [ComImport]
    [Guid("3E68D4BD-7135-4D10-8018-9FB6D9F33FA1")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IInitializeWithWindow
    {
        void Initialize(long hwnd);
    }

    public partial class MainWindow : Window
    {
        private StoreContext? _storeContext = null;

        public StoreProductQueryResult storeProducts { get; private set; }

        public MainWindow()
        {
            InitializeComponent();

        }

        private void ButtonGo_Click(object sender, RoutedEventArgs e)
        {
            if (webView != null && webView.CoreWebView2 != null)
            {
                webView.CoreWebView2.Navigate(addressBar.Text);
            }
        }

        bool bActivated = false;
        private async void Window_Activated(object sender, WindowActivatedEventArgs args)
        {
            if (bActivated)
            {
                return;
            } else
            {
                bActivated = true;
            }
            
            await webView.EnsureCoreWebView2Async();
            webView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;

            if (null == _storeContext)
            {
                InitializeStoreContext();
            }

            string[] productKinds = { "Durable" };
            List<string> filterList = new List<string>(productKinds);
            storeProducts = await _storeContext.GetAssociatedStoreProductsAsync(filterList);
        }


        private async void CoreWebView2_WebMessageReceived(object? sender, object e)
        {
            var p = storeProducts.Products.First().Key;
            var result = await _storeContext.RequestPurchaseAsync(storeProducts.Products[p].StoreId);
        }

        private void InitializeStoreContext()
        {
            Debug.WriteLine("StoreContext.GetDefault...");
            _storeContext = StoreContext.GetDefault();
            // Required for Store UI to work with desktop window handles (this is handled transparently in UWP)
            IInitializeWithWindow initWindow = ((object)_storeContext).As<IInitializeWithWindow>(); ;
            var hwnd = (long)System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
            initWindow.Initialize(hwnd);
        }

    }
}
