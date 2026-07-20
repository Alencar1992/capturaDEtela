using System.Drawing;

namespace CapturaRapida.Native;

internal sealed class CapturePreviewForm : Form
{
    private Bitmap _image;
    private readonly PictureBox _preview;

    public CapturePreviewForm(Image image)
    {
        _image = new Bitmap(image);

        Text = "Captura realizada";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(720, 500);
        Size = new Size(980, 680);
        TopMost = true;
        BackColor = Color.FromArgb(245, 247, 250);
        Font = new Font("Segoe UI", 10F);

        _preview = new PictureBox
        {
            Dock = DockStyle.Fill,
            Image = _image,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.FromArgb(28, 33, 41),
        };

        var editButton = CreateButton("Editar print", Color.FromArgb(8, 102, 229), Color.White);
        editButton.Click += (_, _) => EditImage();

        var finishButton = CreateButton("Concluir", Color.FromArgb(22, 163, 74), Color.White);
        finishButton.Click += (_, _) => Close();

        var hint = new Label
        {
            AutoSize = true,
            Text = "A captura já foi copiada. Edite ou clique em Concluir.",
            ForeColor = Color.FromArgb(75, 85, 99),
            Margin = new Padding(0, 11, 18, 0),
        };

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 64,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(14, 11, 14, 8),
            BackColor = Color.White,
        };
        actions.Controls.Add(finishButton);
        actions.Controls.Add(editButton);
        actions.Controls.Add(hint);

        Controls.Add(_preview);
        Controls.Add(actions);
    }

    public Bitmap GetFinalImage() => new(_image);

    protected override void Dispose(bool disposing)
    {
        if (disposing) _image.Dispose();
        base.Dispose(disposing);
    }

    private void EditImage()
    {
        using var editor = new ImageEditorForm(_image);
        if (editor.ShowDialog(this) != DialogResult.OK) return;

        var updated = editor.GetEditedImage();
        _preview.Image = updated;
        _image.Dispose();
        _image = updated;
    }

    private static Button CreateButton(string text, Color background, Color foreground) => new()
    {
        Text = text,
        AutoSize = true,
        MinimumSize = new Size(125, 38),
        BackColor = background,
        ForeColor = foreground,
        FlatStyle = FlatStyle.Flat,
        Margin = new Padding(8, 0, 0, 0),
    };
}
