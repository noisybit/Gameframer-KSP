using KSPPluginFramework;
using UnityEngine;

namespace Gameframer
{
    public class GFCamera : MonoBehaviourExtended
    {
        private GameObject camGameObject_Main;
        private GameObject camGameObject_Far;
        private GameObject camGameObject_Space;
        private GameObject camGameObject_Galaxy;
        private Camera cam_Main;
        private Camera cam_Far;
        private Camera cam_Space;
        private Camera cam_Galaxy;
        private RenderTexture renTex;

        //public static Rect res = new Rect(0, 0, Screen.width, Screen.height);
        public static Rect res = new Rect(0, 0, 1280f, 720f);
        public static float RES_SCALE = 0.5f;
        public static Rect scaled_res = new Rect(0, 0, res.width * RES_SCALE, res.height * RES_SCALE);

        #region Overrides
        internal override void Awake()
        {
            renTex = RenderTexture.GetTemporary((int)scaled_res.width, (int)scaled_res.height, 32, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
            GFLogger.Instance.AddDebugLog(string.Format("Gameframer system info: {0}, {1}, {2}", SystemInfo.operatingSystem, SystemInfo.supportedRenderTargetCount, SystemInfo.supportsRenderTextures));
            GFLogger.Instance.AddDebugLog(string.Format("Gameframer graphics info: {0}, {1}, {2}", SystemInfo.graphicsDeviceVendor, SystemInfo.graphicsDeviceName, SystemInfo.graphicsMemorySize));
            GFLogger.Instance.AddDebugLog(string.Format("Gameframer camera w={0}, h={1}, f={2}", renTex.width, renTex.height, renTex.format));

            ResetCameras();
        }
        internal override void OnDestroy()
        {
            DestroyCams();
            renTex.Release();
            RenderTexture.Destroy(renTex); 
            renTex = null;
            cam_Space = null;
            cam_Main = null;
            cam_Far = null;
            GFLogger.Instance.AddDebugLog("GFCamera destroyed");
        }
        public override string ToString()
        {
            return string.Format("GFCamera: {0}x{1} (aa = {2})", renTex.width, renTex.height, renTex.antiAliasing);
        }
        #endregion

        #region public
        public void PointAtTarget(Vessel target)
        {
            if (target == null)
            {
                GFLogger.Instance.AddError("KMACamera.PointAtTarget called with null target!");
                return;
            }

            if (cam_Main == null)
            {
                GFLogger.Instance.AddError("KMACamera.PointAtTarget called with valid target but cam_Main is null!");
                return;
            }

            // initialize by copying the camera
            Vector3 normal = target.terrainNormal;
            Vector3 pos = cam_Main.transform.position;
            Vector3 aim = cam_Main.transform.forward;
            Quaternion rot = Quaternion.LookRotation(aim, normal);

            //cam positioning
            int cameraDistance = 12;
            Vector3 posOffset = Vector3.one * -25;
            Vector3 ref_origin = target.findLocalCenterOfMass();

            aim = (target.transform.position - ref_origin).normalized;
            rot = Quaternion.LookRotation(aim, normal);
            pos = target.transform.position - posOffset - aim * cameraDistance;

            // apply positions
            cam_Main.transform.position = pos;
            cam_Main.transform.forward = aim;
            cam_Far.transform.position = cam_Main.transform.position;
            cam_Far.transform.forward = cam_Main.transform.forward;

            cam_Main.fieldOfView = cam_Main.fieldOfView;
            cam_Far.fieldOfView = cam_Main.fieldOfView;
            cam_Space.fieldOfView = cam_Main.fieldOfView;

            cam_Space.transform.rotation = cam_Main.transform.rotation;
            cam_Space.transform.forward = cam_Main.transform.forward;
        }

        /// <summary>
        /// Take a picture and return as a raw JPG byte array.
        /// </summary>
        /// <returns>A byte array that represents the JPG encoded picture just taken.</returns>
        public byte[] TakePictureAsJPG()
        {
            Texture2D screenShot = new Texture2D(renTex.width, renTex.height);
            //Texture2D screenShot = new Texture2D(renTex.width, renTex.height, TextureFormat.RGB24, false);
            //if (cam_Main == null)
            {
                // RESETing the cameras gets the newest camera positions (so they move with the vessel)
                ResetCameras();
            }

            if (cam_Main != null)
            {
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();

                RenderCam();

                // capture the frame
                RenderTexture savedRT = RenderTexture.active;
                RenderTexture.active = renTex;
                screenShot.ReadPixels(new Rect(0, 0, renTex.width, renTex.height), 0, 0);
                screenShot.Apply();
                RenderTexture.active = savedRT;

                sw.Stop();
//                GFLogger.Instance.AddDebugLog(string.Format("Done with regular camera. {0}ms", sw.ElapsedMilliseconds));
            }

            byte[] jpg = screenShot.EncodeToJPG(85);
            //KSP.IO.File.WriteAllBytes<OmniController>(jpg, System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + "shot.jpg");
            Texture2D.Destroy(screenShot);
            return jpg;
        }
        #endregion

        #region Camera logic
        private void ResetCameras()
        {
            SetupCamera("TargetCam Galaxy", "GalaxyCamera", ref camGameObject_Galaxy, ref cam_Galaxy);
            SetupCamera("TargetCam Space", "Camera ScaledSpace", ref camGameObject_Space, ref cam_Space);
            SetupCamera("TargetCam Far", "Camera 01", ref camGameObject_Far, ref cam_Far);
            SetupCamera("TargetCam Main", "Camera 00", ref camGameObject_Main, ref cam_Main);
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
                cam.enabled = false;
            }
            else
            {
                cam = null;
                GFLogger.Instance.AddError(string.Format("Gameframer: Couldn't find camera: {0}", findName));
            }
        }

        private void DestroyCams()
        {
            if (cam_Galaxy && cam_Galaxy.gameObject) GameObject.Destroy(cam_Galaxy.gameObject);
            if (cam_Space && cam_Space.gameObject) GameObject.Destroy(cam_Space.gameObject);
            if (cam_Far && cam_Far.gameObject) GameObject.Destroy(cam_Far.gameObject);
            if (cam_Main && cam_Main.gameObject) GameObject.Destroy(cam_Main.gameObject);
        }

        private static Camera FindCamera(string name)
        {
            foreach (Camera c in Camera.allCameras)
            {
                if (c.name == name)
                {
                    return c;
                }
            }
            return null;
        }

        private void RenderCam()
        {
            Color originalColor = RenderSettings.ambientLight;

            if (SettingsManager.Instance.settings.boostAmbientLight)
            {
                KSPUtils.AdjustAmbientLight(0.5f);
            }

            cam_Galaxy.Render();
            cam_Space.Render();
            cam_Far.Render();
            cam_Main.Render();

            if (SettingsManager.Instance.settings.boostAmbientLight)
            {
                RenderSettings.ambientLight = originalColor;
            }
        }
        #endregion
    }
}
