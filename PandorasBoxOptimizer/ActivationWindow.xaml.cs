using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace PandorasBoxOptimizer;

public partial class ActivationWindow : Window
{
    public ActivationWindow()
    {
        InitializeComponent();
        Loaded += (_, _) => { EnableDarkTitleBar(); Start(); };
    }

    private void Start()
    {
        var args = Environment.GetCommandLineArgs();
        if (args.Contains("--shotlogin")) { ShowEntry(); SetupShot(args); return; }

        var saved = License.SavedCode();
        if (saved is null) { ShowEntry(); return; }

        // Re-check a saved code online (honors revoke / machine move). If offline, let them in.
        Checking.Visibility = Visibility.Visible;
        Entry.Visibility = Visibility.Collapsed;
        _ = Reverify(saved);
    }

    private async Task Reverify(string code)
    {
        var r = await License.VerifyAsync(code);
        if (r.Ok || (r.Reason?.StartsWith("network") ?? false)) { OpenMain(); return; }
        License.Clear();
        ShowEntry();
        ErrText.Text = License.FriendlyReason(r.Reason);
    }

    private void ShowEntry()
    {
        Checking.Visibility = Visibility.Collapsed;
        Entry.Visibility = Visibility.Visible;
        CodeBox.Focus();
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) OnActivate(sender, e);
    }

    private async void OnActivate(object sender, RoutedEventArgs e)
    {
        string code = CodeBox.Text.Trim();
        if (code.Length < 5) { ErrText.Text = "Enter your code."; return; }

        BtnActivate.IsEnabled = false;
        BtnActivate.Content = "Activating…";
        ErrText.Text = "";

        var r = await License.VerifyAsync(code);
        if (r.Ok) { License.Save(code); OpenMain(); return; }

        ErrText.Text = License.FriendlyReason(r.Reason);
        BtnActivate.IsEnabled = true;
        BtnActivate.Content = "Activate";
    }

    private void OpenMain()
    {
        var main = new MainWindow();
        Application.Current.MainWindow = main;
        main.Show();
        Close();
    }

    // ---- dark title bar ----
    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int val, int size);
    private void EnableDarkTitleBar()
    {
        try { int on = 1; DwmSetWindowAttribute(new WindowInteropHelper(this).Handle, 20, ref on, sizeof(int)); }
        catch { }
    }

    // ---- verification render mode: --shotlogin <path> ----
    private void SetupShot(string[] args)
    {
        int si = Array.IndexOf(args, "--shotlogin");
        if (si < 0 || si + 1 >= args.Length) return;
        string shot = args[si + 1];
        ContentRendered += (_, _) =>
        {
            var t = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            t.Tick += (_, _) =>
            {
                t.Stop();
                try
                {
                    var rtb = new System.Windows.Media.Imaging.RenderTargetBitmap((int)ActualWidth, (int)ActualHeight, 96, 96, PixelFormats.Pbgra32);
                    rtb.Render(this);
                    var enc = new System.Windows.Media.Imaging.PngBitmapEncoder();
                    enc.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(rtb));
                    using var fs = System.IO.File.Create(shot);
                    enc.Save(fs);
                }
                catch { }
                Application.Current.Shutdown();
            };
            t.Start();
        };
    }
}
