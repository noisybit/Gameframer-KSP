using OldSimpleJSON;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Gameframer
{
    public class MissionPatchWorker : GFWorker
    {
        private FinishedDelegate callerDone;
        private FinishedDelegate callerFail;

        internal override void Start()
        {
            base.Start();

            OnDone = _OnDone;
            OnFail = _OnFail;
        }

        private void _OnDone(OldJSONNode n)
        {
            GFLogger.Instance.AddDebugLog(String.Format("{0}.{1}", this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name));
            callerDone(n);
            Destroy(this);
        }

        private void _OnFail(OldJSONNode n)
        {
            GFLogger.Instance.AddDebugLog(String.Format("{0}.{1}", this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name));
            callerFail(n);
            Destroy(this);
        }

        override internal void OnDestroy()
        {
            base.OnDestroy();
            GFLogger.Instance.AddDebugLog("MissionPatcher DESTROYED");
        }

        public static MissionPatchWorker CreateComponent(GameObject where, string patchLocation, OldJSONNode patch, FinishedDelegate OnDone, FinishedDelegate OnFail)
        {
            MissionPatchWorker patcher = where.AddComponent<MissionPatchWorker>();
            var data = new List<PostData>();
            data.Add(new PostData(PostData.JSON, Encoding.UTF8.GetBytes(patch.ToString())));
            patcher.initialize(patchLocation, data, "PATCH");
            patcher.callerDone = OnDone;
            patcher.callerFail = OnFail;
            return patcher;
        }

    }
}
