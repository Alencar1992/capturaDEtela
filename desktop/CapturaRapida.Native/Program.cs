namespace CapturaRapida.Native;

internal static class Program
{
    private const string SingleInstanceName = "CapturaRapida.Native.SingleInstance";

    [STAThread]
    private static void Main()
    {
        using var mutex = new Mutex(initiallyOwned: true, SingleInstanceName, out var isFirstInstance);

        if (!isFirstInstance)
        {
            MessageBox.Show(
                "O Captura Rápida já está em execução na bandeja do Windows.",
                "Captura Rápida",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}
