using Avalonia.Controls;
using ExamCare.ViewModels;
using ExamCare.Services;
namespace ExamCare
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var _studentService = new StudentExamApiService();
            var vm = new MainWindowViewModel(_studentService);

            // 👉 cho phép ViewModel đóng app
            vm.CloseAction = () => this.Close();

            // 👉 QUAN TRỌNG: truyền vm vào Login
            vm.Init();

            DataContext = vm;
        }
    }
}