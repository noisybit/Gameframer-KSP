using OldSimpleJSON;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Gameframer
{
    public class MissionEventWorker : GFWorker
    {
        private FinishedDelegate callerDone;
        private FinishedDelegate callerFail;
        private string eventURL;
        protected TimelapseCamera cameraTL;
        private OldJSONNode eventToAdd;
        private OldJSONNode serverResponse;
        private OldJSONNode serverResponseEvent;
        private GameObject where;
        private int videoOption = VideoOptions.VIDEO;
        override internal void Start()
        {
            base.Start();

            OnDone = _OnDone;
            OnFail = _OnFail;
        }

        protected void ScreenshotsDone(List<ImageFile> images)
        {
            //UnityEngine.Debug.Log("Gameframer: images length=" + images.Count);
            if (images.Count == 0)
            {
                callerDone(serverResponse);
                return;
            }

            var data = new List<PostData>();
            foreach (ImageFile i in images)
            {
                // KSP.IO.File.WriteAllBytes<OmniController>(i.image, serverResponseEvent["description"] + "_" + i.filename);
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
            var url = eventURL + "/" + serverResponseEvent["eid"] +
                "/images?endTime=" + endTime
                + "&endTimeInDays=" + endTimeInDays
                + "&endTimeUniversal=" + endTimeUniversal;
            GFWorker.CreateWorker(where, url, data, "POST");
            callerDone(serverResponse);
        }

        private void _OnDone(OldJSONNode n)
        {
            GFLogger.Instance.AddDebugLog(String.Format("{0}.{1}", this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name));
            // Debug.Log(n.ToString());
            serverResponse = n["data"];
            serverResponseEvent = GFDataUtils.FindEvent(eventToAdd, serverResponse["events"]);

            // do screenshots
            if (videoOption == VideoOptions.NONE)
            {
                callerDone(serverResponse);
                return;
            }
            if (MapView.MapIsEnabled)
            {
                callerDone(serverResponse);
                return;
            }
            MissionCamera theCamera = MissionCamera.CreateComponent
                    (gameObject, ScreenshotsDone, videoOption);
        }

        private void _OnFail(OldJSONNode n)
        {
            GFLogger.Instance.AddDebugLog(String.Format("{0}.{1}", this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name));
            if (n["message"] == "Unauthorized")
            {
                GFLogger.Instance.AddDebugLog("Unauthorized access.");
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
        }

        public static MissionEventWorker CreateComponent(GameObject where, uint missionID, uint flightID,
            OldJSONNode eventToAdd, int videoOption, FinishedDelegate OnDone, FinishedDelegate OnFail)
        {
            var url = String.Format("/missions/{0}/{1}/events", missionID, flightID);
            MissionEventWorker eventWorker = where.AddComponent<MissionEventWorker>();
            eventWorker.where = where;
            eventWorker.videoOption = videoOption;
            eventWorker.eventURL = url;
            eventWorker.eventToAdd = eventToAdd;

            var data = new List<PostData>();
            data.Add(new PostData(PostData.JSON, Encoding.UTF8.GetBytes(eventToAdd.ToString())));
            eventWorker.initialize(url, data, "POST");
            eventWorker.callerDone = OnDone;
            eventWorker.callerFail = OnFail;

            return eventWorker;
        }
    }
}
