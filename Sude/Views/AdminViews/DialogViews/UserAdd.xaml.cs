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
using Sude.ViewModels;
using Sude; 
using Sude.ViewModels.AdminViewModels.DialogWindowViewModels;

namespace Sude.Views.AdminViews.DialogViews
{
    /// <summary>
    /// UserAdd.xaml etkileşim mantığı
    /// </summary>
    public partial class UserAdd : UserControl
    {
        public UserAdd()
        {
            InitializeComponent();

            var vm = new UserAddViewModel();
            DataContext = vm;

            Loaded += (s, e) =>
            {
                if (Window.GetWindow(this) is DialogWindow win)
                    vm.CloseAction = () => win.Close();
            };
        }
    }
}
