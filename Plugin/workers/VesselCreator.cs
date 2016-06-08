using KronalUtils;
using KSPPluginFramework;
using OldSimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace Gameframer
{
    public class VesselCreator : MonoBehaviourExtended
    {
        public delegate void FinishedDelegate(OldJSONNode result);
        private KRSVesselShot vesselShot;
        protected FinishedDelegate cUploadDone;
        protected FinishedDelegate cUploadFailed;

        internal override void OnDestroy()
        {
            GFLogger.Instance.AddDebugLog(String.Format("{0}.{1}", this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name));

            vesselShot = null;
        }

        public static VesselCreator CreateWorker(GameObject where, FinishedDelegate uploadDone, FinishedDelegate uploadFailed)
        {
            VesselCreator newWorker = where.AddComponent<VesselCreator>();
            newWorker.cUploadDone = uploadDone;
            newWorker.cUploadFailed = uploadFailed;

            return newWorker;
        }

        internal override void Start()
        {
            vesselShot = new KRSVesselShot();
            GFLogger.Instance.AddDebugLog(String.Format("{0}.{1}", this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name));
            StartCoroutine(DoCaptureAndUpload());
        }

        IEnumerator DoCaptureAndUpload()
        {
            //            GFLogger.Instance.SetUserStatusMessage("Analyzing...");
            //yield return new WaitForSeconds(0.05f);

            GFLogger.Instance.SetUserStatusMessage("Analyzing and imaging 1 of 2...");
            //GFLogger.Instance.AddDebugLog(String.Format("direction1 = ({0}, {1}, {2})", this.vesselShot.direction.x, this.vesselShot.direction.y, this.vesselShot.direction.z));
            byte[] image = TakeAndGetScreenshot();
            yield return new WaitForSeconds(0.1f);

            GFLogger.Instance.SetUserStatusMessage("Analyzing and imaging 2 of 2...");
            this.vesselShot.direction = new Vector3(0, 1, 0);
            //GFLogger.Instance.AddDebugLog(String.Format("direction2 = ({0}, {1}, {2})", this.vesselShot.direction.x, this.vesselShot.direction.y, this.vesselShot.direction.z));
            byte[] downImage = TakeAndGetScreenshot();
            yield return new WaitForSeconds(0.1f);

            this.vesselShot.direction = new Vector3(0, 0, 1);

            GFLogger.Instance.SetUserStatusMessage("Uploading to gameframer...");
            var vesselJSON = GFDataUtils.CreateJsonForVessel(EditorLogic.fetch.ship);
            var craftFile = EditorLogic.fetch.ship.SaveShip();
            var data = new List<PostData>();

            if (GFLogger.PRINT_DEBUG_INFO)
            {
                KSP.IO.File.WriteAllBytes<VesselCreator>(Encoding.UTF8.GetBytes(vesselJSON.ToString()), vesselJSON["name"] + ".json");
            }

            data.Add(new MultiPostData("json", System.Guid.NewGuid().ToString() + ".json", Encoding.UTF8.GetBytes(vesselJSON.ToString())));
            data.Add(new MultiPostData("craft", System.Guid.NewGuid().ToString() + ".craft", Encoding.UTF8.GetBytes(craftFile.ToString())));
            data.Add(new MultiPostData("image", System.Guid.NewGuid().ToString() + ".jpg", image));
            data.Add(new MultiPostData("imageBottomUp", System.Guid.NewGuid().ToString() + ".jpg", downImage));

            var w = GFWorker.CreateWorker(this.gameObject, "/vessels/", data, "POST");
            w.OnDone = (OldJSONNode n) =>
            {
                UploadDone(n);
            };

            w.OnFail = (OldJSONNode n) =>
            {
                UploadFailed(n);
            };
        }


        private void UploadDone(OldJSONNode n)
        {
            GFLogger.Instance.AddDebugLog(String.Format("Upload done"));
            cUploadDone(n);
            Destroy(this);
        }
        private void UploadFailed(OldJSONNode n)
        {
            cUploadFailed(n);
            Destroy(this);
            GFLogger.Instance.AddDebugLog(String.Format("Upload failed: {0}", n));
        }

        private byte[] TakeAndGetScreenshot()
        {
            GFLogger.Instance.AddDebugLog(String.Format("{0}.{1}", this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name));
            var watch = Stopwatch.StartNew();
            vesselShot.Update();
            vesselShot.GetTexture();
            watch.Stop();
            GFLogger.Instance.AddDebugLog(String.Format("TakeAndGetScreenshot took {0}ms", watch.ElapsedMilliseconds));

            watch = Stopwatch.StartNew();
            RenderTexture t = this.vesselShot.GetTexture();
            Texture2D screenShot = new Texture2D(t.width, t.height, TextureFormat.ARGB32, false);
            var saveRt = RenderTexture.active;
            RenderTexture.active = t;
            screenShot.ReadPixels(new Rect(0, 0, t.width, t.height), 0, 0);
            screenShot.Apply();
            RenderTexture.active = saveRt;

            var bytes = screenShot.EncodeToJPG(85);
            watch.Stop();
            GFLogger.Instance.AddDebugLog(String.Format("Screenshot encoding took {0}ms", watch.ElapsedMilliseconds));

            screenShot = null;
            saveRt = null;
            watch = null;

            return bytes;
        }
    }
}
