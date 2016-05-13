using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Terradue.Geoserver
{
    [DataContract]
    internal class LayerGroupRequest
    {
        public LayerGroupRequest(String layerGroupName, IList<String> targetLayers)
        {
            LayerGroup = new TargetLayerGroup(targetLayers){Name = layerGroupName};
        }

        [DataMember(Name = "layerGroup")]
        private TargetLayerGroup LayerGroup { get; set; }
    }

    [DataContract]
    internal class TargetLayerGroup
    {
        public TargetLayerGroup(IList<String> targetLayers)
        {
            IList<TargetLayer> targets = new List<TargetLayer>();
            foreach (string targetLayer in targetLayers)
            {
                TargetLayer tl = new TargetLayer();
                tl.PublishedLayer = new Published(){Name = targetLayer};
                targets.Add(tl);
            }
            Layers = targets.ToArray();
        }
        
        [DataMember(Name = "name")]
        public String Name { get; set; }

        [DataMember(Name = "publishables")]
        private TargetLayer[] Layers { get; set; }
    }

    [DataContract]
    internal class TargetLayer
    {
        [DataMember(Name = "published")]
        public Published PublishedLayer { get; set; }
    }

    [DataContract]
    internal class Published
    {
        [DataMember(Name = "name")]
        public String Name { get; set; }
    }
}
