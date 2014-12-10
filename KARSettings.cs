using KSPPluginFramework;
using System;

namespace Gameframer
{
    public class KARSettings : ConfigNodeStorage
    {
        internal KARSettings(String FilePath) : base(FilePath) { }

        [Persistent]
        internal String username = "";
        [Persistent]
        internal bool editorVisible = true;
        [Persistent]
        internal float editorX = 265;
        [Persistent]
        internal float editorY = 175;
        [Persistent]
        internal bool editorOpened = true;
    }
}