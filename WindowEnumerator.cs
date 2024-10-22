using System.Runtime.InteropServices;
using System.Text;

namespace Plugin_WindowSwitch {
  delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);
  /// <summary>
  /// Gets Windows from Processes
  /// </summary>
  public static class WindowEnumerator {
    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("USER32.DLL")]
    private static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

    [DllImport("USER32.DLL")]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("USER32.DLL")]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("USER32.DLL")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("USER32.DLL")]
    private static extern IntPtr GetShellWindow();

    /// <summary>
    /// Gets the open windows of a process from it's ID
    /// </summary>
    /// <param name="processID">ID of the process to get windows for</param>
    /// <returns>Dictionary of windows open</returns>
    public static IDictionary<IntPtr, string> GetOpenWindowsFromPID(int processID) {
      IntPtr hShellWindow = GetShellWindow();
      Dictionary<IntPtr, string> dictWindows = new Dictionary<IntPtr, string>();

      EnumWindows(delegate (IntPtr hWnd, int lParam) {
        if (hWnd == hShellWindow) return true;
        if (!IsWindowVisible(hWnd)) return true;

        int length = GetWindowTextLength(hWnd);
        if (length == 0) return true;

        uint windowPid;
        GetWindowThreadProcessId(hWnd, out windowPid);
        if (windowPid != processID) return true;

        StringBuilder stringBuilder = new StringBuilder(length);
        GetWindowText(hWnd, stringBuilder, length + 1);
        dictWindows.Add(hWnd, stringBuilder.ToString());
        return true;
      }, 0);

      return dictWindows;
    }
  }

}
