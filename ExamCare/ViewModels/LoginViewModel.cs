using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExamCare.Services;
using System;
using System.Threading.Tasks;

namespace ExamCare.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly MainWindowViewModel _main;
        private readonly StudentExamApiService _apiService;

        [ObservableProperty]
        private string examCode = string.Empty;

        [ObservableProperty]
        private string email = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private bool isLoading;

        public LoginViewModel(MainWindowViewModel main, StudentExamApiService apiService)
        {

            _main = main;
            _apiService = apiService;
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            try
            {
                ErrorMessage = string.Empty;

                if (string.IsNullOrWhiteSpace(ExamCode))
                {
                    ErrorMessage = "Vui lòng nhập mã đề thi.";
                    return;
                }

                if (string.IsNullOrWhiteSpace(Email))
                {
                    ErrorMessage = "Vui lòng nhập email.";
                    return;
                }

                if (string.IsNullOrWhiteSpace(Password))
                {
                    ErrorMessage = "Vui lòng nhập mật khẩu.";
                    return;
                }

                IsLoading = true;

                Console.WriteLine($"ExamCode: {ExamCode}");
                Console.WriteLine($"Email: {Email}");

                var loginResult = await _apiService.LoginExamAsync(ExamCode, Email, Password);

                if (loginResult == null || string.IsNullOrWhiteSpace(loginResult.AccessToken))
                {
                    ErrorMessage = "Đăng nhập thất bại, không nhận được token.";
                    return;
                }

                _main.ShowDashboard();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                Console.WriteLine("Login error: " + ex);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}