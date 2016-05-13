using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Terradue.Geoserver
{
    [DataContract]
    internal class GetCoveragesResponse
    {
        [DataMember(Name = "coverages")]
        public CoverageSet CoverageSet { get; set; }

        [IgnoreDataMember]
        public IEnumerable<Coverage> Coverages
        {
            get { return (CoverageSet != null && CoverageSet.Coverages != null) ? CoverageSet.Coverages.ToList() : new List<Coverage>(); }
        }
    }

    [DataContract]
    internal class CoverageSet
    {
        [DataMember(Name = "coverage")]
        public Coverage[] Coverages { get; set; }
    }
}
