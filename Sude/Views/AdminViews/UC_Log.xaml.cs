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
    /// UC_Log.xaml etkileşim mantığı
    /// </summary>
    public partial class UC_Log : UserControl
    {
        public UC_Log()
        {
            try
            {
            InitializeComponent();
            DataContext = new LogViewModel();
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show($"Log sayfası yüklenirken bir hata oluştu: {ex.Message}", "Hata", CustomMessageBoxType.Error);
            }
        }
    }
}
