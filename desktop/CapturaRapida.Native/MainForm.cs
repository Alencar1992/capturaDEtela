using System.Drawing;

namespace CapturaRapida.Native;

internal sealed class MainForm : Form
{
    private const int HotkeyId = 0xCA71;
    private const int SelectionHotkeyId = 0xCA72;

    private readonly AppConfig _config;
    private readonly NotifyIcon _notifyIcon;
    private readonly CheckBox _controlCheckBox;
    private readonly CheckBox _altCheckBox;
    private readonly CheckBox _shiftCheckBox;
    private readonly CheckBox _winCheckBox;
    private readonly ComboBox _keyComboBox;
    private readonly CheckBox _selectionControlCheckBox;
    private readonly CheckBox _selectionAltCheckBox;
    private readonly CheckBox _selectionShiftCheckBox;
    private readonly CheckBox _selectionWinCheckBox;
    private readonly ComboBox _selectionKeyComboBox;
    private readonly Label _selectionShortcutPreviewLabel;
    private readonly CheckBox _startWithWindowsCheckBox;
    private readonly CheckBox _saveToFileCheckBox;
    private readonly TextBox _saveDirectoryTextBox;
    private readonly Button _browseDirectoryButton;
    private readonly Label _shortcutPreviewLabel;
    private readonly Label _statusLabel;

    private bool _registered;
    private bool _selectionRegistered;
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
        ClientSize = new Size(560, 740);
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
            Text = "Configure uma tecla única ou uma combinação para cada tipo de captura. A imagem será copiada automaticamente.",
            ForeColor = Color.FromArgb(80, 92, 108),
            Location = new Point(30, 68),
            Size = new Size(500, 52),
        };

        var hotkeyGroup = new GroupBox
        {
            Text = "Atalho — monitor inteiro",
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

        var selectionHotkeyGroup = new GroupBox
        {
            Text = "Atalho — área selecionada",
            Location = new Point(28, 286),
            Size = new Size(504, 150),
            Padding = new Padding(18),
        };

        _selectionControlCheckBox = CreateModifierCheckBox("Ctrl", 22);
        _selectionAltCheckBox = CreateModifierCheckBox("Alt", 105);
        _selectionShiftCheckBox = CreateModifierCheckBox("Shift", 180);
        _selectionWinCheckBox = CreateModifierCheckBox("Win", 270);
        _selectionKeyComboBox = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(360, 32),
            Size = new Size(115, 31),
        };
        _selectionKeyComboBox.Items.AddRange(CreateHotkeyOptions().Cast<object>().ToArray());
        _selectionShortcutPreviewLabel = new Label
        {
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = Color.FromArgb(8, 102, 229),
            BackColor = Color.FromArgb(237, 244, 255),
            Location = new Point(22, 83),
            Size = new Size(453, 42),
        };
        selectionHotkeyGroup.Controls.AddRange([
            _selectionControlCheckBox,
            _selectionAltCheckBox,
            _selectionShiftCheckBox,
            _selectionWinCheckBox,
            _selectionKeyComboBox,
            _selectionShortcutPreviewLabel,
        ]);

        _startWithWindowsCheckBox = new CheckBox
        {
            AutoSize = true,
            Text = "Iniciar automaticamente com o Windows",
            Location = new Point(32, 456),
        };

        _saveToFileCheckBox = new CheckBox
        {
            AutoSize = true,
            Text = "Salvar também uma cópia em PNG",
            Location = new Point(32, 489),
        };

        _saveDirectoryTextBox = new TextBox
        {
            Location = new Point(32, 523),
            Size = new Size(402, 29),
        };

        _browseDirectoryButton = new Button
        {
            Text = "Pasta...",
            Location = new Point(443, 521),
            Size = new Size(89, 33),
            FlatStyle = FlatStyle.Flat,
        };
        _browseDirectoryButton.Click += (_, _) => ChooseSaveDirectory();
        _saveToFileCheckBox.CheckedChanged += (_, _) => UpdateSaveControls();

        var mouseHintLabel = new Label
        {
            AutoSize = false,
            Text = "Os modificadores são opcionais: sem marcação, o atalho usa uma tecla única. No software do mouse, associe cada botão ao atalho escolhido.",
            ForeColor = Color.FromArgb(80, 92, 108),
            Location = new Point(32, 569),
            Size = new Size(495, 42),
        };

        var testButton = new Button
        {
            Text = "Testar tela inteira",
            Location = new Point(29, 645),
            Size = new Size(145, 42),
            FlatStyle = FlatStyle.Flat,
        };
        testButton.FlatAppearance.BorderColor = Color.FromArgb(150, 162, 178);
        testButton.Click += async (_, _) => await CaptureNowAsync();

        var testSelectionButton = new Button
        {
            Text = "Testar seleção",
            Location = new Point(184, 645),
            Size = new Size(145, 42),
            FlatStyle = FlatStyle.Flat,
        };
        testSelectionButton.FlatAppearance.BorderColor = Color.FromArgb(150, 162, 178);
        testSelectionButton.Click += async (_, _) => await CaptureSelectionAsync();

        var saveButton = new Button
        {
            Text = "Salvar e ocultar",
            Location = new Point(359, 645),
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
            Location = new Point(29, 696),
            Size = new Size(503, 24),
        };

        Controls.AddRange([
            titleLabel,
            descriptionLabel,
            hotkeyGroup,
            selectionHotkeyGroup,
            _startWithWindowsCheckBox,
            _saveToFileCheckBox,
            _saveDirectoryTextBox,
            _browseDirectoryButton,
            mouseHintLabel,
            testButton,
            testSelectionButton,
            saveButton,
            _statusLabel,
        ]);

        var trayMenu = new ContextMenuStrip();
        trayMenu.Items.Add("Capturar monitor inteiro", null, async (_, _) => await CaptureNowAsync());
        trayMenu.Items.Add("Capturar área selecionada", null, async (_, _) => await CaptureSelectionAsync());
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
        foreach (var checkBox in new[] { _selectionControlCheckBox, _selectionAltCheckBox, _selectionShiftCheckBox, _selectionWinCheckBox })
        {
            checkBox.CheckedChanged += (_, _) => UpdateSelectionShortcutPreview();
        }
        _selectionKeyComboBox.SelectedIndexChanged += (_, _) => UpdateSelectionShortcutPreview();
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
                $"Tela: {HotkeyFormatter.Format(_config.Modifiers, _config.Key)} | Área: {HotkeyFormatter.Format(_config.SelectionModifiers, _config.SelectionKey)}",
                ToolTipIcon.Info);
        }
    }

    protected override void WndProc(ref Message message)
    {
        if (message.Msg == NativeMethods.WmHotkey && message.WParam.ToInt32() == HotkeyId)
        {
            _ = CaptureNowAsync();
        }
        else if (message.Msg == NativeMethods.WmHotkey && message.WParam.ToInt32() == SelectionHotkeyId)
        {
            _ = CaptureSelectionAsync();
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
        _saveToFileCheckBox.Checked = _config.SaveToFile;
        _saveDirectoryTextBox.Text = _config.SaveDirectory;
        UpdateSaveControls();

        var selectedIndex = _keyComboBox.Items
            .Cast<HotkeyOption>()
            .ToList()
            .FindIndex(option => option.Key == _config.Key);

        _keyComboBox.SelectedIndex = selectedIndex >= 0 ? selectedIndex : 15;
        UpdateShortcutPreview();

        _selectionControlCheckBox.Checked = _config.SelectionModifiers.HasFlag(HotkeyModifiers.Control);
        _selectionAltCheckBox.Checked = _config.SelectionModifiers.HasFlag(HotkeyModifiers.Alt);
        _selectionShiftCheckBox.Checked = _config.SelectionModifiers.HasFlag(HotkeyModifiers.Shift);
        _selectionWinCheckBox.Checked = _config.SelectionModifiers.HasFlag(HotkeyModifiers.Win);
        var selectionIndex = _selectionKeyComboBox.Items.Cast<HotkeyOption>().ToList()
            .FindIndex(option => option.Key == _config.SelectionKey);
        _selectionKeyComboBox.SelectedIndex = selectionIndex >= 0 ? selectionIndex : 18;
        UpdateSelectionShortcutPreview();
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

    private Keys GetSelectedKey() => (_keyComboBox.SelectedItem as HotkeyOption)?.Key ?? Keys.F8;

    private HotkeyModifiers GetSelectionModifiers()
    {
        var modifiers = HotkeyModifiers.None;
        if (_selectionControlCheckBox.Checked) modifiers |= HotkeyModifiers.Control;
        if (_selectionAltCheckBox.Checked) modifiers |= HotkeyModifiers.Alt;
        if (_selectionShiftCheckBox.Checked) modifiers |= HotkeyModifiers.Shift;
        if (_selectionWinCheckBox.Checked) modifiers |= HotkeyModifiers.Win;
        return modifiers;
    }

    private Keys GetSelectionKey() => (_selectionKeyComboBox.SelectedItem as HotkeyOption)?.Key ?? Keys.F9;

    private void UpdateShortcutPreview()
    {
        _shortcutPreviewLabel.Text = HotkeyFormatter.Format(GetSelectedModifiers(), GetSelectedKey());
    }

    private void UpdateSelectionShortcutPreview()
    {
        _selectionShortcutPreviewLabel.Text = HotkeyFormatter.Format(GetSelectionModifiers(), GetSelectionKey());
    }

    private void UpdateSaveControls()
    {
        _saveDirectoryTextBox.Enabled = _saveToFileCheckBox.Checked;
        _browseDirectoryButton.Enabled = _saveToFileCheckBox.Checked;
    }

    private void ChooseSaveDirectory()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Escolha ou crie a pasta onde as capturas serão salvas",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = true,
            SelectedPath = Directory.Exists(_saveDirectoryTextBox.Text)
                ? _saveDirectoryTextBox.Text
                : Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _saveDirectoryTextBox.Text = dialog.SelectedPath;
        }
    }

    private void SaveSettings()
    {
        var newModifiers = GetSelectedModifiers();
        var newKey = GetSelectedKey();
        var newSelectionModifiers = GetSelectionModifiers();
        var newSelectionKey = GetSelectionKey();

        if (newModifiers == newSelectionModifiers && newKey == newSelectionKey)
        {
            ShowStatus("Os dois tipos de captura precisam usar atalhos diferentes.", isError: true);
            return;
        }

        if (_saveToFileCheckBox.Checked && string.IsNullOrWhiteSpace(_saveDirectoryTextBox.Text))
        {
            ShowStatus("Escolha uma pasta para salvar as capturas.", isError: true);
            return;
        }

        var previousModifiers = _config.Modifiers;
        var previousKey = _config.Key;
        var previousSelectionModifiers = _config.SelectionModifiers;
        var previousSelectionKey = _config.SelectionKey;

        UnregisterCurrentHotkey();

        if (!TryRegisterHotkey(newModifiers, newKey) || !TryRegisterSelectionHotkey(newSelectionModifiers, newSelectionKey))
        {
            UnregisterCurrentHotkey();
            TryRegisterHotkey(previousModifiers, previousKey);
            TryRegisterSelectionHotkey(previousSelectionModifiers, previousSelectionKey);
            ShowStatus("Um dos atalhos já está em uso. Escolha outra combinação.", isError: true);
            return;
        }

        try
        {
            StartupService.SetEnabled(_startWithWindowsCheckBox.Checked);

            _config.Modifiers = newModifiers;
            _config.Key = newKey;
            _config.SelectionModifiers = newSelectionModifiers;
            _config.SelectionKey = newSelectionKey;
            _config.StartWithWindows = _startWithWindowsCheckBox.Checked;
            _config.SaveToFile = _saveToFileCheckBox.Checked;
            _config.SaveDirectory = _saveDirectoryTextBox.Text.Trim();
            _config.SetupCompleted = true;
            ConfigStore.Save(_config);

            ShowStatus("Configurações salvas. O aplicativo continuará na bandeja.", isError: false);
            _notifyIcon.ShowBalloonTip(
                2500,
                "Captura Rápida configurado",
                $"Tela: {HotkeyFormatter.Format(newModifiers, newKey)} | Área: {HotkeyFormatter.Format(newSelectionModifiers, newSelectionKey)}",
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
        var fullScreenOk = TryRegisterHotkey(_config.Modifiers, _config.Key);
        var selectionOk = TryRegisterSelectionHotkey(_config.SelectionModifiers, _config.SelectionKey);
        if (fullScreenOk && selectionOk) return;

        if (showError)
        {
            Show();
            ShowStatus("Um dos atalhos configurados já está em uso. Escolha outro.", isError: true);
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

    private bool TryRegisterSelectionHotkey(HotkeyModifiers modifiers, Keys key)
    {
        _selectionRegistered = NativeMethods.RegisterHotKey(
            Handle,
            SelectionHotkeyId,
            (uint)(modifiers | HotkeyModifiers.NoRepeat),
            (uint)key);
        return _selectionRegistered;
    }

    private void UnregisterCurrentHotkey()
    {
        if (_registered)
        {
            NativeMethods.UnregisterHotKey(Handle, HotkeyId);
            _registered = false;
        }
        if (_selectionRegistered)
        {
            NativeMethods.UnregisterHotKey(Handle, SelectionHotkeyId);
            _selectionRegistered = false;
        }
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
            CompleteCapture(bitmap, "A imagem do monitor sob o cursor está na área de transferência.");
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

    private async Task CaptureSelectionAsync()
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

            using var virtualScreen = CaptureService.CaptureVirtualScreen();
            using var selector = new SelectionForm(virtualScreen);
            if (selector.ShowDialog() != DialogResult.OK || selector.SelectedArea.IsEmpty) return;

            using var selectedImage = CaptureService.Crop(virtualScreen, selector.SelectedArea);
            CompleteCapture(selectedImage, "A área selecionada está na área de transferência.");
        }
        catch (Exception exception)
        {
            _notifyIcon.ShowBalloonTip(3500, "Falha na captura selecionada", exception.Message, ToolTipIcon.Error);
        }
        finally
        {
            _capturing = false;
        }
    }

    private void CompleteCapture(Image image, string clipboardMessage)
    {
        CaptureService.CopyImageToClipboard(image);
        string? savedPath = null;
        if (_config.SaveToFile) savedPath = CaptureService.SavePng(image, _config.SaveDirectory);

        _statusLabel.Text = savedPath is null
            ? $"Captura copiada às {DateTime.Now:HH:mm:ss}."
            : $"Captura copiada e salva às {DateTime.Now:HH:mm:ss}.";
        _statusLabel.ForeColor = Color.FromArgb(20, 128, 74);
        _notifyIcon.ShowBalloonTip(
            1800,
            savedPath is null ? "Captura copiada" : "Captura copiada e salva",
            savedPath is null ? clipboardMessage : $"Arquivo salvo em {savedPath}",
            ToolTipIcon.Info);
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
