using KSPPluginFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Gameframer
{
    public class TimelapseCamera : MonoBehaviourExtended
    {
        public delegate void ImagesFinishedDelegate(List<ImageFile> images);
        private ImagesFinishedDelegate DoneCallback;

        public float ssDelay { get; private set; }
        private string _filenameGUID;
        public List<ImageFile> images { get; private set; }
        private GFCamera theCamera;

        public bool stop { get; private set; }
        public Stopwatch stopwatch { get; private set; }

        public static float DEFAULT_DELAY = 0.08f;
        public static int FRAME_COUNT = 75;

        #region Overrides
        override internal void Start()
        {
            stopwatch = new Stopwatch();
            stopwatch.Start();
            images = new List<ImageFile>();
            _filenameGUID = System.Guid.NewGuid().ToString();
            theCamera = AddComponent<GFCamera>();
            StartCoroutine(TakeSyncPicture(0));
        }
        internal override void OnDestroy()
        {
            Destroy(theCamera);
            theCamera = null;
            GFLogger.Instance.AddDebugLog("TimelapseCamera Destroyed");
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Creates a new TimelapseCamera and starts it. The passed in callback should be used to retrieve the final images when available.
        /// </summary>
        /// <param name="where">gameobject</param>
        /// <param name="onDone">Called with all images when recording is finished (i.e. user stops recording)</param>
        /// <returns></returns>
        public static TimelapseCamera CreateComponent(GameObject where, ImagesFinishedDelegate onDone)
        {
            TimelapseCamera myC = where.AddComponent<TimelapseCamera>();
            myC.stop = false;
            myC.DoneCallback = onDone;
            myC.ssDelay = DEFAULT_DELAY;

            return myC;
        }

        /// <summary>
        /// Stops recording, will trigger the onDone callback
        /// </summary>
        public void StopRecording()
        {
            GFLogger.Instance.AddDebugLog("TimelapseCamera stopped");

            stop = true;
            stopwatch.Stop();
            DoneCallback(images);
        }
        #endregion

        IEnumerator<object> TakeSyncPicture(int num)
        {
            stop = false;
            //GFLogger.Instance.AddDebugLog(string.Format("EventWorker: {0}: Created camera: {1}", name, theCamera.ToString()));
            while (!stop)
            {
                yield return new WaitForSeconds(ssDelay);
                yield return new WaitForEndOfFrame();
                //GFLogger.Instance.AddDebugLog(string.Format("TimelapseCamera: num = {0}, images.length = {1}", num, images.Count));
                if (MapView.MapIsEnabled)
                {
                    StopRecording();
                }
                else
                {
                    string filename = String.Format("{0:0000}-{1:000}.jpg", _filenameGUID, num);
                    ImageFile imgFile = new ImageFile(filename, theCamera.TakePictureAsJPG());
                    images.Add(imgFile);
                    num++;

                    // time to throw away half the images and double frame delay
                    if ((images.Count % FRAME_COUNT) == 0)
                    {
                        //GFLogger.Instance.AddDebugLog(string.Format("TimelapseCamera: (num = {2}) step-up: {0:0.00} -> {1:0.00}", ssDelay, (ssDelay * 2), num));
                        //GFLogger.Instance.AddDebugLog(string.Format("TimelapseCamera: Total length = {0}s", (ssDelay * images.Count)));
                        // double delay
                        ssDelay *= 2;

                        // remove every other image from buffer
                        int pos = 0;
                        for (int i = 0; i < images.Count; i += 2, pos++)
                        {
                            images[pos] = images[i];
                        }
                        //GFLogger.Instance.AddDebugLog(string.Format("Images.length = {0}", images.Count));
                        //GFLogger.Instance.AddDebugLog(string.Format("Remove range {0}, {1}", pos, images.Count));
                        images.RemoveRange(pos, images.Count - pos);
                        images.TrimExcess();
                        //GFLogger.Instance.AddDebugLog(string.Format("Images.length = {0}", images.Count));
                    }
                }
            }

           
            Destroy(theCamera);
        }
    }
}
