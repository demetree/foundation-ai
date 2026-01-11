namespace Foundation
{
    /// <summary>
    /// 
    /// Just a simple class to hold some common constants
    /// 
    /// </summary>
    public static class CommonConstants
    {
        public const string MIME_TYPE_CSV = "text/csv";
        public const string MIME_TYPE_GZIP = "application/gzip";
        public const string MIME_TYPE_ZIP = "application/zip";
        public const string MIME_TYPE_JSON = "application/json";
        public const string MIME_TYPE_GEOJSON = "application/geo+json";
        public const string MIME_TYPE_XML = "text/xml";
        public const string MIME_TYPE_TXT = "text/plain";
        public const string MIME_TYPE_BINARY = "application/octet-stream";
        public const string MIME_TYPE_MESSAGE_PACK = "application/vnd.msgpack";
        public const string MIME_TYPE_ICO = "image/x-icon";
        public const string MIME_TYPE_PNG = "image/png";
        public const string MIME_TYPE_SVG = "image/svg+xml";


        public const string JSON = "JSON";
        public const string XML = "XML";
        public const string CSV = "CSV";
        public const string ZIP = "ZIP";



        // Use this for serializing dates to strings with full precision.
        public const string DATE_FORMAT_FOR_MICROSECOND_PRECISION = "yyyy-MM-ddTHH:mm:ss.ffffff";

        public const string DATE_FORMAT_FOR_SECOND_PRECISION = "yyyy-MM-ddTHH:mm:ss";
        public const string DATE_FORMAT_FOR_SECOND_PRECISION_WITH_UTC_ZONE_INDICATOR = "yyyy-MM-ddTHH:mm:ssZ";


        // Use this for file names because it does not have colons or periods in it.
        public const string DATE_FORMAT_FOR_MICROSECOND_PRECISION_FOR_FILE_NAMES = "yyyy-MM-ddTHH-mm-ss-ffffff";

        // Use this when it is important to ensure that the time zone is UTC.
        public const string DATE_FORMAT_FOR_MICROSECOND_PRECISION_WITH_UTC_ZONE_INDICATOR = "yyyy-MM-ddTHH:mm:ss.ffffffZ";


        public const string DATE_FORMAT_FOR_FILENAMES = "yyyy-MM-ddTHH-mm-ss";

        public const int EXCEL_ROW_MAX = 1048576;


    }
}
