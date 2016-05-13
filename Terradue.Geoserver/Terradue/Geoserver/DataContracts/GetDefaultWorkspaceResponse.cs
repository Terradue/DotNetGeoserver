using System.Runtime.Serialization;

namespace Terradue.Geoserver
{
    [DataContract]
    internal class GetDefaultWorkspaceResponse
    {
        [DataMember(Name = "workspace")]
        public Workspace Workspace { get; set; }
    }
}
