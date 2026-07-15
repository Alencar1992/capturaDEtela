using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace CapturaRapida.Native;

internal static class CaptureService
{
    public static Bitmap CaptureMonitorUnderCursor()
    {
        var cursorPosition = Cursor.Position;
        var screen = Screen.FromPoint(cursorPosition);
        var bounds = screen.Bounds;

        var bitmap = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);

        using var graphics = Graphics.FromImage(bitmap);
        graphics.CopyFromScreen(
            bounds.Left,
            bounds.Top,
            0,
            0,
            bounds.Size,
            CopyPixelOperation.SourceCopy);

        return bitmap;
    }

    public static void CopyImageToClipboard(Image image)
    {
        const int maximumAttempts = 5;

        for (var attempt = 1; attempt <= maximumAttempts; attempt++)
        {
            try
            {
                Clipboard.SetImage(image);
                return;
            }
            catch (ExternalException) when (attempt < maximumAttempts)
            {
                Thread.Sleep(60);
            }
        }
    }
}
