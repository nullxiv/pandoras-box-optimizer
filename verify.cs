using System.Text.RegularExpressions;

// Replicates ConfigEngine.SetValue to validate the patch logic against a real config copy.
static (string, int) SetValue(string text, string key, string value)
{
    string pattern = @"(?m)^(?<pre>" + Regex.Escape(key) +
                     @"@[^\r\n=]*=[ \t]*)(?<val>[^\r\n]*?)(?<cmt>[ \t]*//[^\r\n]*)?$";
    int hits = 0;
    string result = Regex.Replace(text, pattern, m => { hits++; return m.Groups["pre"].Value + value + m.Groups["cmt"].Value; });
    return (result, hits);
}

string src = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "Activision", "Call of Duty", "Players", "s.1.0.cod25.txt0");
Console.WriteLine($"Reading real config copy: {src}\n");
string original = File.ReadAllText(src);

// A representative slice of the Max FPS profile, incl. a double-variant key.
var changes = new (string key, string val)[]
{
    ("TextureQuality", "3"),
    ("TextureFilter", "aniso 2x"),
    ("ShadowQuality", "Very_Low"),
    ("BulletImpacts", "false"),
    ("DLSSPerfModeMP", "Maximum Performance"),
    ("VideoMemoryScaleMP", "0.700000"),
    ("VolumetricQuality", "QUALITY_LOW"),   // appears as @0 and @1
    ("WaterCausticsMode", "Off"),
    ("RecommendedSet", "true"),
};

string text = original;
int total = 0;
foreach (var (k, v) in changes) { var (t, h) = SetValue(text, k, v); text = t; total += h; Console.WriteLine($"  {k,-22} -> {v,-22}  ({h} line(s))"); }
Console.WriteLine($"\nTotal lines rewritten: {total}");

// --- Assertions ---
int fail = 0;
void Check(bool ok, string msg) { Console.WriteLine((ok ? "PASS  " : "FAIL  ") + msg); if (!ok) fail++; }

Check(text.Contains("TextureQuality@0;61129;7764 = 3 // 0 to 3"), "TextureQuality value+comment preserved");
int volOrig = Regex.Matches(original, @"(?m)^VolumetricQuality@").Count;
Check(Regex.Matches(text, @"(?m)^VolumetricQuality@\S*\s*=\s*QUALITY_LOW").Count == volOrig, $"All VolumetricQuality variants set ({volOrig} present)");
Check(!text.Contains("ScreenSpaceShadowQuality@0;42742;43096 = Very_Low"), "ShadowQuality did NOT bleed into ScreenSpaceShadowQuality");

// Lines that must be UNTOUCHED (compare specific originals survive)
foreach (var keep in new[] { "Resolution@", "Monitor@", "RefreshRate@", "Volume@", "VoicePushToTalk@", "YawSpeed@", "DetectedMemoryAmountMB@" })
{
    var oLine = Regex.Match(original, @"(?m)^" + Regex.Escape(keep) + @".*$").Value;
    Check(text.Contains(oLine) && oLine.Length > 0, $"Untouched: {keep.TrimEnd('@')}  [{oLine.Trim()}]");
}

// No collateral damage: every line that differs must belong to a key we intended to change,
// and the total line count must be unchanged.
var oLines = original.Replace("\r\n", "\n").Split('\n');
var nLines = text.Replace("\r\n", "\n").Split('\n');
Check(oLines.Length == nLines.Length, "Line count unchanged");
var keySet = changes.Select(c => c.key).ToHashSet();
int diff = 0, collateral = 0;
for (int i = 0; i < System.Math.Min(oLines.Length, nLines.Length); i++)
{
    if (oLines[i] == nLines[i]) continue;
    diff++;
    string lineKey = Regex.Match(nLines[i], @"^(?<k>[A-Za-z0-9]+)@").Groups["k"].Value;
    if (!keySet.Contains(lineKey)) { collateral++; Console.WriteLine($"   COLLATERAL change on: {nLines[i].Trim()}"); }
}
Console.WriteLine($"\nLines differing from original: {diff} (of {total} rewrites; rest were already at target)");
Check(collateral == 0, "Zero collateral changes — only profile keys were touched");
Check(diff <= total, "Diff count consistent with rewrites");

Console.WriteLine(fail == 0 ? "\nALL CHECKS PASSED" : $"\n{fail} CHECK(S) FAILED");
Environment.Exit(fail == 0 ? 0 : 1);
