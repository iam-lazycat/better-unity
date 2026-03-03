using System;
using System.Collections.Generic;
using UnityEngine;

namespace LazyCat.BetterUnity
{
    public enum ScreenshotTarget { GameView, SceneView }

    [Serializable]
    public class ScreenshotPreset
    {
        public string id     = Guid.NewGuid().ToString();
        public string name   = "Custom";
        public int    width  = 1920;
        public int    height = 1080;
    }

    [Serializable]
    public class ScreenshotSettings
    {
        public string          outputPath          = "Screenshots";
        public int             selectedPresetIndex = 1;
        public bool            useCustomSize       = false;
        public int             customWidth         = 1920;
        public int             customHeight        = 1080;
        public bool            hideUI              = false;
        public ScreenshotTarget target             = ScreenshotTarget.GameView;
        public bool            openAfterCapture    = false;

        public List<ScreenshotPreset> presets = new List<ScreenshotPreset>
        {
            new ScreenshotPreset { name = "HD",      width = 1280, height = 720  },
            new ScreenshotPreset { name = "Full HD",  width = 1920, height = 1080 },
            new ScreenshotPreset { name = "2K",       width = 2560, height = 1440 },
            new ScreenshotPreset { name = "4K",       width = 3840, height = 2160 },
        };
    }
}
