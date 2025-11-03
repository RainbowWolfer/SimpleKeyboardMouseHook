using RW.Common.WPF.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace KeyboardMouseHook;

public partial class App : Application {

	public const string Token = "RW.KeyboardMouseHook.WPF";
	public const string AppName = "Keyboard Mouse Hook (By R.Wolfer)";

	private static readonly Mutex mutex;

	static App() {
		mutex = new Mutex(initiallyOwned: true, name: Token, createdNew: out bool createdNew);
		if (!createdNew) {
			Environment.Exit(0);
		}
	}

	private readonly NotifyIcon notifyIcon;

	private readonly MenuItem item_ToggleActive = new() {
		Header = "Toggle Actvie",
		IsCheckable = true,
		IsChecked = true,
	};

	private readonly MenuItem item_BlockKeyboard = new() {
		Header = "Block Keyboard",
		IsCheckable = true,
		IsChecked = false,
	};

	private readonly MenuItem item_About = new() {
		Header = "About",
	};

	private readonly MenuItem item_Exit = new() {
		Header = "Exit",
	};

	//private Window _hiddenWindow;

	public App() {
		ShutdownMode = ShutdownMode.OnExplicitShutdown;

		DispatcherUnhandledException += OnDispatcherUnhandledException;

		DispatcherTimer dispatcherTimer = new(TimeSpan.FromMilliseconds(1), DispatcherPriority.Send, Tick, Dispatcher);
		dispatcherTimer.Start();

		_proc = HookCallback;
		_hookID = SetHook(_proc);

		notifyIcon = new NotifyIcon() {
			Token = Token,
			Text = AppName,
			Icon = new BitmapImage(new Uri(@"pack://application:,,,/KeyboardMouseHook;component/AppIcon.png")),
			IsBlink = false,
			Visibility = Visibility.Visible,
		};

		ContextMenu contextMenu = new();

		contextMenu.Items.Add(item_ToggleActive);
		contextMenu.Items.Add(item_BlockKeyboard);
		contextMenu.Items.Add(new Separator());
		contextMenu.Items.Add(item_About);
		contextMenu.Items.Add(item_Exit);

		item_About.Click += Item_About_Click;
		item_Exit.Click += Item_Exit_Click;

		notifyIcon.ContextMenu = contextMenu;

		notifyIcon.Click += NotifyIconOnClick;
		notifyIcon.MouseDoubleClick += NotifyIconOnMouseDoubleClick;

		notifyIcon.Initialize();

		//_hiddenWindow = new Window {
		//	Width = 0,
		//	Height = 0,
		//	WindowStyle = WindowStyle.None,
		//	ShowInTaskbar = false,
		//	ShowActivated = false,
		//	Opacity = 0,
		//	AllowsTransparency = true,
		//	// 把它移到屏幕外
		//	Left = -10000,
		//	Top = -10000
		//};

		//_hiddenWindow.Show();
	}

	private void Item_About_Click(object sender, RoutedEventArgs e) {
		//MessageBox.Show(_hiddenWindow, "dwqdq", AppName, MessageBoxButton.OK);
		AboutDialog.ShowInstance();
	}

	private void Item_Exit_Click(object sender, RoutedEventArgs e) {
		Shutdown();
	}

	protected override void OnExit(ExitEventArgs e) {
		base.OnExit(e);
		UnhookWindowsHookEx(_hookID);
	}

	private void NotifyIconOnMouseDoubleClick(object sender, RoutedEventArgs e) {

	}

	private void NotifyIconOnClick(object sender, RoutedEventArgs e) {

	}

	private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {

	}

	private bool lastSpaceDown = false;
	private bool lastCtrlSpace = false;

	private void Tick(object? sender, EventArgs e) {
		if (item_ToggleActive.IsChecked is false) {
			return;
		}

		GetCursorPos(out POINT lpPoint);

		//bool w = (GetAsyncKeyState(VK_W) & 0x8000) != 0;
		//bool a = (GetAsyncKeyState(VK_A) & 0x8000) != 0;
		//bool s = (GetAsyncKeyState(VK_S) & 0x8000) != 0;
		//bool d = (GetAsyncKeyState(VK_D) & 0x8000) != 0;
		bool w = pressedKeys.Contains(VK_W);
		bool a = pressedKeys.Contains(VK_A);
		bool s = pressedKeys.Contains(VK_S);
		bool d = pressedKeys.Contains(VK_D);

		int x = 0;
		int y = 0;
		int offset = 10;

		if (w) {
			y -= offset;
		}

		if (a) {
			x -= offset;
		}

		if (s) {
			y += offset;
		}

		if (d) {
			x += offset;
		}

		SetCursorPos(lpPoint.X + x, lpPoint.Y + y);

		//bool space = (GetAsyncKeyState(VK_SPACE) & 0x8000) != 0;
		bool space = pressedKeys.Contains(VK_SPACE);
		if (space && !lastSpaceDown) {
			// 刚刚按下
			GetCursorPos(out POINT lpPoint2);
			mouse_event(MOUSEEVENTF_LEFTDOWN, lpPoint2.X, lpPoint2.Y, 0, 0);
		} else if (!space && lastSpaceDown) {
			// 刚刚抬起
			GetCursorPos(out POINT lpPoint2);
			mouse_event(MOUSEEVENTF_LEFTUP, lpPoint2.X, lpPoint2.Y, 0, 0);
		}

		bool ctrl = pressedKeys.Contains(VK_LCONTROL) || pressedKeys.Contains(VK_RCONTROL);

		// Ctrl+Space 组合
		bool ctrlSpace = ctrl && space;

		if (ctrlSpace && !lastCtrlSpace) {
			// 刚刚按下组合键 → 触发一次右键点击
			GetCursorPos(out POINT lpPoint3);
			mouse_event(MOUSEEVENTF_RIGHTDOWN, lpPoint3.X, lpPoint3.Y, 0, 0);
			mouse_event(MOUSEEVENTF_RIGHTUP, lpPoint3.X, lpPoint3.Y, 0, 0);
		}

		lastCtrlSpace = ctrlSpace;


		lastSpaceDown = space;
	}


	private const int VK_W = 0x57; // W
	private const int VK_A = 0x41; // A
	private const int VK_S = 0x53; // S
	private const int VK_D = 0x44; // D
	private const int VK_ESC = 0x1B; // ESC
	private const int VK_SPACE = 0x20; // 空格

	private const int VK_LCONTROL = 0xA2;
	private const int VK_RCONTROL = 0xA3;

	public const int MOUSEEVENTF_RIGHTDOWN = 0x08;
	public const int MOUSEEVENTF_RIGHTUP = 0x10;


	[DllImport("user32.dll")]
	private static extern short GetAsyncKeyState(int vKey);



	[DllImport("User32.dll")]
	private static extern bool SetCursorPos(int X, int Y);

	[DllImport("User32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GetCursorPos(out POINT lpPoint);

	[StructLayout(LayoutKind.Sequential)]
	public struct POINT {
		public int X;
		public int Y;
	}


	[DllImport("user32.dll")]
	public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

	public const int MOUSEEVENTF_LEFTDOWN = 0x02;
	public const int MOUSEEVENTF_LEFTUP = 0x04;









	#region HOOK

	[StructLayout(LayoutKind.Sequential)]
	private struct KBDLLHOOKSTRUCT {
		public uint vkCode;
		public uint scanCode;
		public uint flags;
		public uint time;
		public IntPtr dwExtraInfo;
	}

	// 全局状态表
	private static readonly HashSet<int> pressedKeys = [];

	private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
		if (nCode >= 0) {
			KBDLLHOOKSTRUCT kbData = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
			int vk = (int)kbData.vkCode;

			if (wParam == WM_KEYDOWN) {
				pressedKeys.Add(vk);
			} else if (wParam == WM_KEYUP) {
				pressedKeys.Remove(vk);
			}

			if (item_BlockKeyboard.IsChecked) {
				if (vk is VK_W or VK_A or VK_S or VK_D or VK_SPACE) {
					return 1;
				}
			}
		}

		return CallNextHookEx(_hookID, nCode, wParam, lParam);
	}


	private readonly LowLevelKeyboardProc _proc;
	private static IntPtr _hookID = IntPtr.Zero;

	private static IntPtr SetHook(LowLevelKeyboardProc proc) {
		using Process curProcess = Process.GetCurrentProcess();
		using ProcessModule? curModule = curProcess.MainModule;
		return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule!.ModuleName), 0);
	}

	private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);



	private const int WH_KEYBOARD_LL = 13;
	private const int WM_KEYDOWN = 0x0100;
	private const int WM_KEYUP = 0x0101;

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool UnhookWindowsHookEx(IntPtr hhk);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr GetModuleHandle(string lpModuleName);


	#endregion
}

