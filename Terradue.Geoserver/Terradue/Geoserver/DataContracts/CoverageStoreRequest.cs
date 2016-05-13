using System;
using System.Runtime.Serialization;

namespace Terradue.Geoserver
{
    [DataContract]
    internal class CoverageStoreRequest
    {
        public CoverageStoreRequest(String workspace, String name, String path, String fileType)
        {
            CoverageStore = new TargetCoverageStore(){Workspace = workspace, Name = name, StorePath = path, Type = fileType, IsEnabled = true};
        }

        [DataMember(Name = "coverageStore")]
        private TargetCoverageStore CoverageStore { get; set; }
    }

    [DataContract]
    internal class TargetCoverageStore
    {
        [DataMember(Name = "workspace")]
        public String Workspace { get; set; }

        [DataMember(Name = "name")]
        public String Name { get; set; }

        [DataMember(Name = "enabled")]
        public Boolean IsEnabled { get; set; }

        [DataMember(Name = "url")]
        public String StorePath { get; set; }

        [DataMember(Name = "type")]
        public String Type { get; set; }
    }
}
