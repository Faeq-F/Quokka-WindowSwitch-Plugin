
using Quokka.ListItems;
using Quokka.PluginArch;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Plugin_WindowSwitch {
  /// <summary>
  /// The Window Switch Plugin
  /// </summary>
  public class WindowSwitch : Plugin {

    private List<window> openWindows = new();
    [DllImport("user32.dll")]
    private static extern bool IsIconic(IntPtr hWnd);

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override string PluggerName { get; set; } = "WindowSwitch";

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="query"><inheritdoc/></param>
    /// <returns></returns>
    public override List<ListItem> OnQueryChange(string query) {
      List<ListItem> items = new();
      foreach (window w in openWindows) {
        items.Add(new WindowItem(w));
      }
      return items;
    }

    /// <summary>
    /// Indexes all open windows
    /// </summary>
    public override void OnSearchWindowStartup() {
      openWindows = new();
      Process[] currentProcesses = Process.GetProcesses();
      foreach (Process process in currentProcesses) {
        // If the process appears on the Taskbar (if has a title)
        if (!String.IsNullOrEmpty(process.MainWindowTitle)) {
          Dictionary<IntPtr, string> windows = (Dictionary<IntPtr, string>) WindowEnumerator.GetOpenWindowsFromPID(process.Id);
          IntPtr mainWindowHandle = IntPtr.Zero;
          foreach (KeyValuePair<IntPtr, string> pair in windows) {
            mainWindowHandle = pair.Key;
            if (mainWindowHandle != IntPtr.Zero) {
              openWindows.Add(new window(process.ProcessName, process.MainWindowTitle, mainWindowHandle, process.Id));
            }
          }
        }
      }
    }
  }

  internal class window {
    public IntPtr handle;
    public int ID;
    public String ProcessName;
    public String Title;
    internal window(String ProcessName, String Title, IntPtr handle, int ID) {
      this.ProcessName = ProcessName;
      this.Title = Title;
      this.handle = handle;
      this.ID = ID;
    }
  }

}
