using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sude.Models;
using Sude.Services;
using Sude.Views;
using System;
using System.Threading.Tasks;

namespace Sude.ViewModels.AdminViewModels.DialogWindowViewModels
{
    public partial class UserEditViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;
        public Action CloseAction { get; set; }

        [ObservableProperty] private User _editingUser;
        [ObservableProperty] private int _userId;
        [ObservableProperty] private string _username;
        [ObservableProperty] private string _password;

        public UserEditViewModel(User userToEdit)
        {
            _databaseService = new DatabaseService();
            EditingUser = userToEdit;
            UserId = userToEdit.Id;
            Username = userToEdit.Username;
            Password = userToEdit.Password;
        }

        [RelayCommand]
        private async Task Update()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
                {
                    CustomMessageBoxWindow.Show("Lütfen kullanıcı adı ve şifre alanlarını boş bırakmayın.", "Uyarı", CustomMessageBoxType.Warning);
                    return;
                }

                EditingUser.Username = Username;
                EditingUser.Password = Password;

                await _databaseService.UpdateUserAsync(EditingUser);
                await _databaseService.LogActionAsync(SessionService.ActiveUserId, IslemTuru.KullaniciGuncellendi, targetUserId: EditingUser.Id);

                CustomMessageBoxWindow.Show("Kullanıcı bilgileri güncellendi.", "Başarılı", CustomMessageBoxType.Success);
                CloseAction?.Invoke();
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show($"Güncelleme sırasında hata:\n{ex.Message}", "Hata", CustomMessageBoxType.Error);
            }
        }

        [RelayCommand]
        private void Cancel() => CloseAction?.Invoke();
    }
}