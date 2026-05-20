using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sude.Models;
using Sude.Services;
using Sude.Views;
using System;
using System.Threading.Tasks;

namespace Sude.ViewModels.AdminViewModels.DialogWindowViewModels
{
    public partial class UserAddViewModel : ObservableObject
    {
        [ObservableProperty]
        private string username;

        [ObservableProperty]
        private string password;

        private readonly DatabaseService _databaseService;
        public Action CloseAction { get; set; }

        public UserAddViewModel()
        {
            _databaseService = new DatabaseService();
        }

        [RelayCommand]
        private async Task Save()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
                {
                    CustomMessageBoxWindow.Show("Kullanıcı adı ve şifre boş bırakılamaz!", "Uyarı", CustomMessageBoxType.Warning);
                    return;
                }

                var user = new User
                {
                    Username = Username,
                    Password = Password,
                    Role = "User"
                };

                int newUserId = await _databaseService.AddUserAsync(user);
                await _databaseService.LogActionAsync(SessionService.ActiveUserId, IslemTuru.YeniKullaniciEklendi, targetUserId: newUserId);

                CustomMessageBoxWindow.Show("Yeni kullanıcı başarıyla eklendi.", "Başarılı", CustomMessageBoxType.Success);
                CloseAction?.Invoke();
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show($"Kayıt sırasında veritabanı hatası:\n{ex.Message}", "Hata", CustomMessageBoxType.Error);
            }
        }

        [RelayCommand]
        private void Cancel() => CloseAction?.Invoke();
    }
}