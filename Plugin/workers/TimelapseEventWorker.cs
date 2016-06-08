using OldSimpleJSON;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Gameframer
{
    public class TimelapseEventWorker : GFWorker
    {
        private FinishedDelegate recordingStopped;
        private FinishedDelegate callerDone;
        private FinishedDelegate callerFail;
        public uint missionID { get; private set; }
        public uint flightID { get; private set; }
        public string fullMissionID { get; private set; }         
        public string eventID { get; private set; }         
        public TimelapseCamera tlCamera { get; private set; }
        private OldJSONNode serverResponse;
        private OldJSONNode serverResponseEvent;
        private OldJSONNode eventToAdd;
        private GameObject where;
        private List<ImageFile> images;

        override internal void Start()
        {
            base.Start();
            OnDone = _OnDone;
            OnFail = _OnFail;
        }

        public int GetSpeed()
        {
            if (tlCamera != null)
            {
                return (int)(tlCamera.ssDelay / TimelapseCamera.DEFAULT_DELAY);
            }

            return 0;
        }

        public TimeSpan GetRecordingTimeElapsed()
        {
            if (tlCamera != null && tlCamera.images != null)
            {
                return tlCamera.stopwatch.Elapsed;
            }

            return new TimeSpan();
        }

        public void DoUpload()
        {
            this.images = tlCamera.images;

            var data = new List<PostData>();
            foreach (ImageFile i in images)
            {
                //KSP.IO.File.WriteAllBytes<OmniController>(i.image, i.filename);
                data.Add(new MultiPostData("image", i.filename, i.image));
            }

            double endTime = -1.0;
            double endTimeInDays = -1.0;
            double endTimeUniversal = Planetarium.GetUniversalTime();

            if (FlightGlobals.ActiveVessel != null)
            {
                endTime = FlightGlobals.ActiveVessel.missionTime;
                endTimeInDays = KSPUtils.GameTimeInDays(endTime);
            } 

            var url = "/missions/" + missionID + "/" + flightID + "/events/" + serverResponseEvent["eid"] + "/images?endTime=" + endTime + "&endTimeInDays=" + endTimeInDays + "&endUniversalTime=" + endTimeUniversal;               
            var w = GFWorker.CreateWorker(where, url, data, "POST");
            w.OnDone = (OldJSONNode n) =>
            {
                if (callerDone != null)
                {
                    callerDone(serverResponse);
                }
            };
            w.OnFail = (OldJSONNode n) =>
            {
                if (callerFail != null)
                {
                    callerFail(n);
                }
            };
        }

        protected void TimelapseDone(List<ImageFile> images)
        {
            this.images = images;
            recordingStopped(null);
        }

        private void _OnDone(OldJSONNode n)
        {
            GFLogger.Instance.AddDebugLog(String.Format("{0}.{1}", this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name));
            serverResponse = n["data"];
            GFLogger.Instance.AddDebugLog("Event Worker OK: " + n.ToString());
            serverResponseEvent = GFDataUtils.FindEvent(eventToAdd, serverResponse["events"]);
            fullMissionID = serverResponse["_id"];
            eventID = serverResponseEvent["eid"];
            GFLogger.Instance.AddDebugLog("Event Worker found event: " + serverResponseEvent.ToString());
            GFLogger.Instance.AddDebugLog("Set fullID: " + fullMissionID + " and eventID: " + eventID);
            // do screenshots
            tlCamera = TimelapseCamera.CreateComponent
                (gameObject, TimelapseDone);
        }

        public bool IsTimelapseRunning()
        {
            return !tlCamera.stop;
        }
        public void StopTimelapse()
        {
            tlCamera.StopRecording();
        }

        // 404 is okay, it means we need to create it.
        private void _OnFail(OldJSONNode n)
        {
            GFLogger.Instance.AddDebugLog(String.Format("{0}.{1}", this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name));
            if (n["message"] == "Unauthorized")
            {
                GFLogger.Instance.AddDebugLog("404 Vessel not found.");
            }
            else
            {
                GFLogger.Instance.AddDebugLog("FAIL: " + n.ToString());
                callerFail(n);
            }
        }

        override internal void OnDestroy()
        {
            base.OnDestroy();
            GFLogger.Instance.AddDebugLog("MissionPatcher DESTROYED");
        }

        public static TimelapseEventWorker CreateComponent(GameObject where, uint missionID, uint flightID, OldJSONNode eventToAdd, FinishedDelegate OnDone, FinishedDelegate OnFail, FinishedDelegate recordingStopped)
        {
            String location = String.Format("/missions/{0}/{1}/events", missionID, flightID);
            TimelapseEventWorker eventWorker = where.AddComponent<TimelapseEventWorker>();
            eventWorker.where = where;
            eventWorker.eventToAdd = eventToAdd;
            var data = new List<PostData>();
            data.Add(new PostData(PostData.JSON, Encoding.UTF8.GetBytes(eventToAdd.ToString())));
            eventWorker.initialize(location, data, "POST");
            eventWorker.callerDone = OnDone;
            eventWorker.callerFail = OnFail;
            eventWorker.recordingStopped = recordingStopped;
            eventWorker.missionID = missionID;
            eventWorker.flightID = flightID;            

            GFLogger.Instance.AddDebugLog("TimelapseEventWorker Created: {0}", location);

            return eventWorker;
        }
    }
}
