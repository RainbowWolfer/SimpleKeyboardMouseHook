using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

namespace KeyboardMouseHookDemo2;
public partial class App : Application
{

	public App()
	{
		ShutdownMode = ShutdownMode.OnExplicitShutdown;

		DispatcherTimer dispatcherTimer = new(TimeSpan.FromMilliseconds(1), DispatcherPriority.Send, Tick, Dispatcher);
		dispatcherTimer.Start();
	}

	private void Tick(object? sender, EventArgs e)
	{
		GetCursorPos(out POINT lpPoint);

		bool w = (GetAsyncKeyState(VK_W) & 0x8000) != 0;
		bool a = (GetAsyncKeyState(VK_A) & 0x8000) != 0;
		bool s = (GetAsyncKeyState(VK_S) & 0x8000) != 0;
		bool d = (GetAsyncKeyState(VK_D) & 0x8000) != 0;

		int x = 0;
		int y = 0;
		int offset = 10;

		if (w)
		{
			y -= offset;
		}

		if (a)
		{
			x -= offset;
		}

		if (s)
		{
			y += offset;
		}

		if (d)
		{
			x += offset;
		}

		SetCursorPos(lpPoint.X + x, lpPoint.Y + y);
	}


	private const int VK_W = 0x57; // W
	private const int VK_A = 0x41; // A
	private const int VK_S = 0x53; // S
	private const int VK_D = 0x44; // D
	private const int VK_ESC = 0x1B; // ESC

	[DllImport("user32.dll")]
	private static extern short GetAsyncKeyState(int vKey);



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

