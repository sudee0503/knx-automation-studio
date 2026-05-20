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
using Sude.ViewModels.AdminViewModels;

namespace Sude.Views.AdminViews
{
    public partial class UC_Setting : UserControl
    {
        public UC_Setting()
        {
            InitializeComponent();
            DataContext = new SettingViewModel();
        }
    }
}