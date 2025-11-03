using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

namespace KeyboardMouseHookDemo;

public partial class MainWindow : Window
{


	public MainWindow()
	{
		InitializeComponent();
		_hookID = SetHook(_proc);
	}

	protected override void OnClosed(EventArgs e)
	{
		UnhookWindowsHookEx(_hookID);
		base.OnClosed(e);
	}

	private static LowLevelKeyboardProc _proc = HookCallback;
	private static IntPtr _hookID = IntPtr.Zero;

	private static IntPtr SetHook(LowLevelKeyboardProc proc)
	{
		using Process curProcess = Process.GetCurrentProcess();
		using ProcessModule? curModule = curProcess.MainModule;
		return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
	}

	private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

	private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
	{
		if (nCode >= 0 && wParam == WM_KEYDOWN)
		{
			int vkCode = Marshal.ReadInt32(lParam);
			GetCursorPos(out POINT lpPoint);
			switch (vkCode)
			{
				case 87://w
					SetCursorPos(lpPoint.X, lpPoint.Y - 10);
					break;
				case 65://a
					SetCursorPos(lpPoint.X - 10, lpPoint.Y);
					break;
				case 83://s
					SetCursorPos(lpPoint.X, lpPoint.Y + 10);
					break;
				case 68://d
					SetCursorPos(lpPoint.X + 10, lpPoint.Y);
					break;
				default:
					break;
			}
			Debug.WriteLine(vkCode);

			//switch ((Keys)vkCode) {
			//	case Keys.Up:
			//		Cursor.Position = new System.Drawing.Point(Cursor.Position.X, Cursor.Position.Y - 10);
			//		break;
			//	case Keys.Down:
			//		Cursor.Position = new System.Drawing.Point(Cursor.Position.X, Cursor.Position.Y + 10);
			//		break;
			//	case Keys.Left:
			//		Cursor.Position = new System.Drawing.Point(Cursor.Position.X - 10, Cursor.Position.Y);
			//		break;
			//	case Keys.Right:
			//		Cursor.Position = new System.Drawing.Point(Cursor.Position.X + 10, Cursor.Position.Y);
			//		break;
			//}
		}
		return CallNextHookEx(_hookID, nCode, wParam, lParam);
	}

	private const int WH_KEYBOARD_LL = 13;
	private const int WM_KEYDOWN = 0x0100;

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool UnhookWindowsHookEx(IntPtr hhk);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr GetModuleHandle(string lpModuleName);


	[DllImport("User32.dll")]
	private static extern bool SetCursorPos(int X, int Y);

	[DllImport("User32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GetCursorPos(out POINT lpPoint);

	[StructLayout(LayoutKind.Sequential)]
	public struct POINT
	{
		public int X;
		public int Y;
	}
}