using System.Drawing;

namespace CapturaRapida.Native;

internal sealed class MainForm : Form
{
    private const int HotkeyId = 0xCA71;

    private readonly AppConfig _config;
    private readonly NotifyIcon _notifyIcon;
    private readonly CheckBox _controlCheckBox;
    private readonly CheckBox _altCheckBox;
    private readonly CheckBox _shiftCheckBox;
    private readonly CheckBox _winCheckBox;
    private readonly ComboBox _keyComboBox;
    private readonly CheckBox _startWithWindowsCheckBox;
    private readonly Label _shortcutPreviewLabel;
    private readonly Label _statusLabel;

    private bool _registered;
    private bool _exiting;
    private bool _capturing;

    public MainForm()
    {
        _config = ConfigStore.Load();

        Text = "Captura Rápida";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = true;
        ClientSize = new Size(560, 470);
        BackColor = Color.White;
        Font = new Font("Segoe UI", 10F);
        Icon = SystemIcons.Application;

        var titleLabel = new Label
        {
            AutoSize = true,
            Text = "Captura Rápida para Windows",
            Font = new Font("Segoe UI", 18F, FontStyle.Bold),
            ForeColor = Color.FromArgb(17, 24, 39),
            Location = new Point(28, 25),
        };

        var descriptionLabel = new Label
        {
            AutoSize = false,
            Text = "Use um atalho global para capturar o monitor onde o cursor estiver. A imagem será copiada automaticamente para a área de transferência.",
            ForeColor = Color.FromArgb(80, 92, 108),
            Location = new Point(30, 68),
            Size = new Size(500, 52),
        };

        var hotkeyGroup = new GroupBox
        {
            Text = "Atalho global",
            Location = new Point(28, 128),
            Size = new Size(504, 150),
            Padding = new Padding(18),
        };

        _controlCheckBox = CreateModifierCheckBox("Ctrl", 22);
        _altCheckBox = CreateModifierCheckBox("Alt", 105);
        _shiftCheckBox = CreateModifierCheckBox("Shift", 180);
        _winCheckBox = CreateModifierCheckBox("Win", 270);

        _keyComboBox = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(360, 32),
            Size = new Size(115, 31),
        };

        _keyComboBox.Items.AddRange(CreateHotkeyOptions().Cast<object>().ToArray());

        _shortcutPreviewLabel = new Label
        {
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = Color.FromArgb(8, 102, 229),
            BackColor = Color.FromArgb(237, 244, 255),
            Location = new Point(22, 83),
            Size = new Size(453, 42),
        };

        hotkeyGroup.Controls.AddRange([
            _controlCheckBox,
            _altCheckBox,
            _shiftCheckBox,
            _winCheckBox,
            _keyComboBox,
            _shortcutPreviewLabel,
        ]);

        _startWithWindowsCheckBox = new CheckBox
        {
            AutoSize = true,
            Text = "Iniciar automaticamente com o Windows",
            Location = new Point(32, 298),
        };

        var mouseHintLabel = new Label
        {
            AutoSize = false,
            Text = "No software do mouse, configure o botão desejado para enviar exatamente o atalho mostrado acima.",
            ForeColor = Color.FromArgb(80, 92, 108),
            Location = new Point(32, 331),
            Size = new Size(495, 42),
        };

        var testButton = new Button
        {
            Text = "Testar captura",
            Location = new Point(29, 387),
            Size = new Size(145, 42),
            FlatStyle = FlatStyle.Flat,
        };
        testButton.FlatAppearance.BorderColor = Color.FromArgb(150, 162, 178);
        testButton.Click += async (_, _) => await CaptureNowAsync();

        var saveButton = new Button
        {
            Text = "Salvar e ocultar",
            Location = new Point(359, 387),
            Size = new Size(173, 42),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(8, 102, 229),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
        };
        saveButton.FlatAppearance.BorderColor = Color.FromArgb(8, 102, 229);
        saveButton.Click += (_, _) => SaveSettings();

        _statusLabel = new Label
        {
            AutoSize = false,
            ForeColor = Color.FromArgb(20, 128, 74),
            TextAlign = ContentAlignment.MiddleLeft,
            Location = new Point(29, 438),
            Size = new Size(503, 24),
        };

        Controls.AddRange([
            titleLabel,
            descriptionLabel,
            hotkeyGroup,
            _startWithWindowsCheckBox,
            mouseHintLabel,
            testButton,
            saveButton,
            _statusLabel,
        ]);

        var trayMenu = new ContextMenuStrip();
        trayMenu.Items.Add("Capturar agora", null, async (_, _) => await CaptureNowAsync());
        trayMenu.Items.Add("Configurações", null, (_, _) => ShowSettings());
        trayMenu.Items.Add(new ToolStripSeparator());
        trayMenu.Items.Add("Sair", null, (_, _) => ExitApplication());

        _notifyIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "Captura Rápida",
            Visible = true,
            ContextMenuStrip = trayMenu,
        };
        _notifyIcon.DoubleClick += (_, _) => ShowSettings();

        LoadSettingsIntoControls();

        foreach (var checkBox in new[] { _controlCheckBox, _altCheckBox, _shiftCheckBox, _winCheckBox })
        {
            checkBox.CheckedChanged += (_, _) => UpdateShortcutPreview();
        }
        _keyComboBox.SelectedIndexChanged += (_, _) => UpdateShortcutPreview();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);

        RegisterConfiguredHotkey(showError: true);

        if (_config.SetupCompleted)
        {
            Hide();
            _notifyIcon.ShowBalloonTip(
                2500,
                "Captura Rápida está ativo",
                $"Use {HotkeyFormatter.Format(_config.Modifiers, _config.Key)} para capturar.",
                ToolTipIcon.Info);
        }
    }

    protected override void WndProc(ref Message message)
    {
        if (message.Msg == NativeMethods.WmHotkey && message.WParam.ToInt32() == HotkeyId)
        {
            _ = CaptureNowAsync();
        }

        base.WndProc(ref message);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (!_exiting && e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            Hide();
            return;
        }

        UnregisterCurrentHotkey();
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        base.OnFormClosing(e);
    }

    private static CheckBox CreateModifierCheckBox(string text, int left) => new()
    {
        AutoSize = true,
        Text = text,
        Location = new Point(left, 36),
    };

    private static IReadOnlyList<HotkeyOption> CreateHotkeyOptions()
    {
        var options = new List<HotkeyOption>();

        for (var value = (int)Keys.A; value <= (int)Keys.Z; value++)
        {
            var key = (Keys)value;
            options.Add(new HotkeyOption(key.ToString(), key));
        }

        for (var value = (int)Keys.F1; value <= (int)Keys.F12; value++)
        {
            var key = (Keys)value;
            options.Add(new HotkeyOption(key.ToString(), key));
        }

        options.Add(new HotkeyOption("Print Screen", Keys.PrintScreen));
        return options;
    }

    private void LoadSettingsIntoControls()
    {
        _controlCheckBox.Checked = _config.Modifiers.HasFlag(HotkeyModifiers.Control);
        _altCheckBox.Checked = _config.Modifiers.HasFlag(HotkeyModifiers.Alt);
        _shiftCheckBox.Checked = _config.Modifiers.HasFlag(HotkeyModifiers.Shift);
        _winCheckBox.Checked = _config.Modifiers.HasFlag(HotkeyModifiers.Win);
        _startWithWindowsCheckBox.Checked = _config.StartWithWindows;

        var selectedIndex = _keyComboBox.Items
            .Cast<HotkeyOption>()
            .ToList()
            .FindIndex(option => option.Key == _config.Key);

        _keyComboBox.SelectedIndex = selectedIndex >= 0 ? selectedIndex : 15;
        UpdateShortcutPreview();
    }

    private HotkeyModifiers GetSelectedModifiers()
    {
        var modifiers = HotkeyModifiers.None;
        if (_controlCheckBox.Checked) modifiers |= HotkeyModifiers.Control;
        if (_altCheckBox.Checked) modifiers |= HotkeyModifiers.Alt;
        if (_shiftCheckBox.Checked) modifiers |= HotkeyModifiers.Shift;
        if (_winCheckBox.Checked) modifiers |= HotkeyModifiers.Win;
        return modifiers;
    }

    private Keys GetSelectedKey() => (_keyComboBox.SelectedItem as HotkeyOption)?.Key ?? Keys.P;

    private void UpdateShortcutPreview()
    {
        _shortcutPreviewLabel.Text = HotkeyFormatter.Format(GetSelectedModifiers(), GetSelectedKey());
    }

    private void SaveSettings()
    {
        var newModifiers = GetSelectedModifiers();
        var newKey = GetSelectedKey();

        if (newModifiers == HotkeyModifiers.None)
        {
            ShowStatus("Escolha pelo menos um modificador: Ctrl, Alt, Shift ou Win.", isError: true);
            return;
        }

        var previousModifiers = _config.Modifiers;
        var previousKey = _config.Key;

        UnregisterCurrentHotkey();

        if (!TryRegisterHotkey(newModifiers, newKey))
        {
            TryRegisterHotkey(previousModifiers, previousKey);
            ShowStatus("Esse atalho já está em uso. Escolha outra combinação.", isError: true);
            return;
        }

        try
        {
            StartupService.SetEnabled(_startWithWindowsCheckBox.Checked);

            _config.Modifiers = newModifiers;
            _config.Key = newKey;
            _config.StartWithWindows = _startWithWindowsCheckBox.Checked;
            _config.SetupCompleted = true;
            ConfigStore.Save(_config);

            ShowStatus("Configurações salvas. O aplicativo continuará na bandeja.", isError: false);
            _notifyIcon.ShowBalloonTip(
                2500,
                "Captura Rápida configurado",
                $"Use {HotkeyFormatter.Format(newModifiers, newKey)} para capturar.",
                ToolTipIcon.Info);
            Hide();
        }
        catch (Exception exception)
        {
            ShowStatus($"Não foi possível salvar: {exception.Message}", isError: true);
        }
    }

    private void RegisterConfiguredHotkey(bool showError)
    {
        if (TryRegisterHotkey(_config.Modifiers, _config.Key)) return;

        if (showError)
        {
            Show();
            ShowStatus("O atalho configurado já está em uso. Escolha outro.", isError: true);
        }
    }

    private bool TryRegisterHotkey(HotkeyModifiers modifiers, Keys key)
    {
        _registered = NativeMethods.RegisterHotKey(
            Handle,
            HotkeyId,
            (uint)(modifiers | HotkeyModifiers.NoRepeat),
            (uint)key);

        return _registered;
    }

    private void UnregisterCurrentHotkey()
    {
        if (!_registered) return;
        NativeMethods.UnregisterHotKey(Handle, HotkeyId);
        _registered = false;
    }

    private async Task CaptureNowAsync()
    {
        if (_capturing) return;
        _capturing = true;

        try
        {
            if (Visible)
            {
                Hide();
                await Task.Delay(180);
            }

            using var bitmap = CaptureService.CaptureMonitorUnderCursor();
            CaptureService.CopyImageToClipboard(bitmap);

            _statusLabel.Text = $"Captura copiada às {DateTime.Now:HH:mm:ss}.";
            _statusLabel.ForeColor = Color.FromArgb(20, 128, 74);
            _notifyIcon.ShowBalloonTip(
                1800,
                "Captura copiada",
                "A imagem do monitor sob o cursor está na área de transferência.",
                ToolTipIcon.Info);
        }
        catch (Exception exception)
        {
            _notifyIcon.ShowBalloonTip(
                3500,
                "Falha na captura",
                exception.Message,
                ToolTipIcon.Error);
        }
        finally
        {
            _capturing = false;
        }
    }

    private void ShowSettings()
    {
        Show();
        WindowState = FormWindowState.Normal;
        Activate();
        BringToFront();
    }

    private void ShowStatus(string message, bool isError)
    {
        _statusLabel.Text = message;
        _statusLabel.ForeColor = isError
            ? Color.FromArgb(195, 59, 50)
            : Color.FromArgb(20, 128, 74);
    }

    private void ExitApplication()
    {
        _exiting = true;
        Close();
    }
}
