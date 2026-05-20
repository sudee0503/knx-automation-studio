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
using Sude.Models;
using Sude.ViewModels.AdminViewModels.DialogWindowViewModels;

namespace Sude.Views.AdminViews.DialogViews
{
    public partial class UserEdit : UserControl
    {
        public UserEdit(User selectedUser)
        {
            InitializeComponent();

            var vm = new UserEditViewModel(selectedUser);
            DataContext = vm;

            Loaded += (s, e) =>
            {
                if (Window.GetWindow(this) is DialogWindow win)
                    vm.CloseAction = () => win.Close();
            };
        }
    }
}