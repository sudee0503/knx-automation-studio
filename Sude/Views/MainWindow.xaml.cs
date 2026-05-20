using Sude.Models;
using Sude.Services;
using Sude.ViewModels;
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

namespace Sude.Views
{
    /// <summary>
    /// MainWindow.xaml etkileşim mantığı
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();

            if (SessionService.ActiveUserId > 0)
            {
                int currentUserId = SessionService.ActiveUserId;
                SessionService.ActiveUserId = 0; 

                var dbService = new DatabaseService();

                _ = dbService.LogActionAsync(currentUserId, IslemTuru.SistemdenCikis);

                new EittService().ReleaseEitt();
            }

            base.OnClosing(e);
        }
    }
}
