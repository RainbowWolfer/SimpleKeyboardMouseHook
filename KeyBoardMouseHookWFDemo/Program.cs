namespace KeyBoardMouseHookWFDemo;

internal static class Program {

	private const string APP_NAME = "KeyBoardMouseHookWFDemo";

	[STAThread]
	static void Main() {
		Mutex mutex = new(true, APP_NAME, out bool createdNew);
		try {

			if (!createdNew) {
				mutex?.ReleaseMutex();
				mutex?.Dispose();
				return;
			}

			ApplicationConfiguration.Initialize();

			using IMethod method = new MethodTimer();

			InitializeNotifyIcon();

			Application.Run();
		} finally {
			mutex.ReleaseMutex();
			mutex.Dispose();
		}
	}

	private static void InitializeNotifyIcon() {
		ContextMenuStrip contextMenuStrip = new();
		contextMenuStrip.Items.Add("Exit", null, Exit_Click);

		NotifyIcon notifyIcon = new() {
			Icon = new Icon("AppIcon.ico"),
			ContextMenuStrip = contextMenuStrip,
			Visible = true,
			Text = "KeyBoard Mouse Hook WF Demo",
		};
		notifyIcon.DoubleClick += NotifyIcon_DoubleClick;
	}

	private static void NotifyIcon_DoubleClick(object? sender, EventArgs e) {

	}

	private static void Exit_Click(object? sender, EventArgs e) {
		Application.Exit();
	}

}