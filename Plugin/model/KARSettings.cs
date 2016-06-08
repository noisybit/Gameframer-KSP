using KSPPluginFramework;
using System;

namespace Gameframer
{
    public class KARSettings : ConfigNodeStorage
    {
        public static string LOCATION = "PluginData/Gameframer/GFSettings.cfg";

        internal KARSettings(String FilePath) : base(FilePath) { }

        [Persistent]
        internal bool seenMissionIntroHelp = false;

        // Gameframer.com authentication
        [Persistent]
        internal String username = "";
        [Persistent]
        internal string apiKey = "";
        [Persistent]
        internal string token = "";

        [Persistent]
        internal bool editorVisible = true;
        [Persistent]
        internal bool editorAutoSave = true;
        [Persistent]
        internal float editorX = 265;
        [Persistent]
        internal float editorY = 175;
        
        [Persistent]
        internal bool missionVisible = true;
        [Persistent]
        internal bool missionAutoCapture = true;
        [Persistent]
        internal float missionX = 100;
        [Persistent]
        internal float missionY = 100;

        [Persistent]
        internal bool spaceCenterVisible = true;
        [Persistent]
        internal float spaceCenterX = 50;
        [Persistent]
        internal float spaceCenterY = 50;

        [Persistent]
        internal bool boostAmbientLight = true;

        [Persistent]
        internal bool onLaunch = true;
        [Persistent]
        internal bool onStage = true;
        [Persistent]
        internal bool onSubOrbit = true;
        [Persistent]
        internal bool onOrbit = true;
        [Persistent]
        internal bool onLanding = true;
        [Persistent]
        internal bool onEVA = true;
        [Persistent]
        internal bool onDocking = true;
        [Persistent]
        internal bool onVesselDestroyed = true;
        [Persistent]
        internal bool onVesselRecovered = true;
    }

}