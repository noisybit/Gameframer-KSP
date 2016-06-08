using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameframer
{
    public class GFLogger:Singleton<GFLogger>
    { 
        public static bool PRINT_DEBUG_INFO = false;
        protected GFLogger()
        {
        }
        public List<string> Errors { get; private set; }
        public string ERROR_TEXT = "";
        public string STATUS_TEXT = "";
        public int STATUS = 0;

        void Awake() {
            Errors = new List<string>();
        }

        public void ClearUserStatusMessage()
        {
            STATUS_TEXT = "";
            STATUS = 0;
            AddDebugLog("Cleared status message");
        }

        public void SetUserStatusMessage(string msg, int status = 0)
        {
            STATUS_TEXT = msg;
            STATUS = status;
            AddDebugLog("SetStatusText: {0}", STATUS_TEXT);
        }

        public void AddDebugLog(String text, params object[] strParams)
        {
            if (PRINT_DEBUG_INFO)
            {
                AddDebugLog(String.Format(text, strParams));
            }
        }

        public void AddError(String text, params object[] strParams)
        {
            AddError(String.Format(text, strParams));
        }

        public void AddError(string text)
        {
            Errors.Add(text);
            ERROR_TEXT += "\n[GF] [ERROR] " + text;
            AddDebugLog(text);
        }
        public void AddDebugLog(String text)
        {
            if (PRINT_DEBUG_INFO)
            {
                Debug.Log(String.Format("[GF] {0}", text));
            }
        }
    }
}
