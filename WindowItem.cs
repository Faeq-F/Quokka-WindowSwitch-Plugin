using Quokka.ListItems;
using Quokka.PluginArch;
using System.Runtime.InteropServices;
using System.Windows;

namespace PluginWindowSwitch
{
  internal sealed class WindowItem : ListItem
  {


    [DllImport("user32.dll")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private readonly IntPtr WindowHandle;

    public WindowItem(Window window)
    {
      WindowHandle = window.Handle;
      Name = $"{window.ProcessName} | {window.Title}";
      Description = $"Process: {window.ProcessName} | PID: {window.Id}";
      Icon = IconCache.GetOrAdd(
        Environment.CurrentDirectory + "\\PlugBoard\\PluginWindowSwitch\\Plugin\\switchTo.png"
      );
    }

    //When item is selected, open the window
    public override void Execute()
    {
      ShowWindow(WindowHandle, 9);
      SetForegroundWindow(WindowHandle);
      Application.Current.MainWindow.Close();
    }
  }
}
