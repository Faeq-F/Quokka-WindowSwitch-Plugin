using Quokka;
using Quokka.ListItems;
using Quokka.PluginArch;
using System.Runtime.InteropServices;

namespace PluginWindowSwitch
{
  class WindowItem : ListItem
  {


    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    readonly IntPtr WindowHandle;

    public WindowItem(Window window)
    {
      WindowHandle = window.handle;
      Name = $"{window.ProcessName} | {window.Title}";
      Description = $"Process: {window.ProcessName} | PID: {window.ID}";
      Icon = IconCache.GetOrAdd(
        Environment.CurrentDirectory + "\\PlugBoard\\PluginWindowSwitch\\Plugin\\switchTo.png"
      );
    }

    //When item is selected, open the window
    public override void Execute()
    {
      ShowWindow(WindowHandle, 9);
      SetForegroundWindow(WindowHandle);
      App.Current.MainWindow.Close();
    }
  }
}
