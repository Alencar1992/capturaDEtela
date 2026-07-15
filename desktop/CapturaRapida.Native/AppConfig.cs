namespace CapturaRapida.Native;

internal sealed class AppConfig
{
    public bool SetupCompleted { get; set; }
    public HotkeyModifiers Modifiers { get; set; } = HotkeyModifiers.Control | HotkeyModifiers.Alt;
    public Keys Key { get; set; } = Keys.P;
    public bool StartWithWindows { get; set; }
    public bool SaveToFile { get; set; }
    public string SaveDirectory { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
        "Captura Rapida");
}
