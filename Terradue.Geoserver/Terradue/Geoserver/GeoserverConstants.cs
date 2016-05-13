using System;

namespace Terradue.Geoserver
{
    public static class GeoserverConstants
    {
        public static class Placeholders
        {
            public static readonly String Workspace = "[WORKSPACE]";
            public static readonly String Layers = "[LAYERS]";
            public static readonly String Styles = "[STYLES]";
            public static readonly String ServerLocation = "[SERVER]";
            public static readonly String ServerPort = "[PORT]";
            public static readonly String Format = "[FORMAT]";
        }

        public static class Urls
        {
            public static readonly String BaseUrl = "http://" + Placeholders.ServerLocation + ":" + Placeholders.ServerPort + "/geoserver";
            public static readonly String RestUrl = BaseUrl + "/rest";
        }
    }
}
