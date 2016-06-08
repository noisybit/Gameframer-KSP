namespace Gameframer
{  
    public class VideoOptions
    {
        public static int NONE = -1;
        public static int VIDEO = 0;
        public static int STILL = 1;
        public static int TIMELAPSE = 2;
    }

    public class ImageFile
    {
        public byte[] image;
        public string filename;

        public ImageFile(string filename, byte[] image)
        {
            this.filename = filename;
            this.image = image;
        }
    }
}
