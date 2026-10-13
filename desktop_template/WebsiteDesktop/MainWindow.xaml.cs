
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WebsiteDesktop;

public partial class MainWindow : Window
{
    private string _startUrl = "https://example.com/";
    private bool _isFullscreen;
    private WindowStyle _previousWindowStyle;
    private WindowState _previousWindowState;
    private ResizeMode _previousResizeMode;

    public MainWindow()
    {
        InitializeComponent();

        Loaded += MainWindow_Loaded;
        Browser.NavigationCompleted += Browser_NavigationCompleted;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            LoadConfiguration();

            Title = AppTitle.Text;

            await Browser.EnsureCoreWebView2Async();

            Browser.CoreWebView2.NavigationStarting += (_, _) =>
            {
                LoadingPanel.Visibility = Visibility.Visible;
            };

            Browser.CoreWebView2.Navigate(_startUrl);
        }
        catch (Exception ex)
        {
            LoadingPanel.Visibility = Visibility.Visible;

            MessageBox.Show(
                "Could not initialize the embedded browser.\n\n" + ex.Message,
                "Browser initialization error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void LoadConfiguration()
    {
        string configPath = Path.Combine(
            AppContext.BaseDirectory,
            "appsettings.json");

        if (!File.Exists(configPath))
        {
            ApplyBrandingDefaults();
            return;
        }

        using JsonDocument document =
            JsonDocument.Parse(File.ReadAllText(configPath));

        JsonElement root = document.RootElement;

        if (root.TryGetProperty("AppName", out JsonElement name)
            && name.ValueKind == JsonValueKind.String
            && !string.IsNullOrWhiteSpace(name.GetString()))
        {
            AppTitle.Text = name.GetString()!;
        }

        if (root.TryGetProperty("StartUrl", out JsonElement url)
            && url.ValueKind == JsonValueKind.String
            && Uri.TryCreate(url.GetString(), UriKind.Absolute, out Uri? parsed)
            && parsed.Scheme == Uri.UriSchemeHttps)
        {
            _startUrl = parsed.AbsoluteUri;
        }

        ApplyBranding(root);

        if (root.TryGetProperty("Fullscreen", out JsonElement fullscreen)
            && fullscreen.ValueKind == JsonValueKind.True)
        {
            SetFullscreen(true);
        }
    }

    private void ApplyBrandingDefaults()
    {
        ApplyBrandColor("#173A63");
    }

    private void ApplyBranding(JsonElement root)
    {
        string GetSetting(string key, string fallback)
        {
            if (root.TryGetProperty(key, out JsonElement value)
                && value.ValueKind == JsonValueKind.String
                && !string.IsNullOrWhiteSpace(value.GetString()))
            {
                return value.GetString()!;
            }

            return fallback;
        }

        string brandColor = GetSetting("BrandColor", "#173A63");
        string logoPath = GetSetting(
            "LogoPath", "Assets/splash-logo.png");
        string iconPath = GetSetting(
            "IconPath", "Assets/app.ico");

        ApplyBrandColor(brandColor);

        string fullLogoPath = ResolveAssetPath(logoPath);

        if (File.Exists(fullLogoPath))
        {
            SplashImage.Source = new BitmapImage(
                new Uri(fullLogoPath, UriKind.Absolute));
        }

        string fullIconPath = ResolveAssetPath(iconPath);

        if (File.Exists(fullIconPath))
        {
            Icon = new BitmapImage(
                new Uri(fullIconPath, UriKind.Absolute));
        }
    }

    private void ApplyBrandColor(string value)
    {
        try
        {
            var brush = (Brush)new BrushConverter()
                .ConvertFromString(value)!;

            ToolbarBorder.Background = brush;
            LoadingText.Foreground = brush;
        }
        catch
        {
            var defaultBrush = new SolidColorBrush(
                Color.FromRgb(23, 58, 99));

            ToolbarBorder.Background = defaultBrush;
            LoadingText.Foreground = defaultBrush;
        }
    }

    private static string ResolveAssetPath(string relativePath)
    {
        string normalized = relativePath
            .Replace('/', Path.DirectorySeparatorChar);

        return Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, normalized));
    }

    private void Browser_NavigationCompleted(
        object? sender,
        CoreWebView2NavigationCompletedEventArgs e)
    {
        LoadingPanel.Visibility = Visibility.Collapsed;

        if (!e.IsSuccess)
        {
            MessageBox.Show(
                $"The website could not be loaded. Error: {e.WebErrorStatus}",
                "Navigation error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    private void Back_Click(object sender, RoutedEventArgs e)
    {
        if (Browser.CoreWebView2?.CanGoBack == true)
            Browser.CoreWebView2.GoBack();
    }

    private void Forward_Click(object sender, RoutedEventArgs e)
    {
        if (Browser.CoreWebView2?.CanGoForward == true)
            Browser.CoreWebView2.GoForward();
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        Browser.CoreWebView2?.Reload();
    }

    private void Fullscreen_Click(object sender, RoutedEventArgs e)
    {
        SetFullscreen(!_isFullscreen);
    }

    private void SetFullscreen(bool enabled)
    {
        if (enabled && !_isFullscreen)
        {
            _previousWindowStyle = WindowStyle;
            _previousWindowState = WindowState;
            _previousResizeMode = ResizeMode;

            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            WindowState = WindowState.Maximized;
            _isFullscreen = true;
        }
        else if (!enabled && _isFullscreen)
        {
            WindowStyle = _previousWindowStyle;
            ResizeMode = _previousResizeMode;
            WindowState = _previousWindowState;
            _isFullscreen = false;
        }
    }
}
