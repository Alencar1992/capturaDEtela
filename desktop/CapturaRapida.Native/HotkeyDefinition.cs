namespace CapturaRapida.Native;

[Flags]
internal enum HotkeyModifiers : uint
{
    None = 0,
    Alt = 0x0001,
    Control = 0x0002,
    Shift = 0x0004,
    Win = 0x0008,
    NoRepeat = 0x4000,
}

internal sealed record HotkeyOption(string Label, Keys Key)
{
    public override string ToString() => Label;
}

internal static class HotkeyFormatter
{
    public static string Format(HotkeyModifiers modifiers, Keys key)
    {
        var parts = new List<string>();

        if (modifiers.HasFlag(HotkeyModifiers.Control)) parts.Add("Ctrl");
        if (modifiers.HasFlag(HotkeyModifiers.Alt)) parts.Add("Alt");
        if (modifiers.HasFlag(HotkeyModifiers.Shift)) parts.Add("Shift");
        if (modifiers.HasFlag(HotkeyModifiers.Win)) parts.Add("Win");

        parts.Add(KeyLabel(key));
        return string.Join(" + ", parts);
    }

    private static string KeyLabel(Keys key) => key switch
    {
        >= Keys.A and <= Keys.Z => key.ToString(),
        >= Keys.F1 and <= Keys.F12 => key.ToString(),
        Keys.PrintScreen => "Print Screen",
        _ => key.ToString(),
    };
}
