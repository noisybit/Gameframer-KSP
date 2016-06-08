using OldSimpleJSON;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Gameframer
{
    public class MissionCreationWorker : GFWorker
    {
        private FinishedDelegate callerDone;
        private FinishedDelegate callerFail;

        public static MissionCreationWorker CreateComponent(GameObject where, string missionName, string missionDetails, string missionPurpose, Vessel v, FinishedDelegate OnDone, FinishedDelegate OnFail)
        {
            MissionCreationWorker w = where.AddComponent<MissionCreationWorker>();
            w.callerDone = OnDone;
            w.callerFail = OnFail;

            var data = new List<PostData>();

            var jsonString = GFDataUtils.CreateJsonForMission(missionName, missionDetails, missionPurpose, v);
            var bytes = Encoding.UTF8.GetBytes(jsonString.ToString());
            data.Add(new PostData(PostData.JSON, bytes));

            w.initialize("/missions/", data, "POST");

            return w;
        }

        override internal void Start()
        {
            base.Start();

            OnDone = _OnDone;
            OnFail = _OnFail;
        }

        private void _OnDone(OldJSONNode n)
        {
            if (SettingsManager.Instance.settings.offlineMode)
            {
                KSP.IO.File.WriteAllText<MissionCreationWorker>(n.ToString(), n["data"]["_id"]+".json");
            }
            GFLogger.Instance.AddDebugLog(String.Format("{0}.{1}", this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name));
            callerDone(n);
        }

        // 404 is okay, it means we need to create it.
        private void _OnFail(OldJSONNode n)
        {
            GFLogger.Instance.AddDebugLog(String.Format("{0}.{1}", this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name));
            if (n["message"] == "Unauthorized")
            {
                GFLogger.Instance.AddDebugLog("404 Vessel not found.");
                callerFail(n);
            }
            else
            {
                GFLogger.Instance.AddDebugLog("FAIL: " + n.ToString());
                callerFail(n);
            }
        }
    }
}
