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

    public static Bitmap CaptureVirtualScreen()
    {
        var bounds = SystemInformation.VirtualScreen;
        var bitmap = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);

        using var graphics = Graphics.FromImage(bitmap);
        graphics.CopyFromScreen(bounds.Left, bounds.Top, 0, 0, bounds.Size, CopyPixelOperation.SourceCopy);
        return bitmap;
    }

    public static Bitmap Crop(Bitmap source, Rectangle selection)
    {
        var imageBounds = new Rectangle(Point.Empty, source.Size);
        var safeSelection = Rectangle.Intersect(imageBounds, selection);
        if (safeSelection.Width < 2 || safeSelection.Height < 2)
        {
            throw new InvalidOperationException("Selecione uma área válida da tela.");
        }

        return source.Clone(safeSelection, PixelFormat.Format32bppArgb);
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

    public static string SavePng(Image image, string directory)
    {
        if (string.IsNullOrWhiteSpace(directory))
        {
            throw new InvalidOperationException("Escolha uma pasta para salvar as capturas.");
        }

        Directory.CreateDirectory(directory);

        var timestamp = DateTime.Now.ToString("dd-MM-yyyy_HH.mm.ss");
        var path = Path.Combine(directory, $"Print_{timestamp}.png");
        var sequence = 2;
        while (File.Exists(path))
        {
            path = Path.Combine(directory, $"Print_{timestamp}_{sequence}.png");
            sequence++;
        }
        image.Save(path, ImageFormat.Png);
        return path;
    }
}
