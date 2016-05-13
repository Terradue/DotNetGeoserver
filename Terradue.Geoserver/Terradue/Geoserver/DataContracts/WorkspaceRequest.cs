using System;
using System.Runtime.Serialization;

namespace Terradue.Geoserver
{
    [DataContract]
    internal class WorkspaceRequest
    {
        public WorkspaceRequest(String workspaceName)
        {
            Workspace = new TargetWorkspace() {Name = workspaceName};
        }

        [DataMember(Name = "workspace")]
        private TargetWorkspace Workspace { get; set; }

        [DataContract]
        private class TargetWorkspace
        {
            [DataMember(Name = "name")]
            public String Name { get; set; }
        }
    }
}
