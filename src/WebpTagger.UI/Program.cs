using WebpTagger.Core.Diagnostics;

namespace WebpTagger.UI;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) => ReportCrash(e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, e) => ReportCrash(e.ExceptionObject as Exception);
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            ReportCrash(e.Exception);
            e.SetObserved();
        };

        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }

    private static void ReportCrash(Exception? exception)
    {
        if (exception is null)
        {
            return;
        }

        DiagnosticsLog.Write($"Unhandled exception: {exception}");

        MessageBox.Show(
            $"An unexpected error occurred:\n\n{exception.Message}\n\n{exception}",
            "WebpTagger - Error",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }
}