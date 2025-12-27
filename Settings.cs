namespace PluginWindowSwitch
{


  /// <summary>
  ///   The settings for this (Gitmoji) plugin
  /// </summary>
  public class Settings
  {

    /// <summary>
    /// The command to see all windows (defaults to "AllWindows")
    /// </summary>
    public string AllWindowsSpecialCommand { get; set; } = "AllWindows";
    /// <summary>
    /// The command signifier used to obtain windows (defaults to "-> ")
    /// </summary>
    public string WindowSwitchSignifier { get; set; } = "-> ";
    /// <summary>
    ///   List of window titles or process names for windows to not show (defaults to
    ///   empty - all windows can be shown)
    /// </summary>
    public List<string> BlackList { get; set; } = new List<string>(Array.Empty<string>());
    /// <summary>
    ///   The threshold for when to consider a window title or process name
    ///   is similar enough to the query for it to be
    ///   displayed (defaults to 60). The larger the number, the
    ///   more similar it needs to be.
    /// </summary>
    public int FuzzySearchThreshold { get; set; } = 60;
  }
}
