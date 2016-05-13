using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Terradue.Geoserver
{
    [DataContract]
    internal class GetLayerGroupsResponse
    {
        [DataMember(Name = "layerGroups")]
        public LayerGroupSet LayerGroupSet { get; set; }

        [IgnoreDataMember]
        public IEnumerable<LayerGroup> LayerGroups
        {
            get { return (LayerGroupSet != null && LayerGroupSet.LayerGroups != null) ? LayerGroupSet.LayerGroups.ToList() : new List<LayerGroup>(); }
        }
    }

    [DataContract]
    internal class LayerGroupSet
    {
        [DataMember(Name = "layerGroup")]
        public LayerGroup[] LayerGroups { get; set; }
    }
}
