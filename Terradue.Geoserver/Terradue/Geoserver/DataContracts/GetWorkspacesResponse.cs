using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Terradue.Geoserver
{
    [DataContract]
    internal class GetWorkspacesResponse
    {
        [DataMember(Name = "workspaces")]
        public WorkspaceSet WorkspaceSet { get; set; }

        [IgnoreDataMember]
        public IEnumerable<Workspace> Workspaces
        {
            get { return (WorkspaceSet != null && WorkspaceSet.Workspaces != null) ? WorkspaceSet.Workspaces.ToList() : new List<Workspace>(); }
        }
    }

    [DataContract]
    internal class WorkspaceSet
    {
        [DataMember(Name = "workspace")]
        public Workspace[] Workspaces { get; set; }
    }
}
