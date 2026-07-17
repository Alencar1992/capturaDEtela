namespace CapturaRapida.Native;

internal sealed class AppConfig
{
    public bool SetupCompleted { get; set; }
    public HotkeyModifiers Modifiers { get; set; } = HotkeyModifiers.None;
    public Keys Key { get; set; } = Keys.F8;
    public HotkeyModifiers SelectionModifiers { get; set; } = HotkeyModifiers.None;
    public Keys SelectionKey { get; set; } = Keys.F9;
    public bool StartWithWindows { get; set; }
    public bool SaveToFile { get; set; }
    public string SaveDirectory { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
        "Captura Rapida");
}
