using KSPPluginFramework;
using OldSimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Gameframer
{
    public class VesselSnapshot : MonoBehaviourExtended
    {
        public delegate void FinishedDelegate(OldJSONNode result);
        protected FinishedDelegate cUploadDone;
        protected FinishedDelegate cUploadFailed;
        RenderTexture renTex;
        private Camera cam;
        private GameObject camGame;
        protected String vesselID;

        internal override void OnDestroy()
        {
            GFLogger.Instance.AddDebugLog(String.Format("{0}.{1}", this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name));
        }

        public static VesselSnapshot CreateWorker(String vesselID, GameObject where, FinishedDelegate uploadDone, FinishedDelegate uploadFailed)
        {
            VesselSnapshot newWorker = where.AddComponent<VesselSnapshot>();
            newWorker.vesselID = vesselID;
            newWorker.cUploadDone = uploadDone;
            newWorker.cUploadFailed = uploadFailed;

            return newWorker;
        }

        internal override void Start()
        {
            GFLogger.Instance.AddDebugLog(String.Format("{0}.{1}", this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name));
            renTex = RenderTexture.GetTemporary((int)1280, (int)720, 32, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
            ResetCameras();
            //StartCoroutine(DoCaptureAndUpload());
            StartCoroutine(CaptureScreen());
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

        private static Camera FindCamera(string name)
        {
            foreach (Camera c in Camera.allCameras)
            {
                if (name == c.name)
                    return c;
                UnityEngine.Debug.Log("Camera:" + c.name);
            }
            return null;
        }

        public IEnumerator CaptureScreen()
        {
            GFLogger.Instance.SetUserStatusMessage("Capturing...");
            GFCamera cam = AddComponent<GFCamera>();
            UnityEngine.Debug.Log("1111111111111");
            var mainCamera = FindCamera("Main Camera");

            if (mainCamera == null) { 

}

            var data = new List<PostData>();
            data.Add(new MultiPostData("image", "foo.jpg", TakePictureAsJPG()));
            var w = GFWorker.CreateWorker(this.gameObject, "/vessels/" + vesselID + "/images", data, "POST");

            w.OnDone = (OldJSONNode n) =>
            {
                UnityEngine.Debug.Log("222222222222");
                UnityEngine.Debug.Log(n);
                UploadDone(n);
            };

            w.OnFail = (OldJSONNode n) =>
            {
                UnityEngine.Debug.Log("33333333333");
                UnityEngine.Debug.Log(n);
                UploadFailed(n);
            };

            yield return null;
        }

        private void ResetCameras()
        {
            SetupCamera("Main Camera", "Main Camera", ref camGame, ref cam);
        }

        private void SetupCamera(string name, string findName, ref GameObject go, ref Camera cam)
        {
            if (FindCamera(findName) != null)
            {
                go = new GameObject();
                go.name = name + " " + go.GetInstanceID();
                cam = go.AddComponent<Camera>();
                cam.CopyFrom(FindCamera(findName));
                cam.targetTexture = renTex;
                cam.cullingMask = EditorLogic.fetch.editorCamera.cullingMask & ~(1 << 16); // hides kerbals
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(1f, 1f, 1f, 0.0f);
                cam.enabled = false;
            }
            else
            {
                cam = null;
                GFLogger.Instance.AddError(string.Format("Gameframer: Couldn't find camera: {0}", findName));
            }
        }

        public byte[] TakePictureAsJPG()
        {
            Texture2D screenShot = new Texture2D(renTex.width, renTex.height);

            ResetCameras();
            if (cam != null)
            {
                // capture the frame
                RenderTexture savedRT = RenderTexture.active;
                RenderTexture.active = renTex;
                cam.Render();
                screenShot.ReadPixels(new Rect(0, 0, renTex.width, renTex.height), 0, 0);
                screenShot.Apply();
                RenderTexture.active = savedRT;
                //                GFLogger.Instance.AddDebugLog(string.Format("Done with regular camera. {0}ms", sw.ElapsedMilliseconds));
            }

            byte[] jpg = screenShot.EncodeToJPG(85);
            Texture2D.Destroy(screenShot);
            return jpg;
        }
    }
}
