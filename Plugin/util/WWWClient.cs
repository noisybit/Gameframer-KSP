using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameframer
{
    /// <summary>
    /// A handy class to use WWW class and WWWForm class.
    /// 
    /// Features:
    /// 
    /// * Use callback (delegate) instead of coroutine. Of course this class uses 
    ///   coroutine internally not to stop other processes.
    /// * Can use timeout. 
    /// * Handle complex constructor of WWW class.
    /// 
    /// Requirements:
    /// 
    /// * Unity 4.5
    /// 
    /// Example usage:
    /// 
    /// using WWWKit;
    /// public class WWWClientExample : MonoBehaviour
    /// {
    ///     void Start()
    ///     {
    ///         // GET request
    ///         WWWClient client = new WWWClient(this, "http://example.com/");
    ///         client.OnDone = (WWW www) => {
    ///             Debug.Log(www.text);
    ///         };
    ///         client.Request();
    /// 
    ///         // POST request
    ///         WWWClient http = new WWWClient(this, "http://example.com/");
    ///         client.AddData("foo", "bar");
    ///         client.OnDone = (WWW www) => {
    ///             Debug.Log(www.text);
    ///         };
    ///         client.Request();
    /// 
    ///         // POST request with binary data (file attachment)
    ///         byte[] binary = System.Text.Encoding.Unicode.GetBytes("bar");
    ///         WWWClient http = new WWWClient(this, "http://example.com/");
    ///         client.AddBinaryData("foo", binary, "test.txt", "application/octet-stream");
    ///         client.OnDone = (WWW www) => {
    ///             Debug.Log(www.text);
    ///         };
    ///         client.Request();
    /// 
    ///         // Handle error
    ///         client.OnFail = (WWW www) => {
    ///             Debug.Log(www.error);
    ///         };
    /// 
    ///         // Handle timed out
    ///         client.OnDisposed = () => {
    ///             Debug.Log("Timed out");
    ///         };
    /// 
    ///         // Set timeout time (default is infinity)
    ///         client.Timeout = 10f;
    /// 
    ///         // Add header
    ///         client.AddHeader("Cookie", "cookiename=cookievalue");   
    ///     }
    /// }
    /// </summary>
    public class WWWClient
    {

        public delegate void FinishedDelegate(WWW www);

        public delegate void DisposedDelegate();

        private MonoBehaviour mMonoBehaviour;
        private string mUrl;
        private WWW mWww;
        private WWWForm mForm;
        private byte[] bodyData;
        private string bodyMimeType;
        private Dictionary<string, string> mHeaders;
        private float mTimeout;
        private FinishedDelegate mOnDone;
        private FinishedDelegate mOnFail;
        private DisposedDelegate mOnDisposed;
        private bool mDisposed;

        public Dictionary<string, string> Headers
        {
            set { mHeaders = value; }
            get { return mHeaders; }
        }

        public float Timeout
        {
            set { mTimeout = value; }
            get { return mTimeout; }
        }

        public FinishedDelegate OnDone
        {
            set { mOnDone = value; }
        }

        public FinishedDelegate OnFail
        {
            set { mOnFail = value; }
        }

        public DisposedDelegate OnDisposed
        {
            set { mOnDisposed = value; }
        }

        public WWWClient(MonoBehaviour monoBehaviour, string url)
        {
            mMonoBehaviour = monoBehaviour;
            mUrl = url;
            mHeaders = new Dictionary<string, string>();
            mForm = new WWWForm();
            mTimeout = -1;
            mDisposed = false;
        }

        public void SetBody(byte[] value, string mimeType)
        {
            bodyData = value;
            bodyMimeType = mimeType;
        }

        public byte[] GetBody()
        {
            return bodyData;
        }

        public void AddHeader(string headerName, string value)
        {
            mHeaders.Add(headerName, value);
        }

        public void AddData(string fieldName, string value)
        {
            mForm.AddField(fieldName, value);
        }

        public void AddBinaryData(string fieldName, byte[] contents)
        {
            mForm.AddBinaryData(fieldName, contents);
        }

        public void AddBinaryData(string fieldName, byte[] contents, string fileName)
        {
            mForm.AddBinaryData(fieldName, contents, fileName);
        }

        public void AddBinaryData(string fieldName, byte[] contents, string fileName, string mimeType)
        {
            mForm.AddBinaryData(fieldName, contents, fileName, mimeType);
        }

        public void Request()
        {
            mMonoBehaviour.StartCoroutine(RequestCoroutine());
        }

        public void Dispose()
        {
            if (mWww != null && !mDisposed)
            {
                mWww.Dispose();
                mDisposed = true;
            }
        }

        private IEnumerator RequestCoroutine()
        {
            if (bodyData != null && bodyData.Length > 0)
            {
                //UnityEngine.Debug.Log("USING MY METHOD! " + bodyData);
                // POST request
                mHeaders["Content-Type"] = bodyMimeType;
                mWww = new WWW(mUrl, bodyData, mHeaders);
            }
            else if (mForm.data.Length > 0)
            {
                // Overwrite added headers with WWWForm.headers because WWWForm.headers may have required
                // headers to request. For example, WWWForm.headers has Content-Type like
                // 'multipart/form-data; boundary="xxxx"' if WWWForm.AddBinaryData() is called.
                foreach (var entry in mForm.headers)
                {
                    mHeaders[System.Convert.ToString(entry.Key)] = System.Convert.ToString(entry.Value);
                }

                // POST request
                mWww = new WWW(mUrl, mForm.data, mHeaders);
            }
            else
            {
                // GET request
                mWww = new WWW(mUrl, null, mHeaders);
            }

            yield return mMonoBehaviour.StartCoroutine(CheckTimeout());

            if (mDisposed)
            {
                if (mOnDisposed != null)
                {
                    mOnDisposed();
                }
            }
            else if (System.String.IsNullOrEmpty(mWww.error))
            {
                if (mOnDone != null)
                {
                    mOnDone(mWww);
                }
            }
            else
            {
                if (mOnFail != null)
                {
                    mOnFail(mWww);
                }
            }
        }

        private IEnumerator CheckTimeout()
        {
            float startTime = Time.time;

            while (!mDisposed && !mWww.isDone)
            {
                if (mTimeout > 0 && (Time.time - startTime) >= mTimeout)
                {
                    Dispose();
                    break;
                }
                else
                {
                    yield return null;
                }
            }

            yield return null;
        }
    }
}