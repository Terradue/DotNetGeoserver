using System;
using System.Runtime.Serialization;

namespace Terradue.Geoserver
{
    [DataContract]
    public class Layer
    {
        [DataMember(Name = "name")]
        public String Name { get; set; }

        [DataMember(Name = "href")]
        public String Href { get; set; }
    }
}
