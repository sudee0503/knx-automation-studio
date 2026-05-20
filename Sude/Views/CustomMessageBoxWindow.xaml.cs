using MaterialDesignThemes.Wpf;
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
    /// CustomMessageBoxWindow.xaml etkileşim mantığı
    /// </summary>
    public enum CustomMessageBoxType { Success, Error, Warning, Info }

    public enum CustomMessageBoxButtons { Ok, YesNo }

    public partial class CustomMessageBoxWindow : Window
    {
        public CustomMessageBoxWindow(string message, string title, CustomMessageBoxType type, CustomMessageBoxButtons buttons)
        {
            InitializeComponent();
            TxtMessage.Text = message;
            TxtTitle.Text = title;

            SetIconAndColor(type);
            SetButtons(buttons);
        }

        private void SetIconAndColor(CustomMessageBoxType type)
        {
            switch (type)
            {
                case CustomMessageBoxType.Success:
                    IconMsg.Kind = PackIconKind.CheckCircle;
                    IconMsg.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50")); // Yeşil
                    break;
                case CustomMessageBoxType.Error:
                    IconMsg.Kind = PackIconKind.CloseCircle;
                    IconMsg.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F44336")); // Kırmızı
                    break;
                case CustomMessageBoxType.Warning:
                    IconMsg.Kind = PackIconKind.Alert;
                    IconMsg.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9800")); // Turuncu
                    break;
                case CustomMessageBoxType.Info:
                    IconMsg.Kind = PackIconKind.Information;
                    IconMsg.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2196F3")); // Mavi
                    break;
            }
        }

        private void SetButtons(CustomMessageBoxButtons buttons)
        {
            if (buttons == CustomMessageBoxButtons.YesNo)
            {
                BtnOk.Content = "EVET";
                BtnCancel.Visibility = Visibility.Visible;
            }
            else
            {
                BtnOk.Content = "TAMAM";
                BtnCancel.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true; // Evet veya Tamam'a basıldı
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        public static bool Show(string message, string title, CustomMessageBoxType type, CustomMessageBoxButtons buttons = CustomMessageBoxButtons.Ok)
        {
            var window = new CustomMessageBoxWindow(message, title, type, buttons);
            bool? result = window.ShowDialog();
            return result ?? false;
        }
    }
}
