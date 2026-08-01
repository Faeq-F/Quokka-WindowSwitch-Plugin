using System.Runtime.InteropServices;

namespace PluginWindowSwitch
{
  internal delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);
  /// <summary>
  /// Gets Windows from Processes
  /// </summary>
  public static class WindowEnumerator
  {
    [DllImport("user32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("USER32.DLL")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

    [DllImport("USER32.DLL", CharSet = CharSet.Unicode, SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern int GetWindowText(IntPtr hWnd, [Out] char[] lpString, int nMaxCount);

    [DllImport("USER32.DLL")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("USER32.DLL")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("USER32.DLL")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern IntPtr GetShellWindow();

    /// <summary>
    /// Gets the open windows of a process from it's ID
    /// </summary>
    /// <param name="processID">ID of the process to get windows for</param>
    /// <returns>Dictionary of windows open</returns>
    public static IDictionary<IntPtr, string> GetOpenWindowsFromPID(int processID)
    {
      IntPtr hShellWindow = GetShellWindow();
      Dictionary<IntPtr, string> dictWindows = new();

#pragma warning disable RCS1163
      EnumWindows((hWnd, lParam) =>
# pragma warning restore RCS1163
      {
        if (hWnd == hShellWindow)
        {
          return true;
        }
        if (!IsWindowVisible(hWnd))
        {
          return true;
        }

        int length = GetWindowTextLength(hWnd);
        if (length == 0)
        {
          return true;
        }

        _ = GetWindowThreadProcessId(hWnd, out uint windowPid);
        if (windowPid != (uint)processID)
        {
          return true;
        }

        char[] buffer = new char[length + 1];
        _ = GetWindowText(hWnd, buffer, buffer.Length);
        string title = new(buffer, 0, length);
        dictWindows.Add(hWnd, title);
        return true;
      }, 0);

      return dictWindows;
    }
  }

}
