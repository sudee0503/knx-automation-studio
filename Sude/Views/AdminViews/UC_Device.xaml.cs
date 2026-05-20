using Sude.ViewModels.AdminViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Sude.Views.AdminViews
{
    /// <summary>
    /// UC_Device.xaml etkileşim mantığı
    /// </summary>
    public partial class UC_Device : UserControl
    {
        public UC_Device()
        {
            try
            {
                InitializeComponent();
                DataContext = new DeviceViewModel(); 
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show($"Ürünler sayfası yüklenirken bir hata oluştu: {ex.Message}", "Hata", CustomMessageBoxType.Error);
            }
        }
    }
}
