using OldSimpleJSON;
using KSPPluginFramework;
using System;
using UnityEngine;

namespace Gameframer
{
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class GFPublicInterface : MonoBehaviourExtended
    {
        internal static OmniController omniController;

        internal override void Awake()
        {
        }

        internal override void Update()
        {
            if(omniController == null)
            {
                omniController = FindObjectOfType<OmniController>();
            }
        }


        /// <summary>
        /// Calling this method will record and upload a Gameframer event. An event is captured
        /// as a short GIF and some metadata about the current game situation. You can append 
        /// your own metadata by passing in a valid JSON object as a string. e.g.:
        /// {
        ///     name: "My Event Name",
        ///     description: "Something amazing just happened and I had to immortalizer it for event on gameframer.com"
        /// }
        /// </summary>
        /// <param name="metadataAsJson">A valid JSON string that represents information about the event to capture</param>
        /// <param name="callback">Delegate to receive callback status updates</param>
        /// <returns>true if event capature was started, false otherwise</returns>
        //public bool CaptureNewEvent(string metadataAsJson, Delegate callback=null)
        //public bool CaptureNewEvent(string name, string description, Delegate callback = null)
        public bool CaptureNewEvent(string name, string description=null)
        {
            if (omniController == null)
                return false;

            return omniController.CaptureNewEvent(name, description);
        }

        public bool SetMissionTitle(string title)
        {
            omniController.RenameMission(title);
            return true;
        }

        public bool SetMissionDescription(string description)
        {
            omniController.RenameMission(null, description);
            return true;
        }
    }
}