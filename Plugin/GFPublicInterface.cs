using OldSimpleJSON;
using KSPPluginFramework;
using System;
using UnityEngine;
using System.Collections.Generic;

namespace Gameframer
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class GFPublicInterface : MonoBehaviourExtended
    {
        #region Internal Stuff
        internal static OmniController omniController;
        internal Dictionary<string, bool> registeredEvents;

        internal override void Awake()
        {
            registeredEvents = new Dictionary<string, bool>();
        }

        internal override void Update()
        {
            if(omniController == null)
            {
                omniController = FindObjectOfType<OmniController>();
            }
        }

        internal string FormatName(string id, string eventName)
        {
            return string.Format("{0}-{1}", id, eventName);
        }
        #endregion

        /// <summary>
        /// Registers an event for your mod to trigger. Registering your even is necessary and will allow
        /// users to control whether the event is fired (defaults to true).
        /// </summary>
        /// <param name="id">Your mod ID</param>
        /// <param name="eventName">Your event name</param>
        /// <returns>True on success or false on failure</returns>
        public bool RegisterEvent(string id, string eventName)
        {
            registeredEvents.Add(FormatName(id, eventName), true);
            return true;
        }

        /// <summary>
        /// Unregisters an event for your mod to trigger. Might be useful?
        /// </summary>
        /// <param name="id">Your mod ID</param>
        /// <param name="eventName">Your event name</param>
        /// <returns>True on success or false on failure</returns>
        public bool DeregisterEvent(string id, string eventName)
        {
            registeredEvents.Remove(FormatName(id, eventName));
            return true;
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
        public bool CaptureEvent(string id, string eventName, string description)
        {
            if (registeredEvents.ContainsKey(FormatName(id, eventName)) && 
                registeredEvents[FormatName(id, eventName)]) 
            {
                return omniController.CaptureNewEvent(eventName, description);
            }
            else
            {
                LogFormatted("Not capturing {0}:{1}. User has disabled it.", id, eventName);
                return false;
            }
        }

        public Dictionary<string, bool> GetRegisteredEvents()
        {
            return registeredEvents;
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