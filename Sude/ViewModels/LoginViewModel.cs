using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sude.Services;
using Sude.Models;
using Sude.Views;
using Sude.ViewModels.AdminViewModels;
using Sude.ViewModels.UserViewModels;
using System.Windows.Controls;

namespace Sude.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly DatabaseService _dbService;
        private readonly MainViewModel _mainRouter;

        [ObservableProperty] private string _username;

        public LoginViewModel(MainViewModel mainRouter)
        {
            _mainRouter = mainRouter;
            _dbService = new DatabaseService();
        }

        [RelayCommand]
        private async Task Login(object parameter)
        {
            try
            {
                var passwordBox = parameter as PasswordBox;
                string password = passwordBox?.Password;

                if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(password))
                {
                    CustomMessageBoxWindow.Show("Lütfen kullanıcı adı ve şifrenizi girin.", "Uyarı", CustomMessageBoxType.Warning);
                    return;
                }

                var user = await _dbService.LoginAsync(Username, password);

                if (user != null)
                {
                    SessionService.ActiveUserId = user.Id;
                    await _dbService.LogActionAsync(user.Id, IslemTuru.SistemeGiris);

                    if (user.Role == "Admin")
                        _mainRouter.Navigate(new AdminPanelViewModel());
                    else if (user.Role == "User")
                        _mainRouter.Navigate(new DeviceSelectionViewModel(_mainRouter));
                }
                else
                {
                    CustomMessageBoxWindow.Show("Hatalı kullanıcı adı/şifre veya hesabınız pasif durumda!", "Giriş Başarısız", CustomMessageBoxType.Error);
                }
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show($"Sistem hatası:\n{ex.Message}", "Hata", CustomMessageBoxType.Error);
            }
        }
    }
}