using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Sude.Views.AdminViews.DialogViews;
using Sude.Views;

public partial class DialogWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private object currentPage;

    public DialogWindowViewModel(object page)
    {
        try
        {
            CurrentPage = page;
        }
        catch (Exception ex)
        {
            CustomMessageBoxWindow.Show($"Kullanıcılar yüklenirken bir hata oluştu: {ex.Message}", "Hata", CustomMessageBoxType.Error);
        }
    }
}
