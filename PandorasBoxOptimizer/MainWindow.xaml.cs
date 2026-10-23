using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PandorasBoxOptimizer;

public partial class MainWindow : Window
{
    private Profile? _selected;
    private bool _userPicked;
    private readonly List<(Border card, Border check, Border reco, Profile p)> _cards = new();
    private static Brush B(string key) => (Brush)Application.Current.Resources[key];
    private static FontFamily F(string key) => (FontFamily)Application.Current.Resources[key];

    public MainWindow()
    {
        InitializeComponent();
        BuildCards();
        LblYourPc.Text = Track("YOUR PC");
        BtnApply.IsEnabled = false;   // stays off until the scan + config check finish
        Loaded += (_, _) => { EnableDarkTitleBar(); StartScan(); };
        SetupShotMode();
    }

    // WPF has no letter-spacing; the site wide-tracks its uppercase micro-labels
    // (~0.18em), so insert a thin space between letters to match that look.
    private static string Track(string s)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < s.Length; i++)
        {
            if (s[i] == ' ') { sb.Append(' ').Append(' '); continue; } // wide word gap
            sb.Append(s[i]);
            if (i < s.Length - 1 && s[i + 1] != ' ') sb.Append(' ');          // thin per-letter track
        }
        return sb.ToString();
    }

    // ---------- cards ----------

    private void BuildCards()
    {
        for (int i = 0; i < Profiles.All.Count; i++)
        {
            var p = Profiles.All[i];
            CardsPanel.Children.Add(MakeCard(p, i == 0 ? new Thickness(0, 0, 6, 0) : new Thickness(6, 0, 0, 0)));
        }
        // No card is highlighted until the scan recommends one (avoids a selection jump).
    }

    private Border MakeCard(Profile p, Thickness margin)
    {
        var check = new Border
        {
            Width = 20, Height = 20, CornerRadius = new CornerRadius(10), Background = B("Brand"),
            VerticalAlignment = VerticalAlignment.Center, Visibility = Visibility.Hidden,
            Child = new TextBlock { Text = "✓", Foreground = Brushes.White, FontSize = 11.5,
                FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center }
        };

        var reco = new Border
        {
            CornerRadius = new CornerRadius(5), Background =
                (Brush)new BrushConverter().ConvertFromString("#143B82F6")!,
            BorderBrush = (Brush)new BrushConverter().ConvertFromString("#4D3B82F6")!,
            BorderThickness = new Thickness(1), Padding = new Thickness(6, 2, 6, 2),
            Margin = new Thickness(0, 10, 0, 0), HorizontalAlignment = HorizontalAlignment.Left,
            Visibility = Visibility.Collapsed,
            Child = new TextBlock { Text = Track("RECOMMENDED FOR YOU"), FontSize = 8.5,
                Foreground = (Brush)new BrushConverter().ConvertFromString("#FF9CC3FF")!,
                FontFamily = F("BodyBold") }
        };

        var head = new Grid();
        head.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        head.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var name = new TextBlock { Text = p.Name, Foreground = B("Ink"), FontFamily = F("Display"), FontSize = 17, VerticalAlignment = VerticalAlignment.Center };
        name.LayoutTransform = new ScaleTransform(0.97, 1);  // tighten display tracking like the site
        Grid.SetColumn(name, 0); Grid.SetColumn(check, 1);
        head.Children.Add(name); head.Children.Add(check);

        var blurb = new TextBlock { Text = p.Blurb, Foreground = B("Muted"), FontFamily = F("Body"),
            FontSize = 12, TextWrapping = TextWrapping.Wrap, LineHeight = 17, Margin = new Thickness(0, 8, 0, 0) };

        var stack = new StackPanel();
        stack.Children.Add(head);
        stack.Children.Add(blurb);
        stack.Children.Add(reco);

        var card = new Border
        {
            CornerRadius = new CornerRadius(12), Background = B("Card"), BorderBrush = B("Line"),
            BorderThickness = new Thickness(1), Padding = new Thickness(16, 15, 16, 16),
            Margin = margin, Cursor = System.Windows.Input.Cursors.Hand, Child = stack, Tag = p
        };
        card.MouseLeftButtonUp += (_, _) => { _userPicked = true; Select(p, card); };
        card.MouseEnter += (_, _) => { if (_selected != p) card.Background = B("CardHover"); };
        card.MouseLeave += (_, _) => { if (_selected != p) card.Background = B("Card"); };
        _cards.Add((card, check, reco, p));
        return card;
    }

    private void Select(Profile p, Border sel)
    {
        _selected = p;
        foreach (var (card, check, _, _) in _cards)
        {
            bool on = ReferenceEquals(card, sel);
            card.BorderBrush = on ? B("Brand") : B("Line");
            card.BorderThickness = new Thickness(on ? 1.5 : 1);
            card.Background = on ? B("CardHover") : B("Card");
            check.Visibility = on ? Visibility.Visible : Visibility.Hidden;
            card.Effect = on ? new DropShadowEffect { Color = (Color)Application.Current.Resources["BrandColor"], BlurRadius = 18, ShadowDepth = 0, Opacity = 0.3 } : null;
        }
    }

    // ---------- real system scan + animated reveal ----------

    private void StartScan()
    {
        SystemInfo info;
        try { info = SystemScan.Read(); }
        catch { Reveal(null, Profiles.All[0]); return; }

        bool balanced = SystemScan.RecommendBalanced(info);
        var reco = Profiles.All.First(p => p.Id == (balanced ? "balanced" : "maxfps"));

        string[] steps = { "Detecting graphics card…", "Reading memory…", "Checking your display…" };
        int i = 0;
        ScanLabel.Text = steps[0];
        var t = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(380) };
        t.Tick += (_, _) =>
        {
            i++;
            if (i < steps.Length) { ScanLabel.Text = steps[i]; }
            else { t.Stop(); Reveal(info, reco); }
        };
        t.Start();
    }

    private void Reveal(SystemInfo? info, Profile reco)
    {
        if (info is not null)
        {
            SpecGpu.Text = info.Gpu;
            SpecSub.Text = $"{CpuShort(info.Cpu)}  ·  {info.RamText}  ·  {info.DisplayText}";
            SpecSub.TextTrimming = TextTrimming.CharacterEllipsis;
            RecoText.Text = Track($"BEST FIT · {reco.Name.ToUpperInvariant()}");
        }
        else
        {
            SpecGpu.Text = "Couldn't read full specs";
            SpecSub.Text = "You can still pick a profile below.";
            RecoPill.Visibility = Visibility.Collapsed;
        }

        ScanState.Visibility = Visibility.Collapsed;
        ResultState.Visibility = Visibility.Visible;

        // Highlight + select the recommended profile.
        var match = _cards.FirstOrDefault(c => c.p.Id == reco.Id);
        if (match.card is not null)
        {
            match.reco.Visibility = Visibility.Visible;
            if (!_userPicked) Select(reco, match.card);   // don't override a manual pick made mid-scan
        }

        Detect();
    }

    private static string CpuShort(string c)
    {
        c = Regex.Replace(c, @"\(R\)|\(TM\)|\bCPU\b|\bProcessor\b", "", RegexOptions.IgnoreCase);
        c = Regex.Replace(c, @"@.*$", "");
        return Regex.Replace(c, @"\s+", " ").Trim();
    }

    // ---------- apply / restore ----------

    private void OnApply(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        if (Process.GetProcesses().Any(pr => Safe(pr).Contains("cod", StringComparison.OrdinalIgnoreCase)
                                           || Safe(pr).Contains("modernwarfare", StringComparison.OrdinalIgnoreCase)))
        {
            SetStatus(B("Faint"), "Close Call of Duty first, then apply again.");
            return;
        }
        try
        {
            var r = ConfigEngine.Apply(_selected);
            SetStatus(r.Ok ? B("Brand") : B("Faint"),
                r.Ok ? $"✓  {_selected.Name} applied · backup saved. Launch MW4, click No if asked to reset."
                     : r.Message.Split('\n')[0]);
        }
        catch (Exception ex) { SetStatus(B("Faint"), ex.Message); }
    }

    private void OnRestore(object sender, RoutedEventArgs e)
    {
        try { var r = ConfigEngine.RestoreLast(); SetStatus(r.Ok ? B("Brand") : B("Faint"), r.Message.Split('\n')[0]); }
        catch (Exception ex) { SetStatus(B("Faint"), ex.Message); }
    }

    private void Detect()
    {
        var files = ConfigEngine.FindConfigFiles();
        if (files.Count > 0) { SetStatus(B("Faint"), "Backs up your current settings first."); BtnApply.IsEnabled = true; }
        else { SetStatus(B("Faint"), "Launch MW4 once to create its settings, then reopen."); BtnApply.IsEnabled = false; }
    }

    private void SetStatus(Brush color, string text) { StatusText.Foreground = color; StatusText.Text = text; }
    private static string Safe(Process p) { try { return p.ProcessName; } catch { return ""; } }

    // ---------- dark title bar ----------

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int val, int size);
    private void EnableDarkTitleBar()
    {
        try { int on = 1; DwmSetWindowAttribute(new WindowInteropHelper(this).Handle, 20, ref on, sizeof(int)); }
        catch { }
    }

    // ---------- verification render mode: --shot <path> ----------

    private void SetupShotMode()
    {
        var args = Environment.GetCommandLineArgs();
        int si = Array.IndexOf(args, "--shot");
        if (si < 0 || si + 1 >= args.Length) return;
        string shot = args[si + 1];
        int delay = (si + 2 < args.Length && int.TryParse(args[si + 2], out int d)) ? d : 2000;
        ContentRendered += (_, _) =>
        {
            var t = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(delay) };
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
