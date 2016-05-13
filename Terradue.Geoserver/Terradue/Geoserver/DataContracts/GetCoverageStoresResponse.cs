using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Terradue.Geoserver
{
    [DataContract]
    internal class GetCoverageStoresResponse
    {
        [DataMember(Name = "coverageStores")]
        public CoverageStoreSet CoverageStoreSet { get; set; }

        [IgnoreDataMember]
        public IEnumerable<CoverageStore> CoverageStores
        {
            get { return (CoverageStoreSet != null && CoverageStoreSet.CoverageStores != null) ? CoverageStoreSet.CoverageStores.ToList() : new List<CoverageStore>(); }
        }
    }

    [DataContract]
    internal class CoverageStoreSet
    {
        [DataMember(Name = "coverageStore")]
        public CoverageStore[] CoverageStores { get; set; }
    }
}
