namespace Gameframer
{
    public class PostData
    {
        public string mimeType;
        public static string JSON = "application/json";
        public static string CSV = "text/plain";
        public static string BINARY = "application/octet-stream";
        public static string JPG = "image/jpg";

        public byte[] data;

        public PostData(string mimeType, byte[] data)
        {
            this.mimeType = mimeType;
            this.data = data;
        }
    }
}