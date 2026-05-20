using Sude.Views;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Threading;

namespace Sude
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            CustomMessageBoxWindow.Show($"Beklenmedik bir hata oluştu. Lütfen sistem yöneticinize başvurun.\n\nHata Detayı: {e.Exception.Message}",
                                        "Kritik Sistem Hatası", CustomMessageBoxType.Error);

            e.Handled = true;
        }
    }

}
