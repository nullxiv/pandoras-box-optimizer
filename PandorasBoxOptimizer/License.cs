using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace PandorasBoxOptimizer;

public sealed record VerifyResult(bool Ok, string? Reason);

/// <summary>
/// Access-code gate. Codes are validated against the TicketHub bot's public API
/// (not baked into the app), so they can be revoked and are machine-locked server-side.
/// This is light anti-sharing, not real DRM.
/// </summary>
public static class License
{
    // Public base URL of the TicketHub bot (no trailing slash).
    public const string BotBaseUrl = "https://tickethubv2-production.up.railway.app";

    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(12) };

    private static string StorePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PandorasBoxOptimizer", "license.txt");

    public static bool Configured => BotBaseUrl.StartsWith("https://") && !BotBaseUrl.Contains("REPLACE-WITH");

    /// <summary>Stable-per-PC id (hash of the Windows MachineGuid). Opaque to the server.</summary>
    public static string MachineId()
    {
        string seed = "";
        try
        {
            using var k = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography");
            seed = k?.GetValue("MachineGuid") as string ?? "";
        }
        catch { }
        if (string.IsNullOrEmpty(seed)) seed = Environment.MachineName + "|" + Environment.UserName;
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("PBO|" + seed)))[..24];
    }

    public static string? SavedCode()
    {
        try { var c = File.ReadAllText(StorePath).Trim(); return c.Length == 0 ? null : c; }
        catch { return null; }
    }

    public static void Save(string code)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(StorePath)!);
            File.WriteAllText(StorePath, code.Trim());
        }
        catch { }
    }

    public static void Clear() { try { File.Delete(StorePath); } catch { } }

    /// <summary>
    /// Ask the bot whether this code is valid for this machine. Reasons the server returns:
    /// "invalid", "revoked", "bound_other". A network failure returns reason "network:...".
    /// </summary>
    public static async Task<VerifyResult> VerifyAsync(string code)
    {
        if (!Configured) return new VerifyResult(false, "network:not-configured");
        try
        {
            string url = $"{BotBaseUrl}/api/license/verify" +
                         $"?code={Uri.EscapeDataString(code.Trim())}&machine={MachineId()}";
            using var resp = await Http.GetAsync(url);
            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
            var root = doc.RootElement;
            bool ok = root.TryGetProperty("ok", out var okp) && okp.ValueKind == JsonValueKind.True;
            string? reason = root.TryGetProperty("reason", out var rp) ? rp.GetString() : null;
            return new VerifyResult(ok, reason);
        }
        catch (Exception ex)
        {
            return new VerifyResult(false, "network:" + ex.Message);
        }
    }

    public static string FriendlyReason(string? reason) => reason switch
    {
        "invalid" => "That code isn't valid.",
        "revoked" => "This code has been revoked.",
        "bound_other" => "This code is already in use on another PC.",
        _ when reason?.StartsWith("network") == true => "Couldn't reach the server — check your internet.",
        _ => "Couldn't activate. Try again.",
    };
}
