using System.Drawing.Drawing2D;

namespace CapturaRapida.Native;

internal sealed class SelectionForm : Form
{
    private readonly Bitmap _screenImage;
    private Point _start;
    private Point _current;
    private bool _selecting;

    public Rectangle SelectedArea { get; private set; }

    public SelectionForm(Bitmap screenImage)
    {
        _screenImage = screenImage;
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        Bounds = SystemInformation.VirtualScreen;
        TopMost = true;
        ShowInTaskbar = false;
        DoubleBuffered = true;
        Cursor = Cursors.Cross;
        KeyPreview = true;
        BackColor = Color.Black;
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        Activate();
        Focus();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.DrawImageUnscaled(_screenImage, Point.Empty);
        using var shade = new SolidBrush(Color.FromArgb(115, 0, 0, 0));
        e.Graphics.FillRectangle(shade, ClientRectangle);

        var selection = GetSelectionRectangle();
        if (selection.Width < 1 || selection.Height < 1)
        {
            DrawHelp(e.Graphics);
            return;
        }

        e.Graphics.SetClip(selection);
        e.Graphics.DrawImageUnscaled(_screenImage, Point.Empty);
        e.Graphics.ResetClip();

        using var border = new Pen(Color.FromArgb(42, 192, 255), 2F) { DashStyle = DashStyle.Solid };
        e.Graphics.DrawRectangle(border, selection);
        DrawSize(e.Graphics, selection);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Right)
        {
            DialogResult = DialogResult.Cancel;
            Close();
            return;
        }

        if (e.Button != MouseButtons.Left) return;
        _start = e.Location;
        _current = e.Location;
        _selecting = true;
        Invalidate();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (!_selecting) return;
        _current = ClampToClient(e.Location);
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (!_selecting || e.Button != MouseButtons.Left) return;
        _selecting = false;
        _current = ClampToClient(e.Location);
        SelectedArea = GetSelectionRectangle();

        if (SelectedArea.Width < 4 || SelectedArea.Height < 4)
        {
            SelectedArea = Rectangle.Empty;
            Invalidate();
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Escape)
        {
            DialogResult = DialogResult.Cancel;
            Close();
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private Rectangle GetSelectionRectangle() => Rectangle.FromLTRB(
        Math.Min(_start.X, _current.X),
        Math.Min(_start.Y, _current.Y),
        Math.Max(_start.X, _current.X),
        Math.Max(_start.Y, _current.Y));

    private Point ClampToClient(Point point) => new(
        Math.Clamp(point.X, 0, Math.Max(0, ClientSize.Width - 1)),
        Math.Clamp(point.Y, 0, Math.Max(0, ClientSize.Height - 1)));

    private void DrawHelp(Graphics graphics)
    {
        const string text = "Arraste para selecionar uma área  •  Esc ou botão direito para cancelar";
        using var font = new Font("Segoe UI", 12F, FontStyle.Bold);
        var size = graphics.MeasureString(text, font);
        var box = new RectangleF((ClientSize.Width - size.Width) / 2 - 18, 24, size.Width + 36, 42);
        using var background = new SolidBrush(Color.FromArgb(205, 17, 24, 39));
        graphics.FillRoundedRectangle(background, box, 10);
        graphics.DrawString(text, font, Brushes.White, box.X + 18, box.Y + 9);
    }

    private static void DrawSize(Graphics graphics, Rectangle selection)
    {
        var text = $"{selection.Width} × {selection.Height} px";
        using var font = new Font("Segoe UI", 9F, FontStyle.Bold);
        var size = graphics.MeasureString(text, font);
        var y = (float)selection.Bottom + 7;
        if (y + size.Height + 10 > graphics.VisibleClipBounds.Bottom) y = selection.Top - size.Height - 12;
        var box = new RectangleF(selection.Left, y, size.Width + 16, size.Height + 8);
        using var background = new SolidBrush(Color.FromArgb(220, 17, 24, 39));
        graphics.FillRectangle(background, box);
        graphics.DrawString(text, font, Brushes.White, box.X + 8, box.Y + 4);
    }
}

internal static class GraphicsExtensions
{
    public static void FillRoundedRectangle(this Graphics graphics, Brush brush, RectangleF bounds, float radius)
    {
        using var path = new GraphicsPath();
        var diameter = radius * 2;
        path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        graphics.FillPath(brush, path);
    }
}
