using Quokka;
using Quokka.ListItems;
using Quokka.PluginArch;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

namespace Plugin_WindowSwitch {
  class WindowItem : ListItem {


    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    IntPtr WindowHandle;

    public WindowItem(window window) {
      WindowHandle = window.handle;
      Name = $"{window.ProcessName} | {window.Title}";
      Description = $"Process: {window.ProcessName} | PID: {window.ID}";
      UiDispatcher.BeginInvoke(() => {
        Icon = new BitmapImage(new Uri(
            Environment.CurrentDirectory + "\\PlugBoard\\Plugin_WindowSwitch\\Plugin\\switchTo.png"));
      });
    }

    //When item is selected, open the window
    public override void Execute() {
      ShowWindow(WindowHandle, 9);
      SetForegroundWindow(WindowHandle);
      App.Current.MainWindow.Close();
    }
  }
}