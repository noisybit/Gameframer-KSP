using OldSimpleJSON;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Gameframer
{
    public class VesselWatcher : GFWorker
    {
        internal static bool SEND_TELEMETRY_ENABLED = false;

        public OldJSONNode vesselJSONFromServer { get; private set; }
        public Vessel vessel { get; private set; }
        private FinishedDelegate callerDone;
        private FinishedDelegate callerFail;

        private FinishedDelegate callerPatchDone;
        private FinishedDelegate callerPatchFail;

        public FlightRecorder recorder;

        public int patchCount = 0;
        public int patchFailCount = 0;

        public static VesselWatcher CreateComponent(GameObject where, Vessel v,
            FinishedDelegate OnDone, FinishedDelegate OnFail,
            FinishedDelegate callerPatchDone, FinishedDelegate callerPatchFail)
        {
            VesselWatcher watcher = where.AddComponent<VesselWatcher>();
            watcher.vessel = v;
            watcher.callerDone = OnDone;
            watcher.callerFail = OnFail;
            watcher.callerPatchDone = callerPatchDone;
            watcher.callerPatchFail = callerPatchFail;
            watcher.recorder = new FlightRecorder(v);
            watcher.initialize("/missions/" + v.rootPart.missionID + "/" + v.rootPart.flightID, null, "GET");
            return watcher;
        }

        public uint MissionID()
        {
            if (vessel.isActiveVessel)
            {
                return vessel.rootPart.missionID;
            }
            else
            {
                return vessel.protoVessel.protoPartSnapshots[vessel.protoVessel.rootIndex].missionID;
            }
        }
        public uint FlightID()
        {
            if (vessel.isActiveVessel)
            {
                return vessel.rootPart.flightID;
            }
            else
            {
                return vessel.protoVessel.protoPartSnapshots[vessel.protoVessel.rootIndex].flightID;
            }
        }
        public void SetServerResponse(OldJSONNode n)
        {
            vesselJSONFromServer = n;
        }
        public string GetEventURL()
        {
            return "/missions/" + vesselJSONFromServer["_id"] + "/events";
        }

        public void PatchMission(GameObject where, OldJSONNode patch)
        {
            if (vesselJSONFromServer == null)
            {
                GFLogger.Instance.AddDebugLog("Can't patch mission, serverV.isNull = {0}", (vesselJSONFromServer == null));
                return;
            }

            MissionPatchWorker.CreateComponent(where, "/missions/" + vesselJSONFromServer["_id"], patch, PatchOnDone, PatchOnFail);
        }

        public void DoTelemetryUpdate()
        {
            // throttled inside UpdateData()
            recorder.UpdateData();
        }

        public void SendTelemetry(GameObject where)
        {
            if (SEND_TELEMETRY_ENABLED)
            {
                string csv = recorder.sb.ToString();

                if (vesselJSONFromServer == null)
                {
                    GFLogger.Instance.AddDebugLog("Can't send telemetry, serverV.isNull = {0}", (vesselJSONFromServer == null));
                    return;
                }
                List<PostData> datalist = new List<PostData>();
                datalist.Add(new PostData(PostData.CSV, Encoding.ASCII.GetBytes(csv)));
                GFWorker w = GFWorker.CreateWorker(where, "/missions/" + vesselJSONFromServer["_id"] + "/csv", datalist, "PUT");
                w.OnDone = (OldJSONNode n) =>
                {
                    recorder.Reset();
                };
                w.OnFail = (OldJSONNode n) =>
                {
                    recorder.Reset();
                    LogFormatted("Telemetry error");
                };
            }
            else
            {
                recorder.Reset();
            }
        }

        private void PatchOnDone(OldJSONNode n)
        {
            patchCount++;
            if (callerPatchDone != null)
                callerPatchDone(n);
        }
        private void PatchOnFail(OldJSONNode n)
        {
            patchFailCount++;
            if (callerPatchFail != null)
                callerPatchFail(n);
        }

        override internal void Start()
        {
            base.Start();

            OnDone = _OnDone;
            OnFail = _OnFail;
        }

        private void _OnDone(OldJSONNode n)
        {
            GFLogger.Instance.AddDebugLog(String.Format("{0}.{1}", this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name));
            vesselJSONFromServer = n["data"];
            var w = GFWorker.CreateWorker(this.gameObject, "POST", "/missions/" + vesselJSONFromServer["_id"] + "/revert", OldJSONNode.Parse("{ missionTime: " + vessel.missionTime + ", universalTime: " + Planetarium.GetUniversalTime() + "}"));
            w.OnDone = (OldJSONNode n2) =>
            {
                callerDone(n2["data"]);
            };
            w.OnFail = (OldJSONNode n2) =>
            {
                callerDone(n["data"]);
            };
        }

        // 404 is okay, it means we need to create it.
        private void _OnFail(OldJSONNode n)
        {
            GFLogger.Instance.AddDebugLog(String.Format("{0}.{1}", this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name));
            callerFail(n);
        }

        private void PostDone(OldJSONNode n)
        {
            GFLogger.Instance.AddDebugLog(String.Format("{0}.{1}", this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name));

            vesselJSONFromServer = n["data"];
            callerDone(vesselJSONFromServer);
        }

        private void PostFail(OldJSONNode n)
        {
            GFLogger.Instance.AddDebugLog(String.Format("{0}.{1}", this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name));
            callerFail(n);
        }
    }
}
