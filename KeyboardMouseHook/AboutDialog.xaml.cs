using RW.Common.WPF.Extensions;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Navigation;

namespace SimpleKeyboardMouseHook;

public partial class AboutDialog : Window {
    private static AboutDialog? instance;

    public static void ShowInstance() {
        instance ??= new();
        instance.ShowAndActivate();
    }

    public AboutDialog() {
        InitializeComponent();
        Title = App.AppName;
        Closed += AboutDialog_Closed;

		Version version = Assembly.GetExecutingAssembly().GetName().Version!;
		string versionString = version.ToString(); // 输出类似 "1.0.0.0"
		VersionText.Text = $"v{versionString}";

        FrameworkText.Text = $"Using {RuntimeInformation.FrameworkDescription}";
	}

    private void AboutDialog_Closed(object? sender, EventArgs e) {
        instance = null;
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) {
        e.Handled = true;

        try {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        } catch (Exception ex) {
            Debug.WriteLine(ex);
        }
    }
}
