using Microsoft.Win32;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace GachaCalculators;

internal static class Program
{
    internal sealed record GameInfo(string Id, string Name);

    internal static readonly GameInfo[] Games =
    [
        new("genshin", "Genshin Impact — Nguyên Thạch Calculator"),
        new("hsr", "Honkai: Star Rail — Tinh Ngọc Calculator"),
        new("hi3", "Honkai Impact 3rd — Pha Lê Calculator"),
        new("zzz", "Zenless Zone Zero — Polychrome Calculator"),
        new("wuwa", "Wuthering Waves — Astrite Calculator")
    ];

    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        if (args.Length > 0 && args[0] == "--launch")
        {
            var game = Games.FirstOrDefault(g => g.Id == (args.Length > 1 ? args[1] : ""));
            if (game is null)
            {
                MessageBox.Show("Không nhận ra app game cần mở.", "Gacha Calculators", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Application.Run(new CalculatorForm(game));
            return;
        }

        if (args.Length > 0 && args[0] == "--uninstall")
        {
            Uninstall();
            return;
        }

        Application.Run(new InstallerForm());
    }

    internal static string InstallDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Programs", "Gacha Calculators");

    internal static string WebDirectory => Path.Combine(InstallDirectory, "Web");
    internal static string InstalledExe => Path.Combine(InstallDirectory, "GachaCalculators.exe");

    internal static void ExtractWebAssets(string destination)
    {
        var assembly = Assembly.GetExecutingAssembly();
        const string prefix = "WebAssets/";
        foreach (var resource in assembly.GetManifestResourceNames().Where(n => n.StartsWith(prefix, StringComparison.Ordinal)))
        {
            var relative = resource[prefix.Length..].Replace('/', Path.DirectorySeparatorChar);
            var path = Path.GetFullPath(Path.Combine(destination, relative));
            var root = Path.GetFullPath(destination) + Path.DirectorySeparatorChar;
            if (!path.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Tệp nhúng nằm ngoài thư mục cài đặt.");

            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            using var input = assembly.GetManifestResourceStream(resource)
                ?? throw new InvalidDataException($"Không đọc được tài nguyên {relative}.");
            using var output = File.Create(path);
            input.CopyTo(output);
        }
    }

    internal static void CreateShortcut(string path, string gameId, string iconPath)
    {
        var shellType = Type.GetTypeFromProgID("WScript.Shell")
            ?? throw new InvalidOperationException("Không tìm thấy Windows Script Host để tạo shortcut.");
        dynamic shell = Activator.CreateInstance(shellType)!;
        dynamic shortcut = shell.CreateShortcut(path);
        shortcut.TargetPath = InstalledExe;
        shortcut.Arguments = $"--launch {gameId}";
        shortcut.WorkingDirectory = InstallDirectory;
        shortcut.Description = Games.First(g => g.Id == gameId).Name;
        shortcut.IconLocation = iconPath;
        shortcut.Save();
        Marshal.FinalReleaseComObject(shortcut);
        Marshal.FinalReleaseComObject(shell);
    }

    internal static void RegisterUninstaller()
    {
        using var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\GachaCalculators");
        key.SetValue("DisplayName", "Gacha Calculators");
        key.SetValue("DisplayVersion", "1.0.0");
        key.SetValue("Publisher", "SaberLotus");
        key.SetValue("InstallLocation", InstallDirectory);
        key.SetValue("UninstallString", $"\"{InstalledExe}\" --uninstall");
        key.SetValue("NoModify", 1, RegistryValueKind.DWord);
        key.SetValue("NoRepair", 1, RegistryValueKind.DWord);
    }

    private static void Uninstall()
    {
        var answer = MessageBox.Show(
            "Gỡ Gacha Calculators khỏi máy? Dữ liệu bạn đã nhập được giữ lại để có thể khôi phục nếu cài lại.",
            "Gỡ cài đặt Gacha Calculators", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (answer != DialogResult.Yes) return;

        try
        {
            var menu = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs", "Gacha Calculators");
            if (Directory.Exists(menu)) Directory.Delete(menu, recursive: true);
            Registry.CurrentUser.DeleteSubKeyTree(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\GachaCalculators", throwOnMissingSubKey: false);
            var web = Path.Combine(InstallDirectory, "Web");
            if (Directory.Exists(web)) Directory.Delete(web, recursive: true);
            var exe = Path.Combine(InstallDirectory, "GachaCalculators.exe");
            var scheduled = MoveFileEx(exe, null, 0x4);
            MessageBox.Show(scheduled
                    ? "Đã gỡ app. Windows sẽ dọn tệp đang chạy sau khi bạn khởi động lại. Dữ liệu đã nhập được giữ lại."
                    : "Đã gỡ shortcut và dữ liệu app. Có thể xoá thư mục cài đặt sau khi đóng cửa sổ này:\n\n" + InstallDirectory,
                "Gỡ cài đặt", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không gỡ được hoàn toàn: " + ex.Message, "Gỡ cài đặt", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool MoveFileEx(string existingFileName, string? newFileName, uint flags);
}

internal sealed class InstallerForm : Form
{
    private readonly CheckedListBox games = new();
    private readonly Button installButton = new();

    internal InstallerForm()
    {
        Text = "Cài đặt Gacha Calculators";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = true;
        ClientSize = new Size(540, 500);

        var heading = new Label
        {
            Text = "Gacha Calculators cho Windows",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            AutoSize = false,
            Location = new Point(28, 24),
            Size = new Size(480, 38)
        };
        var intro = new Label
        {
            Text = "Chọn những game bạn muốn cài. Mỗi game có shortcut và dữ liệu riêng; app chạy offline sau khi cài.",
            Location = new Point(30, 72),
            Size = new Size(475, 44)
        };
        var listLabel = new Label { Text = "App sẽ được thêm vào Start Menu:", Location = new Point(30, 132), AutoSize = true };
        games.Location = new Point(30, 160);
        games.Size = new Size(475, 170);
        games.CheckOnClick = true;
        games.BorderStyle = BorderStyle.FixedSingle;
        foreach (var game in Program.Games) games.Items.Add(game.Name, true);

        var location = new Label
        {
            Text = "Cài đặt cho tài khoản Windows hiện tại, không cần quyền quản trị viên:\n" + Program.InstallDirectory,
            Location = new Point(30, 346),
            Size = new Size(475, 42)
        };
            var runtime = new Label
            {
            Text = "Cần Microsoft Edge WebView2 Runtime (thường đã có trên máy cài Microsoft Edge).",
            Location = new Point(30, 395),
            Size = new Size(475, 38),
            ForeColor = Color.DimGray
        };
        installButton.Text = "Cài đặt";
        installButton.Location = new Point(360, 445);
        installButton.Size = new Size(145, 36);
        installButton.Click += InstallClicked;

        Controls.AddRange([heading, intro, listLabel, games, location, runtime, installButton]);
        AcceptButton = installButton;
    }

    private void InstallClicked(object? sender, EventArgs e)
    {
        var selected = Program.Games.Where((_, i) => games.GetItemChecked(i)).ToArray();
        if (selected.Length == 0)
        {
            MessageBox.Show("Hãy chọn ít nhất một app để cài.", "Gacha Calculators", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        installButton.Enabled = false;
        try
        {
            Directory.CreateDirectory(Program.InstallDirectory);
            var currentExe = Environment.ProcessPath ?? Application.ExecutablePath;
            if (!string.Equals(Path.GetFullPath(currentExe), Path.GetFullPath(Program.InstalledExe), StringComparison.OrdinalIgnoreCase))
                File.Copy(currentExe, Program.InstalledExe, overwrite: true);
            Program.ExtractWebAssets(Program.WebDirectory);

            var menu = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs", "Gacha Calculators");
            Directory.CreateDirectory(menu);
            foreach (var game in Program.Games)
            {
                var shortcut = Path.Combine(menu, $"{game.Id} - Gacha Calculators.lnk");
                if (!selected.Contains(game))
                {
                    if (File.Exists(shortcut)) File.Delete(shortcut);
                    continue;
                }
                var icon = Path.Combine(Program.WebDirectory, game.Id, "favicon.ico");
                Program.CreateShortcut(shortcut, game.Id, icon);
            }
            Program.RegisterUninstaller();

            MessageBox.Show("Cài đặt xong. Mở app từ Start Menu. Sau cài đặt, các calculator chạy offline.",
                "Gacha Calculators", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }
        catch (Exception ex)
        {
            installButton.Enabled = true;
            MessageBox.Show("Cài đặt không thành công:\n" + ex.Message, "Gacha Calculators", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

internal sealed class CalculatorForm : Form
{
    private readonly Program.GameInfo game;
    private readonly WebView2 browser = new() { Dock = DockStyle.Fill };

    internal CalculatorForm(Program.GameInfo game)
    {
        this.game = game;
        Text = game.Name;
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(760, 600);
        Size = new Size(1180, 820);
        Controls.Add(browser);
        Shown += InitializeBrowser;
    }

    private async void InitializeBrowser(object? sender, EventArgs e)
    {
        var profile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SaberLotus", "GachaCalculators", "Profiles", game.Id);
        try
        {
            Directory.CreateDirectory(profile);
            var environment = await CoreWebView2Environment.CreateAsync(userDataFolder: profile);
            await browser.EnsureCoreWebView2Async(environment);
            browser.CoreWebView2.Settings.AreDevToolsEnabled = false;
            browser.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
            browser.CoreWebView2.Settings.IsZoomControlEnabled = false;
            browser.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "gacha.localhost", Program.WebDirectory, CoreWebView2HostResourceAccessKind.Deny);
            browser.CoreWebView2.NavigationStarting += (_, args) =>
            {
                if (!Uri.TryCreate(args.Uri, UriKind.Absolute, out var uri)) return;
                if (uri.Host.Equals("gacha.localhost", StringComparison.OrdinalIgnoreCase)) return;
                args.Cancel = true;
                try { Process.Start(new ProcessStartInfo(args.Uri) { UseShellExecute = true }); }
                catch { /* External links are optional; keep the app open if no browser is available. */ }
            };
            browser.Source = new Uri($"https://gacha.localhost/{game.Id}/");
        }
        catch (WebView2RuntimeNotFoundException)
        {
            ShowRuntimeMessage();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không mở được app:\n" + ex.Message, "Gacha Calculators", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Close();
        }
    }

    private void ShowRuntimeMessage()
    {
        var result = MessageBox.Show(
            "Máy chưa có Microsoft Edge WebView2 Runtime. Hãy cài WebView2 Runtime của Microsoft rồi mở lại app.\n\nMở trang tải chính thức?",
            "Thiếu WebView2 Runtime", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
        if (result == DialogResult.Yes)
            Process.Start(new ProcessStartInfo("https://developer.microsoft.com/microsoft-edge/webview2/") { UseShellExecute = true });
        Close();
    }
}
