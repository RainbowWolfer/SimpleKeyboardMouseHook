using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace KeyBoardMouseHookWFDemo;

[SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "<Pending>")]
[SuppressMessage("Interoperability", "SYSLIB1054:Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time", Justification = "<Pending>")]
public class MethodWindowsHook : IMethod
{

	private bool isControlDown = false;

	private readonly IntPtr hookID;

	public MethodWindowsHook()
	{
		hookID = SetHook(HookCallback);
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
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
			if ((Keys)vkCode is Keys.LControlKey or Keys.RControlKey)
			{
				if (wParam == WM_KEYDOWN)
				{
					isControlDown = true;
					//Cursor.Position = new Point(Cursor.Position.X + 10, Cursor.Position.Y);
				}
				else if (wParam == WM_KEYUP)
				{
					isControlDown = false;
					//Cursor.Position = new Point(Cursor.Position.X - 10, Cursor.Position.Y);
				}
			}
			else if (wParam == WM_KEYDOWN)
			{
				double speed = 10;
				if (isControlDown)
				{
					speed *= 3;
				}
				int _speed = (int)speed;
				switch ((Keys)vkCode)
				{
					case Keys.W:
						//up
						Cursor.Position = new Point(Cursor.Position.X, Cursor.Position.Y - _speed);
						break;
					case Keys.S:
						//down
						Cursor.Position = new Point(Cursor.Position.X, Cursor.Position.Y + _speed);
						break;
					case Keys.A:
						//left
						Cursor.Position = new Point(Cursor.Position.X - _speed, Cursor.Position.Y);
						break;
					case Keys.D:
						//right
						Cursor.Position = new Point(Cursor.Position.X + _speed, Cursor.Position.Y);
						break;
				}
			}
		}
		return CallNextHookEx(hookID, nCode, wParam, lParam);
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
}
