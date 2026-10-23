using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PandorasBoxOptimizer;

public sealed record ApplyResult(bool Ok, string Message, int FilesTouched, int KeysWritten,
                                 List<string> MissingKeys, string? BackupDir);

/// <summary>
/// Reads/patches the Call of Duty settings file. We only ever rewrite the setting
/// lines named in a profile; every other line (resolution, monitor, audio, keybinds,
/// hardware-detect, FPS caps, HDR...) is preserved byte-for-byte. Each change is
/// backed up first so it can be fully restored.
/// </summary>
public static class ConfigEngine
{
    // MW4 == "cod26". The game double-buffers settings as s.1.0.cod26.txt0 / .txt1
    // (older titles also just .txt), so we patch whichever of these exist.
    private const string GameTag = "cod26";
    private static readonly string SettingsGlob = $"s.1.0.{GameTag}.txt*";

    public static string PlayersDir =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                     "Activision", "Call of Duty", "Players");

    public static string BackupRoot =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                     "PandorasBoxOptimizer", "backups");

    /// <summary>The settings files that exist right now (empty if the game hasn't made them yet).</summary>
    public static List<string> FindConfigFiles()
    {
        if (!Directory.Exists(PlayersDir)) return new();
        return Directory.GetFiles(PlayersDir, SettingsGlob)
                        .Where(f => !f.EndsWith(".m", StringComparison.OrdinalIgnoreCase))
                        .OrderBy(f => f)
                        .ToList();
    }

    /// <summary>Apply a profile. Backs up first, then patches only the profile's keys.</summary>
    public static ApplyResult Apply(Profile profile)
    {
        var files = FindConfigFiles();
        if (files.Count == 0)
        {
            return new(false,
                $"No MW4 config found yet.\n\nLaunch Modern Warfare 4 once (reach the main menu), fully quit, then run this again.\n\nLooked in:\n{PlayersDir}",
                0, 0, new(), null);
        }

        // Back every target file up under a timestamped folder.
        string stamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string backupDir = Path.Combine(BackupRoot, stamp);
        Directory.CreateDirectory(backupDir);

        int filesTouched = 0, keysWritten = 0;
        HashSet<string> missing = new(profile.Settings.Keys);

        foreach (var file in files)
        {
            File.Copy(file, Path.Combine(backupDir, Path.GetFileName(file)), overwrite: true);

            string text = File.ReadAllText(file);
            foreach (var (key, value) in profile.Settings)
            {
                var (patched, hits) = SetValue(text, key, value);
                if (hits > 0) { missing.Remove(key); keysWritten += hits; }
                text = patched;
            }
            File.WriteAllText(file, text);
            filesTouched++;
        }

        // Remember the newest backup so one-click Restore knows what to put back.
        File.WriteAllText(Path.Combine(BackupRoot, "last.txt"), backupDir);

        string msg = $"Applied \"{profile.Name}\" to {filesTouched} file(s).";
        if (missing.Count > 0)
            msg += $"\nNote: {missing.Count} setting(s) weren't found in this build and were skipped.";
        return new(true, msg, filesTouched, keysWritten, missing.ToList(), backupDir);
    }

    /// <summary>Restore the most recent backup this app made.</summary>
    public static ApplyResult RestoreLast()
    {
        string marker = Path.Combine(BackupRoot, "last.txt");
        if (!File.Exists(marker))
            return new(false, "No backup to restore — you haven't applied a profile yet.", 0, 0, new(), null);

        string backupDir = File.ReadAllText(marker).Trim();
        if (!Directory.Exists(backupDir))
            return new(false, "The last backup folder is missing.", 0, 0, new(), null);

        int restored = 0;
        foreach (var bak in Directory.GetFiles(backupDir))
        {
            string dest = Path.Combine(PlayersDir, Path.GetFileName(bak));
            File.Copy(bak, dest, overwrite: true);
            restored++;
        }
        return new(true, $"Restored {restored} file(s) from your last backup.", restored, 0, new(), backupDir);
    }

    /// <summary>
    /// Replace the value of every variant of <paramref name="key"/> (key@..., any scope),
    /// keeping the "// comment" tail intact. Returns the new text and how many lines changed.
    /// </summary>
    private static (string text, int hits) SetValue(string text, string key, string value)
    {
        // ^KEY@<scope> = <value>   [// comment]
        string pattern = @"(?m)^(?<pre>" + Regex.Escape(key) +
                         @"@[^\r\n=]*=[ \t]*)(?<val>[^\r\n]*?)(?<cmt>[ \t]*//[^\r\n]*)?$";
        int hits = 0;
        string result = Regex.Replace(text, pattern, m =>
        {
            hits++;
            return m.Groups["pre"].Value + value + m.Groups["cmt"].Value;
        });
        return (result, hits);
    }
}
