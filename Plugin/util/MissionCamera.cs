using KSPPluginFramework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameframer
{
    public class MissionCamera : MonoBehaviourExtended
    {
        public delegate void ImagesFinishedDelegate(List<ImageFile> images);
        private ImagesFinishedDelegate DoneCallback;

        private int ssCount;
        public float ssDelay { get; private set; }
        private string _filenameGUID;
        public List<ImageFile> images { get; private set; }
        private GFCamera theCamera;

        public static float DEFAULT_DELAY = 0.08f;
        public static int FRAME_COUNT = 25;

        public static MissionCamera CreateComponent(GameObject where, ImagesFinishedDelegate onDone, int videoOption)
        {
            MissionCamera myC = where.AddComponent<MissionCamera>();
            myC.DoneCallback = onDone;
            if (videoOption == VideoOptions.VIDEO)
            {
                myC.ssCount = FRAME_COUNT;
                myC.ssDelay = DEFAULT_DELAY;
            }
            else
            {
                myC.ssCount = 1;
                myC.ssDelay = 0;
            }

            return myC;
        }

        #region Overrides
        override internal void Start()
        {
            images = new List<ImageFile>(ssCount);
            _filenameGUID = System.Guid.NewGuid().ToString();
            theCamera = AddComponent<GFCamera>();

            StartCoroutine(TakeSyncPicture(0));
        }
        internal override void OnDestroy()
        {
            Destroy(theCamera);
            theCamera = null;
            GFLogger.Instance.AddDebugLog("MissionCamera Destroyed");
        }
        #endregion

        IEnumerator<object> TakeSyncPicture(int num)
        {
            while (num < ssCount)
            {
                //GFLogger.Instance.AddDebugLog(string.Format("MissionCamera: DoScreenshot {0} of {1}", num, ssCount));
                yield return new WaitForSeconds(ssDelay);
                yield return new WaitForEndOfFrame();

                if (MapView.MapIsEnabled)
                {
                }
                else
                {
                    string filename = String.Format("{0:0000}-{1:000}.jpg", _filenameGUID, num);
                    images.Add(new ImageFile(filename, theCamera.TakePictureAsJPG()));
                }

                num++;
            }

            Destroy(theCamera);
            DoneCallback(images);
        }
    }
}
