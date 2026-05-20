using CommunityToolkit.Mvvm.Input;
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
using Sude.ViewModels.AdminViewModels.DialogWindowViewModels;

namespace Sude.Views.AdminViews
{
    /// <summary>
    /// UC_User.xaml etkileşim mantığı
    /// </summary>
    public partial class UC_User : UserControl
    {
        public UC_User()
        {
            try
            {
            InitializeComponent();
            DataContext = new UserViewModel();
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show($"Kullanıcı sayfası yüklenirken bir hata oluştu: {ex.Message}", "Hata", CustomMessageBoxType.Error);
            }
        }
    }
}
