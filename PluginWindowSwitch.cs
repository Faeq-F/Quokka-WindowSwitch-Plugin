using Newtonsoft.Json;
using Quokka.ListItems;
using Quokka.PluginArch;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace PluginWindowSwitch
{
  /// <summary>
  /// The Window Switch Plugin
  /// </summary>
  public class WindowSwitch : Plugin
  {

    private List<Window> openWindows = new();
    [DllImport("user32.dll")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern bool IsIconic(IntPtr hWnd);

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override string PluginName { get; set; } = "Window Switch";

    /// <summary>
    /// Gets the icon for the window switch plugin.
    /// </summary>
    public override System.Windows.Media.ImageSource? PluginIcon =>
      IconCache.GetOrAdd(Path.Combine(Environment.CurrentDirectory, "PlugBoard", "PluginWindowSwitch", "Plugin", "switchTo.png"));

    internal static Settings PluginSettings { get; set; } = new();

    /// <summary>
    /// Loads plugin settings
    /// </summary>
    public WindowSwitch()
    {
      string fileName = Environment.CurrentDirectory + "\\PlugBoard\\PluginWindowSwitch\\Plugin\\settings.json";
      PluginSettings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(fileName))!;
    }

    private Collection<ListItem> ProduceItems(string query)
    {
      return new Collection<ListItem>(

      FuzzySearch.SearchAll(query, new Collection<string>(openWindows.ConvertAll(x => x.ProcessName)), PluginSettings.FuzzySearchThreshold).Concat(

        FuzzySearch.SearchAll(query, new Collection<string>(openWindows.ConvertAll(x => x.Title)), PluginSettings.FuzzySearchThreshold)

        ).Where(x => !PluginSettings.BlackList.Contains(x.Value))
        .Select(x => (ListItem)new WindowItem(openWindows[x.Index])
      ).Distinct().ToList());
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="query"><inheritdoc/></param>
    /// <returns>Matching windows that do not have their ProcessName or Title in the blacklist</returns>
    public override Collection<ListItem> OnQueryChange(string query) { return ProduceItems(query); }

    /// <summary>
    /// Indexes all open windows
    /// </summary>
    public override void OnSearchWindowStartup()
    {
      openWindows = new();
      Process[] currentProcesses = Process.GetProcesses();
      foreach (Process process in currentProcesses)
      {
        // If the process appears on the Taskbar (if has a title)
        if (!string.IsNullOrEmpty(process.MainWindowTitle))
        {
          Dictionary<IntPtr, string> windows = (Dictionary<IntPtr, string>)WindowEnumerator.GetOpenWindowsFromPID(process.Id);
          IntPtr mainWindowHandle = IntPtr.Zero;
          foreach (KeyValuePair<IntPtr, string> pair in windows)
          {
            mainWindowHandle = pair.Key;
            if (mainWindowHandle != IntPtr.Zero)
            {
              openWindows.Add(new Window(process.ProcessName, process.MainWindowTitle, mainWindowHandle, process.Id));
            }
          }
        }
      }
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns>
    /// The WindowSwitchSignifier from plugin settings
    /// </returns>
    public override Collection<string> CommandSignifiers()
    {
      return new Collection<string>() { PluginSettings.WindowSwitchSignifier };
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="command">The WindowSwitchSignifier (Since there is only 1 signifier for this plugin), followed by the window being searched for</param>
    /// <returns>Collection of windows that possibly match what is being searched for (windows that do not have their ProcessName or Title in the blacklist)</returns>
    public override Collection<ListItem> OnSignifier(string command)
    {
      command ??= "";
      command = command.Substring(PluginSettings.WindowSwitchSignifier.Length);
      return FuzzySearch.Sort(command, ProduceItems(command));
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns>The AllWindowsSpecialCommand from plugin settings</returns>
    public override Collection<string> SpecialCommands()
    {
      Collection<string> SpecialCommand = new() {
        PluginSettings.AllWindowsSpecialCommand
      };
      return SpecialCommand;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="command">The AllWindowsSpecialCommand (Since there is only 1 special command for this plugin)</param>
    /// <returns>All windows (that do not have their ProcessName or Title in the blacklist) sorted alphabetically</returns>
    public override Collection<ListItem> OnSpecialCommand(string command)
    {
      List<ListItem> AllList = new();
      foreach (Window w in openWindows)
      {
        if (!PluginSettings.BlackList.Contains(w.Title)
                && !PluginSettings.BlackList.Contains(w.ProcessName))
        {
          AllList.Add(new WindowItem(w));
        }
      }
      return new Collection<ListItem>(AllList.OrderBy(x => x.Name).ToList());
    }
  }

  internal sealed class Window
  {
    public IntPtr Handle { get; }
    public int Id { get; }
    public string ProcessName { get; }
    public string Title { get; }
    internal Window(string processName, string title, IntPtr handle, int id)
    {
      ProcessName = processName;
      Title = title;
      Handle = handle;
      Id = id;
    }
  }

}
