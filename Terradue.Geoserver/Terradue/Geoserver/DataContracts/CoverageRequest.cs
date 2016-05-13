using System;
using System.Runtime.Serialization;

namespace Terradue.Geoserver
{
    [DataContract]
    internal class CoverageRequest
    {
        public CoverageRequest(String name, String desiredSrs)
        {
            Coverage = new TargetCoverage() { Name = name, Srs = desiredSrs, IsEnabled = true, Advertised = true };
            Coverage.Parameters = new CoverageParameterSet();
            Coverage.Parameters.Entries = new ParameterEntry[]{new ParameterEntry(){KeyValue = new string[]{ "USE_JAI_IMAGEREAD", "false"}},
                                                               new ParameterEntry(){KeyValue = new string[]{"USE_MULTITHREADING", "true"}}, 
                                                               new ParameterEntry(){KeyValue = new string[]{"SUGGESTED_TILE_SIZE", "256,256"}}};
            SrsSet set = new SrsSet(){Srs = new string[]{desiredSrs}};
            Coverage.RequestSrs = set;
            Coverage.ResponseSrs = set;
        }

        [DataMember(Name = "coverage")]
        private TargetCoverage Coverage { get; set; }
    }

    [DataContract]
    internal class TargetCoverage
    {
        [DataMember(Name = "name")]
        public String Name { get; set; }

        [DataMember(Name = "requestSRS")]
        public SrsSet RequestSrs { get; set; }

        [DataMember(Name = "responseSRS")]
        public SrsSet ResponseSrs { get; set; }

        [DataMember(Name = "srs")]
        public String Srs { get; set; }

        [DataMember(Name = "enabled")]
        public Boolean IsEnabled { get; set; }

        [DataMember(Name = "parameters")]
        public CoverageParameterSet Parameters { get; set; }

        [DataMember(Name = "advertised")]
        public Boolean Advertised { get; set; }
    }

    [DataContract]
    internal class CoverageParameterSet
    {
        [DataMember(Name = "entry")]
        public ParameterEntry[] Entries { get; set; }
    }

    [DataContract]
    internal class ParameterEntry
    {
        [DataMember(Name = "string")]
        public String[] KeyValue { get; set; }
    }

    [DataContract]
    internal class SrsSet
    {
        [DataMember(Name = "string")]
        public String[] Srs { get; set; }
    }
}
