using OldSimpleJSON;
using System;
using UnityEngine;
namespace Gameframer
{
    public class VesselListLookup : GFWorker
    {
        string username;

        private FinishedDelegate callerDone;
        private FinishedDelegate callerFail;

        override internal void Start()
        {
            base.Start();

            OnDone = _OnDone;
            OnFail = _OnFail;
        }

        private void _OnDone(OldJSONNode n)
        {
            GFLogger.Instance.AddDebugLog(String.Format("{0}.{1}", this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name));
            callerDone(n);
        }

        private void _OnFail(OldJSONNode n)
        {
            GFLogger.Instance.AddDebugLog(String.Format("{0}.{1}", this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name));
            callerFail(n);
        }

        public static VesselListLookup CreateComponent(GameObject where, string username, FinishedDelegate OnDone, FinishedDelegate OnFail)
        {
            VesselListLookup w = where.AddComponent<VesselListLookup>();
            w.username = username;
            w.initialize(GenActionURL(w.username), null, "GET");
            w.callerDone = OnDone;
            w.callerFail = OnFail;
            return w;
        }

        public static string GenActionURL(string username)
        {
            var building = "VAB";
            if (HighLogic.LoadedScene == GameScenes.EDITOR)
            {
                building = (EditorLogic.fetch.ship.shipFacility == EditorFacility.SPH ? "SPH" : "VAB");
            }
            return "/vessels?username=" + username + "&sort=-updatedAt" + "&building=" + building;
        }
    }
}
