namespace Gameframer
{
    public class MultiPostData : PostData
    {
        public string filename;
        public string key;

        public MultiPostData(string key, string filename, byte[] data)
            : base("unknown", data)
        {
            this.filename = filename;
            this.key = key;

            if (filename.ToUpper().IndexOf(".JSON") > -1)
            {
                mimeType = JSON;
            }
            else if (filename.ToUpper().IndexOf(".JPG") > -1)
            {
                mimeType = JPG;
            }
            else if (filename.ToUpper().IndexOf(".CRAFT") > -1)
            {
                mimeType = BINARY;
            }
        }
    }
}