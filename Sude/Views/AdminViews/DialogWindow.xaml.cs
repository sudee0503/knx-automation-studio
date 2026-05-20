using CommunityToolkit.Mvvm.ComponentModel;
using Sude.Views.AdminViews.DialogViews;
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
using System.Windows.Shapes;
using Sude.ViewModels.AdminViewModels;

namespace Sude.Views.AdminViews
{
    /// <summary>
    /// DialogWindow.xaml etkileşim mantığı
    /// </summary>
    public partial class DialogWindow : Window
    {
        public DialogWindow(object page)
        {
            try
            {
                InitializeComponent();
                DataContext = new DialogWindowViewModel(page);
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show($"DialogWindow açılırken bir hata oluştu: {ex.Message}", "Hata", CustomMessageBoxType.Error);
            }
        }
    }
}
