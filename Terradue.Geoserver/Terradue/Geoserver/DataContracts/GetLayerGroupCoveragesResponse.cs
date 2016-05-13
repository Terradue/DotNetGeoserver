using System;
using System.Runtime.Serialization;

namespace Terradue.Geoserver
{
    [DataContract]
    internal class GetLayerGroupCoveragesResponse
    {
        [DataMember(Name = "layerGroup")]
        public LayerGroupCoveragesSet CoveragesSet { get; set; }
    }

    [DataContract]
    internal class LayerGroupCoveragesSet
    {
        [DataMember(Name = "name")]
        public String Name { get; set; }

        [DataMember(Name = "workspace")]
        public Workspace Workspace { get; set; }

        [DataMember(Name = "publishables")]
        public PublishedSet CoveragesSet { get; set; }
    }

    [DataContract]
    internal class PublishedSet
    {
        [DataMember(Name = "published")]
        public Object Coverages { get; set; }
    }
}
