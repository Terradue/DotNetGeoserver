using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Terradue.Geoserver
{
    internal static class MimeMapper
    {
        private static readonly IDictionary<FileTypeKey, String> _SupportedFileTypeMap = new Dictionary<FileTypeKey, String>()
        {
            {new FileTypeKey("image/tiff",".tif"), "GeoTIFF"},
            {new FileTypeKey("image/tiff",".tiff"), "GeoTIFF" },
            {new FileTypeKey("application/octet-stream", ".sid"), "MrSID"},
            {new FileTypeKey("application/octet-stream", ".dt0"), "DTED"},
            {new FileTypeKey("application/octet-stream", ".dt1"), "DTED"},
            {new FileTypeKey("application/octet-stream", ".dt2"), "DTED"}
        };

        public static String GetMimeType(String filename)
        {
            String retVal = String.Empty;
            if(!String.IsNullOrEmpty(filename) && File.Exists(filename))
            {
                Type mimeMapping = Assembly.Load("System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a").GetType("System.Web.MimeMapping");
                MethodInfo getMapping = mimeMapping.GetMethod("GetMimeMapping");//.Invoke(null, new object[] {filename});
                retVal = getMapping.Invoke(null, new object[] {filename}) as String;
            }

            return retVal;
        }

        public static String GetFileType(String mimeType, String fileExtension)
        {
            FileTypeKey key = _SupportedFileTypeMap.Keys.SingleOrDefault(fk => fk.MimeType.Equals(mimeType, StringComparison.InvariantCultureIgnoreCase) && fk.FileExt.Equals(fileExtension, StringComparison.InvariantCultureIgnoreCase));
            if(key != null)
            {
                return _SupportedFileTypeMap[key];
            }
            return String.Empty;
        }

        public static String GetFileType(String filename)
        {
            FileInfo fi = new FileInfo(filename);
            if(fi.Exists)
            {
                String mimeType = GetMimeType(filename);
                return GetFileType(mimeType, fi.Extension);
            }
            return String.Empty;
        }

        private class FileTypeKey
        {
            public FileTypeKey(String mimeType, String fileExtension)
            {
                MimeType = mimeType;
                FileExt = fileExtension;
            }

            public String MimeType { get; private set; }

            public String FileExt { get; private set; }
        }
    }
}
