using System;
using Avalonia;

namespace ExamCare
{
    internal class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Startup.Init(); // đọc appsettings.json trước

            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}