using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace CapturaRapida.Native;

internal sealed class ImageEditorForm : Form
{
    private enum EditorTool { Mark, Blur, Text }

    private Bitmap _image;
    private readonly PictureBox _canvas;
    private readonly Stack<Bitmap> _history = new();
    private EditorTool _tool = EditorTool.Mark;
    private Point _dragStart;
    private Point _dragEnd;
    private bool _dragging;

    public ImageEditorForm(Image image)
    {
        _image = new Bitmap(image);
        Text = "Editar captura";
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(850, 600);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(28, 33, 41);
        Font = new Font("Segoe UI", 10F);

        _canvas = new PictureBox
        {
            Dock = DockStyle.Fill,
            Image = _image,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.FromArgb(28, 33, 41),
            Cursor = Cursors.Cross,
        };
        _canvas.MouseDown += CanvasMouseDown;
        _canvas.MouseMove += CanvasMouseMove;
        _canvas.MouseUp += CanvasMouseUp;
        _canvas.Paint += CanvasPaint;

        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 62,
            Padding = new Padding(12, 10, 12, 8),
            BackColor = Color.White,
        };

        toolbar.Controls.Add(CreateToolButton("Marcar campo", EditorTool.Mark));
        toolbar.Controls.Add(CreateToolButton("Borrar", EditorTool.Blur));
        toolbar.Controls.Add(CreateToolButton("Escrever texto", EditorTool.Text));

        var undo = CreateButton("Desfazer");
        undo.Click += (_, _) => Undo();
        toolbar.Controls.Add(undo);

        var cancel = CreateButton("Cancelar");
        cancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        toolbar.Controls.Add(cancel);

        var finish = CreateButton("Concluir edição");
        finish.BackColor = Color.FromArgb(22, 163, 74);
        finish.ForeColor = Color.White;
        finish.Click += (_, _) => { DialogResult = DialogResult.OK; Close(); };
        toolbar.Controls.Add(finish);

        Controls.Add(_canvas);
        Controls.Add(toolbar);
    }

    public Bitmap GetEditedImage() => new(_image);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _image.Dispose();
            while (_history.TryPop(out var item)) item.Dispose();
        }
        base.Dispose(disposing);
    }

    private Button CreateToolButton(string text, EditorTool tool)
    {
        var button = CreateButton(text);
        button.Click += (_, _) =>
        {
            _tool = tool;
            _canvas.Cursor = tool == EditorTool.Text ? Cursors.IBeam : Cursors.Cross;
        };
        return button;
    }

    private static Button CreateButton(string text) => new()
    {
        Text = text,
        AutoSize = true,
        MinimumSize = new Size(110, 38),
        FlatStyle = FlatStyle.Flat,
        BackColor = Color.FromArgb(243, 244, 246),
        ForeColor = Color.FromArgb(17, 24, 39),
        Margin = new Padding(0, 0, 8, 0),
    };

    private void CanvasMouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;
        var point = ToImagePoint(e.Location);
        if (point is null) return;

        if (_tool == EditorTool.Text)
        {
            AddText(point.Value);
            return;
        }

        _dragStart = point.Value;
        _dragEnd = point.Value;
        _dragging = true;
    }

    private void CanvasMouseMove(object? sender, MouseEventArgs e)
    {
        if (!_dragging) return;
        var point = ToImagePoint(e.Location);
        if (point is null) return;
        _dragEnd = point.Value;
        _canvas.Invalidate();
    }

    private void CanvasMouseUp(object? sender, MouseEventArgs e)
    {
        if (!_dragging || e.Button != MouseButtons.Left) return;
        _dragging = false;
        var point = ToImagePoint(e.Location);
        if (point is not null) _dragEnd = point.Value;

        var area = NormalizeRectangle(_dragStart, _dragEnd);
        if (area.Width < 4 || area.Height < 4) return;
        SaveHistory();

        if (_tool == EditorTool.Mark)
        {
            using var graphics = Graphics.FromImage(_image);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var fill = new SolidBrush(Color.FromArgb(55, 255, 230, 0));
            using var border = new Pen(Color.FromArgb(235, 220, 38, 38), Math.Max(3, _image.Width / 550F));
            graphics.FillRectangle(fill, area);
            graphics.DrawRectangle(border, area);
        }
        else
        {
            Pixelate(area);
        }

        RefreshImage();
    }

    private void CanvasPaint(object? sender, PaintEventArgs e)
    {
        if (!_dragging) return;
        var first = ToControlPoint(_dragStart);
        var second = ToControlPoint(_dragEnd);
        var area = NormalizeRectangle(first, second);
        using var pen = new Pen(_tool == EditorTool.Blur ? Color.DeepSkyBlue : Color.Red, 2F)
        {
            DashStyle = DashStyle.Dash,
        };
        e.Graphics.DrawRectangle(pen, area);
    }

    private void AddText(Point location)
    {
        using var dialog = new TextInputForm();
        if (dialog.ShowDialog(this) != DialogResult.OK || string.IsNullOrWhiteSpace(dialog.Value)) return;
        SaveHistory();

        using var graphics = Graphics.FromImage(_image);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var fontSize = Math.Max(18F, _image.Width / 55F);
        using var font = new Font("Segoe UI", fontSize, FontStyle.Bold, GraphicsUnit.Pixel);
        using var outline = new Pen(Color.White, Math.Max(2F, fontSize / 12F)) { LineJoin = LineJoin.Round };
        using var path = new GraphicsPath();
        path.AddString(dialog.Value, font.FontFamily, (int)font.Style, font.Size, location, StringFormat.GenericDefault);
        graphics.DrawPath(outline, path);
        using var brush = new SolidBrush(Color.FromArgb(220, 38, 38));
        graphics.FillPath(brush, path);
        RefreshImage();
    }

    private void Pixelate(Rectangle area)
    {
        area.Intersect(new Rectangle(Point.Empty, _image.Size));
        var block = Math.Max(8, Math.Min(area.Width, area.Height) / 18);
        using var source = new Bitmap(_image);
        using var graphics = Graphics.FromImage(_image);
        graphics.PixelOffsetMode = PixelOffsetMode.Half;

        for (var y = area.Top; y < area.Bottom; y += block)
        for (var x = area.Left; x < area.Right; x += block)
        {
            var width = Math.Min(block, area.Right - x);
            var height = Math.Min(block, area.Bottom - y);
            var color = source.GetPixel(Math.Min(x + width / 2, source.Width - 1), Math.Min(y + height / 2, source.Height - 1));
            using var brush = new SolidBrush(color);
            graphics.FillRectangle(brush, x, y, width, height);
        }
    }

    private void SaveHistory()
    {
        _history.Push(new Bitmap(_image));
    }

    private void Undo()
    {
        if (!_history.TryPop(out var previous)) return;
        _image.Dispose();
        _image = previous;
        RefreshImage();
    }

    private void RefreshImage()
    {
        _canvas.Image = _image;
        _canvas.Invalidate();
    }

    private Point? ToImagePoint(Point point)
    {
        var bounds = GetImageBounds();
        if (!bounds.Contains(point)) return null;
        var x = (int)((point.X - bounds.Left) * (_image.Width / (double)bounds.Width));
        var y = (int)((point.Y - bounds.Top) * (_image.Height / (double)bounds.Height));
        return new Point(Math.Clamp(x, 0, _image.Width - 1), Math.Clamp(y, 0, _image.Height - 1));
    }

    private Point ToControlPoint(Point point)
    {
        var bounds = GetImageBounds();
        return new Point(
            bounds.Left + (int)(point.X * (bounds.Width / (double)_image.Width)),
            bounds.Top + (int)(point.Y * (bounds.Height / (double)_image.Height)));
    }

    private Rectangle GetImageBounds()
    {
        var ratio = Math.Min(_canvas.ClientSize.Width / (double)_image.Width, _canvas.ClientSize.Height / (double)_image.Height);
        var width = Math.Max(1, (int)(_image.Width * ratio));
        var height = Math.Max(1, (int)(_image.Height * ratio));
        return new Rectangle((_canvas.ClientSize.Width - width) / 2, (_canvas.ClientSize.Height - height) / 2, width, height);
    }

    private static Rectangle NormalizeRectangle(Point a, Point b) => Rectangle.FromLTRB(
        Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
}

internal sealed class TextInputForm : Form
{
    private readonly TextBox _input;
    public string Value => _input.Text.Trim();

    public TextInputForm()
    {
        Text = "Escrever texto";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(450, 142);
        Font = new Font("Segoe UI", 10F);

        _input = new TextBox { Location = new Point(18, 20), Size = new Size(414, 30) };
        var confirm = new Button
        {
            Text = "Adicionar",
            DialogResult = DialogResult.OK,
            Location = new Point(312, 76),
            Size = new Size(120, 38),
        };
        var cancel = new Button
        {
            Text = "Cancelar",
            DialogResult = DialogResult.Cancel,
            Location = new Point(182, 76),
            Size = new Size(120, 38),
        };
        Controls.AddRange([_input, cancel, confirm]);
        AcceptButton = confirm;
        CancelButton = cancel;
    }
}
