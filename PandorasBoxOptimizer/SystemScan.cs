using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace PandorasBoxOptimizer;

public sealed record SystemInfo(
    string Gpu, string Cpu, int Cores, double RamGb,
    int Width, int Height, int RefreshHz)
{
    // Windows reports slightly under installed (firmware-reserved); round up to the nearest 2 GB.
    public string RamText => RamGb > 0 ? $"{(int)(Math.Ceiling(RamGb / 2.0) * 2)} GB" : "—";
    public string DisplayText => Width > 0 ? $"{Width}×{Height} @ {RefreshHz} Hz" : "—";
}

/// <summary>
/// Reads REAL hardware via Win32 (no extra dependencies) and recommends a profile.
/// Nothing here is faked — GPU/display come from the display driver, RAM from the
/// kernel, CPU from the registry.
/// </summary>
public static class SystemScan
{
    public static SystemInfo Read()
    {
        return new SystemInfo(
            Gpu: PrimaryGpu(),
            Cpu: CpuName(),
            Cores: Environment.ProcessorCount,
            RamGb: TotalRamGb(),
            Width: Display().w, Height: Display().h, RefreshHz: Display().hz);
    }

    /// <summary>true = recommend the "Balanced" (stronger-PC) profile; false = "Max FPS".</summary>
    public static bool RecommendBalanced(SystemInfo s)
    {
        string g = s.Gpu.ToUpperInvariant();
        // NVIDIA RTX model number, e.g. 4070, 5080, 3080.
        var nv = Regex.Match(g, @"RTX\s*(\d{3,4})");
        if (nv.Success && int.TryParse(nv.Groups[1].Value, out int model))
        {
            if (model >= 4000) return true;            // RTX 40 / 50 series
            if (model >= 3080) return true;            // RTX 3080/3090
        }
        // AMD Radeon RX, e.g. 7800, 7900, 9070.
        var amd = Regex.Match(g, @"RX\s*(\d{3,4})");
        if (amd.Success && int.TryParse(amd.Groups[1].Value, out int rx) && rx >= 7800) return true;
        // Intel Arc B-series.
        if (g.Contains("ARC B")) return true;
        return false;
    }

    // ---- GPU (primary display adapter) ----
    private static string PrimaryGpu()
    {
        try
        {
            var dd = new DISPLAY_DEVICE { cb = Marshal.SizeOf<DISPLAY_DEVICE>() };
            if (EnumDisplayDevices(null, 0, ref dd, 0) && !string.IsNullOrWhiteSpace(dd.DeviceString))
                return dd.DeviceString.Trim();
        }
        catch { }
        return "Unknown GPU";
    }

    // ---- CPU name ----
    private static string CpuName()
    {
        try
        {
            using var k = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
            var v = k?.GetValue("ProcessorNameString") as string;
            if (!string.IsNullOrWhiteSpace(v)) return v.Trim();
        }
        catch { }
        return "Unknown CPU";
    }

    // ---- RAM ----
    private static double TotalRamGb()
    {
        try
        {
            var m = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(m)) return m.ullTotalPhys / 1024.0 / 1024.0 / 1024.0;
        }
        catch { }
        return 0;
    }

    // ---- Display (primary, current mode) ----
    private static (int w, int h, int hz) Display()
    {
        try
        {
            var dm = new DEVMODE { dmSize = (ushort)Marshal.SizeOf<DEVMODE>() };
            if (EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref dm))
                return ((int)dm.dmPelsWidth, (int)dm.dmPelsHeight, (int)dm.dmDisplayFrequency);
        }
        catch { }
        return (0, 0, 0);
    }

    // ---------- interop ----------
    private const int ENUM_CURRENT_SETTINGS = -1;

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern bool EnumDisplayDevices(string? lpDevice, uint iDevNum, ref DISPLAY_DEVICE dd, uint flags);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern bool EnumDisplaySettings(string? lpszDeviceName, int iModeNum, ref DEVMODE dm);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX m);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct DISPLAY_DEVICE
    {
        public int cb;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string DeviceName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string DeviceString;
        public int StateFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string DeviceID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string DeviceKey;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct DEVMODE
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string dmDeviceName;
        public ushort dmSpecVersion, dmDriverVersion, dmSize, dmDriverExtra;
        public uint dmFields;
        public int dmPositionX, dmPositionY;
        public uint dmDisplayOrientation, dmDisplayFixedOutput;
        public short dmColor, dmDuplex, dmYResolution, dmTTOption, dmCollate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string dmFormName;
        public ushort dmLogPixels;
        public uint dmBitsPerPel, dmPelsWidth, dmPelsHeight, dmDisplayFlags, dmDisplayFrequency;
        public uint dmICMMethod, dmICMIntent, dmMediaType, dmDitherType, dmReserved1, dmReserved2;
        public uint dmPanningWidth, dmPanningHeight;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private sealed class MEMORYSTATUSEX
    {
        public uint dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>();
        public uint dwMemoryLoad;
        public ulong ullTotalPhys, ullAvailPhys, ullTotalPageFile, ullAvailPageFile,
                     ullTotalVirtual, ullAvailVirtual, ullAvailExtendedVirtual;
    }
}
