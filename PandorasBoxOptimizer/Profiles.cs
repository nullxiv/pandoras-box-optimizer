using System.Collections.Generic;

namespace PandorasBoxOptimizer;

/// <summary>A named set of config overrides plus display copy for the UI.</summary>
public sealed class Profile
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Blurb { get; init; }
    /// <summary>Setting base-name (text before '@') -> value to write. Every matching
    /// variant (@0, @1, ...) of that setting is set to this value. Anything not listed
    /// here is left exactly as the player already has it (resolution, monitor, audio,
    /// keybinds, HDR, FPS caps, hardware-detect lines, etc.).</summary>
    public required Dictionary<string, string> Settings { get; init; }
}

public static class Profiles
{
    public static readonly IReadOnlyList<Profile> All = new[]
    {
        new Profile
        {
            Id = "maxfps",
            Name = "Max FPS",
            Blurb = "Everything floored for the highest frame rate.",
            Settings = new()
            {
                ["TextureQuality"] = "3",
                ["TextureFilter"] = "aniso 2x",
                ["ShaderQuality"] = "Low",
                ["ModelQuality"] = "Low Quality",
                ["ParticleQuality"] = "very low",
                ["ShadowQuality"] = "Very_Low",
                ["ShadowFilteringQuality"] = "Low",
                ["ScreenSpaceShadowQuality"] = "Off",
                ["ShadingQuality"] = "Low",
                ["VolumetricQuality"] = "QUALITY_LOW",
                ["SSRQuality"] = "Off",
                ["AmbientLightingQuality"] = "Off",
                ["WeatherGridVolumesQuality"] = "Off",
                ["WaterCausticsMode"] = "Off",
                ["TerrainQuality"] = "Very Low",
                ["Tessellation"] = "0_Off",
                ["WorldStreamingQuality"] = "Low",
                ["DeferredPhysics"] = "Low Quality",
                ["DepthOfFieldQuality"] = "Low",
                ["ReflectionProbeHalfResolution"] = "true",
                ["ReflectionProbeRelighting"] = "1",
                ["SubdivisionLevel"] = "0",
                ["STLodSkip"] = "0",
                ["DxrMode"] = "Off",
                ["DofEnable"] = "Off",
                ["DofWeaponDisable"] = "true",
                ["DofWorldDisable"] = "true",
                ["MotionBlur"] = "Off",
                ["WeaponMotionBlur"] = "false",
                ["ShowBlood"] = "false",
                ["BulletImpacts"] = "false",
                ["ShowBrass"] = "false",
                ["CorpseLimit"] = "0",
                ["VRS"] = "true",
                ["AATechniquePreferredMP"] = "DLSS",
                ["DLSSModelUI"] = "TRANSFORMER",
                ["DLSSPerfModeMP"] = "Maximum Performance",
                ["DLSSSharpnessMP"] = "0.800000",
                ["DLSSFrameGeneration"] = "false",
                ["NvidiaReflex"] = "Enabled",
                ["VideoMemoryScaleMP"] = "0.700000",
                ["GPUUploadHeaps"] = "false",
                ["RecommendedSet"] = "true",
            },
        },
        new Profile
        {
            Id = "balanced",
            Name = "Balanced",
            Blurb = "Sharp image, still well above your refresh rate.",
            Settings = new()
            {
                ["TextureQuality"] = "1",
                ["TextureFilter"] = "aniso 8x",
                ["ShaderQuality"] = "Medium",
                ["ModelQuality"] = "High Quality",
                ["ParticleQuality"] = "medium",
                ["ShadowQuality"] = "Low",
                ["ShadowFilteringQuality"] = "Medium",
                ["ScreenSpaceShadowQuality"] = "Low",
                ["ShadingQuality"] = "High",
                ["VolumetricQuality"] = "QUALITY_MEDIUM",
                ["SSRQuality"] = "Low",
                ["AmbientLightingQuality"] = "Low",
                ["WeatherGridVolumesQuality"] = "Low",
                ["WaterCausticsMode"] = "Low Quality",
                ["TerrainQuality"] = "Medium",
                ["Tessellation"] = "1_Near",
                ["WorldStreamingQuality"] = "High",
                ["DeferredPhysics"] = "Medium Quality",
                ["DepthOfFieldQuality"] = "Low",
                ["ReflectionProbeHalfResolution"] = "false",
                ["ReflectionProbeRelighting"] = "2",
                ["SubdivisionLevel"] = "0",
                ["STLodSkip"] = "0",
                ["DxrMode"] = "Off",
                ["DofEnable"] = "Off",
                ["DofWeaponDisable"] = "true",
                ["DofWorldDisable"] = "true",
                ["MotionBlur"] = "Off",
                ["WeaponMotionBlur"] = "false",
                ["ShowBlood"] = "true",
                ["BulletImpacts"] = "true",
                ["ShowBrass"] = "false",
                ["CorpseLimit"] = "0",
                ["VRS"] = "true",
                ["AATechniquePreferredMP"] = "DLSS",
                ["DLSSModelUI"] = "TRANSFORMER",
                ["DLSSPerfModeMP"] = "Balanced",
                ["DLSSSharpnessMP"] = "0.500000",
                ["DLSSFrameGeneration"] = "false",
                ["NvidiaReflex"] = "Enabled",
                ["VideoMemoryScaleMP"] = "0.900000",
                ["GPUUploadHeaps"] = "true",
                ["RecommendedSet"] = "true",
            },
        },
    };
}
