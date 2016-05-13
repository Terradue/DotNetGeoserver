using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Terradue.Geoserver
{
    [DataContract]
    public class Workspace
    {
        private ObservableCollection<CoverageStore> _CoverageStores;
        private ObservableCollection<Coverage> _Coverages;
        private ObservableCollection<LayerGroup> _LayerGroups;
        private Geoserver _Geoserver;

        [DataMember(Name = "name")]
        public String Name { get; set; }

        [DataMember(Name = "href")]
        public String Href { get; set; }

        [IgnoreDataMember]
        public IEnumerable<CoverageStore> CoverageStores
        {
            get
            {
                if(_CoverageStores == null && Geoserver != null)
                {
                    IEnumerable<CoverageStore> coverageStores = _Geoserver.GetCoverageStores(Name);
                    _CoverageStores = new ObservableCollection<CoverageStore>(coverageStores);
                }
                return _CoverageStores ??  new ObservableCollection<CoverageStore>();
            }
        }

        [IgnoreDataMember]
        public IEnumerable<Layer> Coverages
        {
            get
            {
                if(_Coverages == null && Geoserver != null)
                {
                    IEnumerable<Coverage> layers = _Geoserver.GetCoverages(Name);
                    _Coverages = new ObservableCollection<Coverage>(layers);
                }
                return _Coverages;
            }
        }

        [IgnoreDataMember]
        public IEnumerable<LayerGroup> LayerGroups
        {
            get
            {
                if(_LayerGroups == null && Geoserver != null)
                {
                    IEnumerable<LayerGroup> layerGroups = _Geoserver.GetLayerGroups(Name);
                    _LayerGroups = new ObservableCollection<LayerGroup>(layerGroups);
                }
                return _LayerGroups;
            }
        }

        internal Geoserver Geoserver
        {
            get { return _Geoserver; }
            set { _Geoserver = value; }
        }
    }
}
