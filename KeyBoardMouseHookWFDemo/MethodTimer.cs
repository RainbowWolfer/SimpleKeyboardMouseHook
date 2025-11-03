using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Timers;
using Timer = System.Timers.Timer;

namespace KeyBoardMouseHookWFDemo;

[SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "<Pending>")]
[SuppressMessage("Interoperability", "SYSLIB1054:Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time", Justification = "<Pending>")]
internal class MethodTimer : IMethod
{

	private readonly Timer timer;

	private bool isControlDown = false;

	private readonly IntPtr hookID;

	public MethodTimer()
	{
		timer = new Timer(TimeSpan.FromMilliseconds(10));
		timer.Elapsed += OnTimedEvent;
		timer.AutoReset = true;
		timer.Enabled = true;
		hookID = SetHook(HookCallback);
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
		timer.Stop();
		timer.Dispose();
		UnhookWindowsHookEx(hookID);
	}

	private static IntPtr SetHook(LowLevelKeyboardProc proc)
	{
		using Process curProcess = Process.GetCurrentProcess();
		using ProcessModule? curModule = curProcess.MainModule;
		return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule!.ModuleName), 0);
	}

	private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

	private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
	{
		if (nCode >= 0)
		{
			int vkCode = Marshal.ReadInt32(lParam);
			Keys key = (Keys)vkCode;
			if (key is Keys.LControlKey or Keys.RControlKey)
			{
				if (wParam == WM_KEYDOWN)
				{
					isControlDown = true;
					return 1;
				}
				else if (wParam == WM_KEYUP)
				{
					isControlDown = false;
					return 1;
				}
			}
			else if (wParam == WM_KEYDOWN && key == Keys.Space)
			{
				MouseActionSimulator.Click(Cursor.Position.X, Cursor.Position.Y);
				return 1;
			}
		}
		return CallNextHookEx(hookID, nCode, wParam, lParam);
	}

	private void OnTimedEvent(object? source, ElapsedEventArgs e)
	{
		double speed = 10;
		if (isControlDown)
		{
			speed *= 3;
		}
		int _speed = (int)speed;

		//up
		if (GetAsyncKeyState(Keys.W) < 0)
		{
			Cursor.Position = new Point(Cursor.Position.X, Cursor.Position.Y - _speed);
		}

		//down
		if (GetAsyncKeyState(Keys.S) < 0)
		{
			Cursor.Position = new Point(Cursor.Position.X, Cursor.Position.Y + _speed);
		}

		//left
		if (GetAsyncKeyState(Keys.A) < 0)
		{
			Cursor.Position = new Point(Cursor.Position.X - _speed, Cursor.Position.Y);
		}

		//right
		if (GetAsyncKeyState(Keys.D) < 0)
		{
			Cursor.Position = new Point(Cursor.Position.X + _speed, Cursor.Position.Y);
		}
	}


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

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern IntPtr GetModuleHandle(string lpModuleName);

	[DllImport("user32.dll")]
	public static extern short GetAsyncKeyState(Keys vKey);
}
