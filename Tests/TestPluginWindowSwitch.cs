using System.Collections.ObjectModel;
using System.IO;

namespace PluginWindowSwitch.Tests
{
  /// <summary>
  /// Integration and unit tests for the <see cref="WindowSwitch"/> plugin.
  /// </summary>
  public class TestPluginWindowSwitch : IDisposable
  {
    private readonly string _tempPath;
    private readonly string _originalCurrentDirectory;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestPluginWindowSwitch"/> class and sets up the sandboxed environment.
    /// </summary>
    public TestPluginWindowSwitch()
    {
      _originalCurrentDirectory = Environment.CurrentDirectory;

      // Clean up legacy directories from previous runs
      try
      {
        foreach (string dir in Directory.GetDirectories(_originalCurrentDirectory, "TempWindowSwitch_*"))
        {
          try
          {
            Directory.Delete(dir, recursive: true);
          }
          catch
          {
            // Ignore if locked
          }
        }
      }
      catch
      {
      }

      string uniqueId = Guid.NewGuid().ToString("N");
      _tempPath = Path.Combine(_originalCurrentDirectory, "TempWindowSwitch_" + uniqueId);
      Directory.CreateDirectory(_tempPath);

      // Create target directory structure matching expected plugin path
      string targetPluginDir = Path.Combine(_tempPath, "PlugBoard", "PluginWindowSwitch", "Plugin");
      Directory.CreateDirectory(targetPluginDir);

      // Write a custom settings.json with a test blacklist
      const string customSettingsJson = /*lang=json*/ @"
      {
        ""AllWindowsSpecialCommand"": ""AllWindows"",
        ""WindowSwitchSignifier"": ""-> "",
        ""BlackList"": [""BlacklistedProcessName""],
        ""FuzzySearchThreshold"": 60
      }";
      File.WriteAllText(Path.Combine(targetPluginDir, "settings.json"), customSettingsJson);

      // Redirect CurrentDirectory
      Environment.CurrentDirectory = _tempPath;

      if (System.Windows.Application.Current == null)
      {
        _ = new System.Windows.Application();
      }

      System.Windows.Application.Current!.Resources["Scorer"] = new FuzzySharp.SimilarityRatio.Scorer.StrategySensitive.PartialRatioScorer();
    }

    /// <summary>
    /// Verifies that the plugin correctly configures its command signifiers and special commands.
    /// </summary>
    [Fact]
    public void TestWindowSwitch_ConfiguresSignifiersAndSpecialCommands()
    {
      WindowSwitch windowSwitch = new();
      windowSwitch.CommandSignifiers().Should().ContainSingle().Which.Should().Be("-> ");
      windowSwitch.SpecialCommands().Should().ContainSingle().Which.Should().Be("AllWindows");
    }

    /// <summary>
    /// Verifies that the plugin enumerates windows, respects the blacklist, and fuzzy-searches items.
    /// </summary>
    [Fact]
    public void TestWindowSwitch_EnumeratesAndFiltersWindows()
    {
      WindowSwitch windowSwitch = new();

      // Startup triggers P/Invoke window scanning on the current machine
      windowSwitch.OnSearchWindowStartup();

      // Retrieve all windows via SpecialCommand
      Collection<Quokka.ListItems.ListItem> allWindows = windowSwitch.OnSpecialCommand("AllWindows");

      // Ensure that blacklisted processes are filtered out
      foreach (Quokka.ListItems.ListItem item in allWindows)
      {
        item.Name.Should().NotStartWith("BlacklistedProcessName |");
        item.Description.Should().NotContain("Process: BlacklistedProcessName");
      }

      // If any windows were found, test query matching against them
      if (allWindows.Count > 0)
      {
        Quokka.ListItems.ListItem firstItem = allWindows[0];
        // Extract a token/substring from name to search
        string searchToken = firstItem.Name.Split('|').First().Trim();

        Collection<Quokka.ListItems.ListItem> queryResults = windowSwitch.OnQueryChange(searchToken);
        queryResults.Should().NotBeEmpty();
        queryResults.Any(x => x.Name == firstItem.Name).Should().BeTrue();

        Collection<Quokka.ListItems.ListItem> signifierResults = windowSwitch.OnSignifier("-> " + searchToken);
        signifierResults.Should().NotBeEmpty();
      }
    }

    /// <summary>
    /// Restores the original current directory and cleans up the sandbox files.
    /// </summary>
    public void Dispose()
    {
      Environment.CurrentDirectory = _originalCurrentDirectory;
      try
      {
        if (Directory.Exists(_tempPath))
        {
          Directory.Delete(_tempPath, recursive: true);
        }
      }
      catch
      {
        // Ignore locked files
      }
      GC.SuppressFinalize(this);
    }
  }
}
