
using Newtonsoft.Json;
using Quokka.ListItems;
using Quokka.PluginArch;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using WinCopies.Util;

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

    private static Settings pluginSettings = new();
    internal static Settings PluginSettings { get => pluginSettings; set => pluginSettings = value; }

    /// <summary>
    /// Loads plugin settings
    /// </summary>
    public WindowSwitch() {
      string fileName = Environment.CurrentDirectory + "\\PlugBoard\\Plugin_WindowSwitch\\Plugin\\settings.json";
      PluginSettings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(fileName))!;
    }


    private List<ListItem> ProduceItems(string query) {
      List<ListItem> items = new();
      foreach (window w in openWindows) {
        if (( w.ProcessName.Contains(query, StringComparison.OrdinalIgnoreCase)
          || w.Title.Contains(query, StringComparison.OrdinalIgnoreCase)
                || FuzzySearch.LD(w.ProcessName, query) < PluginSettings.FuzzySearchThreshold
                || FuzzySearch.LD(w.Title, query) < PluginSettings.FuzzySearchThreshold ) && !PluginSettings.BlackList.Contains(w.Title)
                && !PluginSettings.BlackList.Contains(w.ProcessName)
                ) {
          items.Add(new WindowItem(w));
        }
      }
      return items;
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="query"><inheritdoc/></param>
    /// <returns>Matching windows that do not have their ProcessName or Title in the blacklist</returns>
    public override List<ListItem> OnQueryChange(string query) { return ProduceItems(query); }

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



    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns>
    /// The WindowSwitchSignifier from plugin settings
    /// </returns>
    public override List<string> CommandSignifiers() {
      return new List<string>() { PluginSettings.WindowSwitchSignifier };
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="command">The WindowSwitchSignifier (Since there is only 1 signifier for this plugin), followed by the window being searched for</param>
    /// <returns>List of windows that possibly match what is being searched for (windows that do not have their ProcessName or Title in the blacklist)</returns>
    public override List<ListItem> OnSignifier(string command) {
      command = command.Substring(PluginSettings.WindowSwitchSignifier.Length);
      return ProduceItems(command);
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns>The AllWindowsSpecialCommand from plugin settings</returns>
    public override List<string> SpecialCommands() {
      List<string> SpecialCommand = new() {
        PluginSettings.AllWindowsSpecialCommand
      };
      return SpecialCommand;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="command">The AllWindowsSpecialCommand (Since there is only 1 special command for this plugin)</param>
    /// <returns>All windows (that do not have their ProcessName or Title in the blacklist) sorted alphabetically</returns>
    public override List<ListItem> OnSpecialCommand(string command) {
      List<ListItem> AllList = new();
      foreach (window w in openWindows) {
        if (!PluginSettings.BlackList.Contains(w.Title)
                && !PluginSettings.BlackList.Contains(w.ProcessName)) {
          AllList.Add(new WindowItem(w));
        }
      }
      AllList = AllList.OrderBy(x => x.Name).ToList();
      return AllList;
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



