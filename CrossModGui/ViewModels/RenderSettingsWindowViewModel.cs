﻿using CrossMod.Rendering;
using SSBHLib.Formats.Materials;
using System.Collections.Generic;

namespace CrossModGui.ViewModels
{
    public class RenderSettingsWindowViewModel : ViewModelBase
    {
        // Workaround to get enums to work with MVVM.
        public Dictionary<RenderSettings.RenderMode, string> DescriptionByRenderMode { get; } = new Dictionary<RenderSettings.RenderMode, string>()
        {
            { RenderSettings.RenderMode.Shaded, "Shaded" },
            { RenderSettings.RenderMode.Basic, "Basic" },
            { RenderSettings.RenderMode.Col, "Col" },
            { RenderSettings.RenderMode.Albedo, "Albedo (Generated)" },
            { RenderSettings.RenderMode.Prm, "Prm" },
            { RenderSettings.RenderMode.Nor, "Nor" },
            { RenderSettings.RenderMode.Emi, "Emi" },
            { RenderSettings.RenderMode.BakeLit, "BakeLit" },
            { RenderSettings.RenderMode.Gao, "Gao Maps" },
            { RenderSettings.RenderMode.Proj, "Proj Maps" },
            { RenderSettings.RenderMode.ColorSet, "ColorSet1" },
            { RenderSettings.RenderMode.Normals, "Normals" },
            { RenderSettings.RenderMode.Tangents, "Tangents" },
            { RenderSettings.RenderMode.Bitangents, "Bitangents (Generated)" },
            { RenderSettings.RenderMode.BakeUV, "bake1" },
            { RenderSettings.RenderMode.UVPattern, "UV Test Pattern" },
            { RenderSettings.RenderMode.AnisotropyLines, "Anisotropic Highlight Direction" },
            { RenderSettings.RenderMode.ParamID, "Param Values" },
            { RenderSettings.RenderMode.MaterialID, "Material ID" }
        };

        public RenderSettings.RenderMode SelectedRenderMode { get; set; } = RenderSettings.RenderMode.Shaded;

        public bool ShowParamControls => SelectedRenderMode == RenderSettings.RenderMode.ParamID;

        public bool ShowChannelControls => SelectedRenderMode != RenderSettings.RenderMode.Shaded;

        public bool EnableBloom { get; set; }
        public float BloomIntensity { get; set; }

        public bool EnableDiffuse { get; set; }

        public bool EnableSpecular { get; set; }

        public bool EnableEmission { get; set; }

        public float DirectLightIntensity { get; set; }

        public float IndirectLightIntensity { get; set; }

        public bool EnableNorMaps { get; set; }

        public bool EnablePrmMetalness { get; set; }
        public bool EnablePrmRoughness { get; set; }
        public bool EnablePrmAo { get; set; }
        public bool EnablePrmSpecular { get; set; }


        public bool EnableVertexColor { get; set; }

        public bool EnableRimLighting { get; set; }

        public bool EnableRed { get; set; }

        public bool EnableGreen { get; set; }

        public bool EnableBlue { get; set; }

        public bool EnableAlpha { get; set; }

        public bool EnableWireframe { get; set; }

        // TODO: This should use an enum and combobox.
        // The available items should be restricted to used material params (ex: not DiffuseTexture).
        public string ParamName { get; set; } = RenderSettings.Instance.ParamId.ToString();

        public RenderSettingsWindowViewModel(RenderSettings renderSettings)
        {
            EnableRed = renderSettings.EnableRed;
            EnableGreen = renderSettings.EnableGreen;
            EnableBlue = renderSettings.EnableBlue;
            EnableAlpha = renderSettings.EnableAlpha;
            EnableDiffuse = renderSettings.EnableDiffuse;
            EnableEmission = renderSettings.EnableEmission;
            EnableRimLighting = renderSettings.EnableRimLighting;
            EnableSpecular = renderSettings.EnableSpecular;
            SelectedRenderMode = renderSettings.ShadingMode;
            EnableVertexColor = renderSettings.RenderVertexColor;
            EnableNorMaps = renderSettings.RenderNorMaps;
            EnablePrmMetalness = renderSettings.RenderPrmMetalness;
            EnablePrmRoughness = renderSettings.RenderPrmRoughness;
            EnablePrmAo = renderSettings.RenderPrmAo;
            EnablePrmSpecular = renderSettings.RenderPrmSpecular;
            DirectLightIntensity = renderSettings.DirectLightIntensity;
            IndirectLightIntensity = renderSettings.DirectLightIntensity;
            EnableBloom = renderSettings.EnableBloom;
            BloomIntensity = renderSettings.BloomIntensity;
            EnableWireframe = renderSettings.EnableWireframe;
        }

        public void SetValues(RenderSettings settings)
        {
            settings.EnableRed = EnableRed;
            settings.EnableGreen = EnableGreen;
            settings.EnableBlue = EnableBlue;
            settings.EnableAlpha = EnableAlpha;
            settings.EnableDiffuse = EnableDiffuse;
            settings.EnableEmission = EnableEmission;
            settings.EnableRimLighting = EnableRimLighting;
            settings.EnableSpecular = EnableSpecular;
            settings.ShadingMode = SelectedRenderMode;
            settings.RenderVertexColor = EnableVertexColor;
            settings.RenderNorMaps = EnableNorMaps;
            settings.RenderPrmMetalness = EnablePrmMetalness;
            settings.RenderPrmRoughness = EnablePrmRoughness;
            settings.RenderPrmAo = EnablePrmAo;
            settings.RenderPrmSpecular = EnablePrmSpecular;
            settings.EnableWireframe = EnableWireframe;

            settings.DirectLightIntensity = DirectLightIntensity;
            settings.IblIntensity = IndirectLightIntensity;
            if (System.Enum.TryParse(ParamName, out MatlEnums.ParamId paramId))
                settings.ParamId = paramId;
            settings.BloomIntensity = BloomIntensity;
            settings.EnableBloom = EnableBloom;
        }
    }
}
