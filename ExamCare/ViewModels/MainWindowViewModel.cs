using CommunityToolkit.Mvvm.ComponentModel;
using ExamCare.Views;
using System;
namespace ExamCare.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private object? currentView;

        public Action? CloseAction { get; set; }

        public void Init()
        {
            // 👇 truyền chính VM vào LoginView
            CurrentView = new LoginView();
        }

        public void ShowDashboard()
        {
            CurrentView = new DashboardView
            {
                DataContext = new DashboardViewModel(() =>
                {
                    CloseAction?.Invoke();
                })
            };
        }
    }
}