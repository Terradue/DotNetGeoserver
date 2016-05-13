using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.Serialization.Json;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Terradue.Geoserver
{
    public class Geoserver
    {
        private const String GEOSERVER_USERNAME = "admin";
        private const String GEOSERVER_PASSWORD = "geoserver";
        private readonly IDictionary<RequestMethod, String> _RequestMethodLookup = new Dictionary<RequestMethod,String>(){{RequestMethod.Delete, "DELETE"},
                                                                                                                          {RequestMethod.Get, "GET"},
                                                                                                                          {RequestMethod.Post, "POST"},
                                                                                                                          {RequestMethod.Put, "PUT"}};
        private readonly IDictionary<ContentType, String> _ContentTypeLookup = new Dictionary<ContentType, string>(){{ContentType.Json, "application/json"},
                                                                                                                     {ContentType.Xml, "text/xml"},
                                                                                                                     {ContentType.Html, "text/html"}};
        private readonly IDictionary<AcceptType, String> _AcceptTypeLookup = new Dictionary<AcceptType, string>(){{AcceptType.Json, "application/json"},
                                                                                                                     {AcceptType.Xml, "text/xml"},
                                                                                                                     {AcceptType.Html, "text/html"}};
        private readonly IDictionary<SrsType, String> _SrsLookup = new Dictionary<SrsType, String>()
        {
            {SrsType.Epsg4326, "EPSG:4326"}
        };

        private static readonly IDictionary<String, String> _SupportedFileTypeMap = new Dictionary<String, String>()
        {
            {".tif", "GeoTIFF"},
            {".tiff", "GeoTIFF" },
            {".sid", "MrSID"},
            {".dt0", "DTED"},
            {".dt1", "DTED"},
            {".dt2", "DTED"}
        };

        public Geoserver(String serverLocation, String serverPort)
        {
            ServerLocation = serverLocation;
            ServerPort = serverPort;
        }

        public Boolean IsValid
        {
            get
            {
                //TODO: fix to work for remote machines?
                //for now we can assume the machine is reachable since we are only running against the localhost
                //GeoServer instance
                IPGlobalProperties ipGlobal = IPGlobalProperties.GetIPGlobalProperties();
                IPEndPoint[] tcpCon = ipGlobal.GetActiveTcpListeners();

                foreach(IPEndPoint ipEnd in tcpCon)
                {
                    if(ipEnd.Port.ToString() == ServerPort)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public String RestServiceUrl
        {
            get { return GeoserverConstants.Urls.RestUrl.Replace(GeoserverConstants.Placeholders.ServerLocation, ServerLocation).Replace(GeoserverConstants.Placeholders.ServerPort, ServerPort); }
        }

        public IDictionary<String, String> SupportedFileTypes { get { return _SupportedFileTypeMap; } }

        public String ServerLocation { get; set; }
        public String ServerPort { get; set; }

        public IEnumerable<Workspace> GetWorkspaces()
        {
            String requestString = RestServiceUrl + "/workspaces";
            Object workspaceResponse = null;
            String status = String.Empty;

            try
            {
                status = SendRestRequest(requestString, RequestMethod.Get, typeof(GetWorkspacesResponse), ref workspaceResponse, ContentType.Json, AcceptType.Json);
            }
            catch (Exception ex)
            {
                throw;
            }

            if (workspaceResponse is GetWorkspacesResponse)
            {
                foreach (Workspace workspace in (workspaceResponse as GetWorkspacesResponse).Workspaces)
                {
                    workspace.Geoserver = this;
                    yield return workspace;
                }
            }
        }

        public Workspace GetDefaultWorkspace()
        {
            String requestString = RestServiceUrl + "/workspaces/default";
            Object defaultWorkspace = null;
            String status = String.Empty;

            try
            {
                status = SendRestRequest(requestString, RequestMethod.Get, typeof(GetDefaultWorkspaceResponse), ref defaultWorkspace, ContentType.Json, AcceptType.Json);
            }
            catch(Exception ex)
            {
                throw new Exception("Getting default workspace failed. This could be because there is no default workspace exists.", ex);
            }
            
            Workspace wksp = (defaultWorkspace is GetDefaultWorkspaceResponse) ? (defaultWorkspace as GetDefaultWorkspaceResponse).Workspace : null;
            if(wksp != null)
            {
                wksp.Geoserver = this;
            }
            return wksp;
        }

        public void AddWorkspace(String workspaceName)
        {
            String requestUrl = RestServiceUrl + "/workspaces";
            Object newWorkspace = new WorkspaceRequest(workspaceName);

            String status = String.Empty;

            try
            {
                status = SendRestRequest(requestUrl, RequestMethod.Post, typeof(WorkspaceRequest), ref newWorkspace, ContentType.Json, AcceptType.Json);
            }
            catch (Exception ex)
            {
                throw new Exception("Adding workspace failed. This could be because a workspace with the same name already exists", ex);
            }
        }

        public void DeleteWorkspace(String workspaceName)
        {
            String requestUrl = RestServiceUrl + "/workspaces/" + workspaceName + "?recurse=true";
            Object workspaceRequest = null;

            String status = String.Empty;

            try
            {
                status = SendRestRequest(requestUrl, RequestMethod.Delete, typeof(Object), ref workspaceRequest, ContentType.Json, AcceptType.Json);
            }
            catch (Exception ex)
            {
                throw new Exception("Deleting workspace failed. Check GeoServer logs for reason", ex);
            }
        }

        public IEnumerable<Coverage> GetCoverages(String workspaceName)
        {
            //first we have to get the layer coverage stores
            IEnumerable<CoverageStore> coverageStores = GetCoverageStores(workspaceName);
            IList<Coverage> layers = new List<Coverage>();

            foreach(CoverageStore coverageStore in coverageStores)
            {
                foreach(Coverage layer in GetCoverages(workspaceName, coverageStore.Name))
                {
                    layers.Add(layer);
                }
            }

            return layers;
        }

        public IEnumerable<Coverage> GetCoverages(String workspace, String coverageStore)
        {
            String requestUrl = RestServiceUrl + "/workspaces/" + workspace + "/coveragestores/" + coverageStore + "/coverages";
            Object layers = null;
            String status = string.Empty;

            try
            {
                status = SendRestRequest(requestUrl, RequestMethod.Get, typeof(GetCoveragesResponse), ref layers, ContentType.Json, AcceptType.Json);
            }
            catch (Exception ex)
            {
                throw;
            }

            if (layers is GetCoveragesResponse)
            {
                foreach (Coverage layer in (layers as GetCoveragesResponse).Coverages)
                {
                    yield return layer;
                }
            }
        }

        public IEnumerable<CoverageStore> GetCoverageStores(String workspace)
        {
            String requestUrl = RestServiceUrl + "/workspaces/" + workspace + "/coveragestores";
            Object coverageStores = null;
            String status = String.Empty;

            try
            {
                status = SendRestRequest(requestUrl, RequestMethod.Get, typeof(GetCoverageStoresResponse), ref coverageStores, ContentType.Json, AcceptType.Json);
            }
            catch (Exception ex)
            {
                throw;
            }

            if (coverageStores is GetCoverageStoresResponse)
            {
                foreach (CoverageStore store in (coverageStores as GetCoverageStoresResponse).CoverageStores)
                {
                    yield return store;
                }
            }
        }

        public void DeleteCoverageStore(String workspace, String coverageStore)
        {
            String requestUrl = RestServiceUrl + "/workspaces/" + workspace + "/coveragestores/" + coverageStore + "?recurse=true";
            Object request = null;

            String status = String.Empty;

            try
            {
                status = SendRestRequest(requestUrl, RequestMethod.Delete, typeof(Object), ref request, ContentType.Json, AcceptType.Json);
            }
            catch (Exception ex)
            {
                throw new Exception("Deleting coverage store failed. Check GeoServer logs for reason", ex);
            }
        }

        public void DeleteCoverage(String workspace, String coverageStore, String coverage)
        {
            String requestUrl = RestServiceUrl + "/workspaces/" + workspace + "/coveragestores/" + coverageStore + "/coverages/" + coverage + "?recurse=true";
            Object request = null;

            String status = String.Empty;

            try
            {
                status = SendRestRequest(requestUrl, RequestMethod.Delete, typeof(Object), ref request, ContentType.Json, AcceptType.Json);
            }
            catch (Exception ex)
            {
                throw new Exception("Deleting coverage failed. Check GeoServer logs for reason", ex);
            }
        }

        public IEnumerable<LayerGroup> GetLayerGroups(String workspace = null)
        {
            String layerGroupPath = String.IsNullOrEmpty(workspace) ? "/layergroups" : "/workspaces/" + workspace + "/layergroups";
            String requestUrl = RestServiceUrl + layerGroupPath;
            Object layerGroups = null;
            String status = String.Empty;

            try
            {
                status = SendRestRequest(requestUrl, RequestMethod.Get, typeof(GetLayerGroupsResponse), ref layerGroups, ContentType.Json, AcceptType.Json);
            }
            catch(Exception)
            {
                throw;
            }

            if (layerGroups is GetLayerGroupsResponse)
            {
                foreach (LayerGroup layerGroup in (layerGroups as GetLayerGroupsResponse).LayerGroups)
                {
                    yield return layerGroup;
                }
            }
        }

        public void AddLayerGroup(String layerGroupName, IEnumerable<String> targetLayers, String workspace = null)
        {
            String layerGroupPath = String.IsNullOrEmpty(workspace) ? "/layergroups" : "/workspaces/" + workspace + "/layergroups";
            String requestUrl = RestServiceUrl + layerGroupPath;

            String status = String.Empty;

            //NOTE: quick and dirty implementation using XML
            Object requestPayload = "<layerGroup><name>" + layerGroupName + "</name><layers>[LAYERS]</layers></layerGroup>";
            StringBuilder layers = new StringBuilder();
            foreach (string targetLayer in targetLayers)
            {
                layers.Append("<layer>");
                layers.Append(targetLayer);
                layers.Append("</layer>");
            }

            requestPayload = (requestPayload as String).Replace("[LAYERS]", layers.ToString());

            try
            {
                status = SendRestRequest(requestUrl, RequestMethod.Post, typeof(String), ref requestPayload, ContentType.Html, AcceptType.Json);
            }
            catch (Exception ex)
            {
                throw new Exception("Adding layer group failed. Check geoserver log for details", ex); ;
            }

            //TODO: doesn't seem to be working for JSON or I am creating a bad JSON object...
            //Object newLayerGroup = new LayerGroupRequest(layerGroupName, targetLayers);

            //try
            //{
            //    status = SendRestRequest(requestUrl, RequestMethod.Post, typeof(LayerGroupRequest), ref newLayerGroup, ContentType.Json, AcceptType.Json);
            //}
            //catch (Exception ex)
            //{
            //    throw new Exception("Adding layer group failed. This could be because a layer group with the same name already exists", ex);
            //}
        }

        public void DeleteLayerGroup(String layerGroup, String workspace = null)
        {
            String layerGroupPath = String.IsNullOrEmpty(workspace) ? "/layergroups/" + layerGroup : "/workspaces/" + workspace + "/layergroups/" + layerGroup;
            String requestUrl = RestServiceUrl + layerGroupPath;
            Object obj = null;
            String status = String.Empty;

            try
            {
                status = SendRestRequest(requestUrl, RequestMethod.Delete, typeof(Object), ref obj, ContentType.Json, AcceptType.Json);
            }
            catch(Exception ex)
            {
                throw new Exception("error deleting layer group", ex);
            }
        }

        public IEnumerable<Coverage> GetLayerGroupCoverages(String layerGroup, String workspace = null)
        {
            String layerGroupPath = String.IsNullOrEmpty(workspace) ? "/layergroups/" + layerGroup : "/workspaces/" + workspace + "/layergroups/" + layerGroup;
            String requestUrl = RestServiceUrl + layerGroupPath;
            Object temp = null;
            
            try
            {
                String response = GetResponseAsString(requestUrl, RequestMethod.Get, typeof(GetLayerGroupCoveragesResponse), ref temp, ContentType.Json, AcceptType.Json);
                GetLayerGroupCoveragesResponse coveragesResponse = JsonConvert.DeserializeObject<GetLayerGroupCoveragesResponse>(response);
                if(coveragesResponse != null)
                {
                    if(coveragesResponse.CoveragesSet.CoveragesSet.Coverages is JArray)
                    {
                        JArray objArray = coveragesResponse.CoveragesSet.CoveragesSet.Coverages as JArray;
                        IList<Coverage> coverages = new List<Coverage>();
                        foreach(JToken jToken in objArray)
                        {
                            coverages.Add(jToken.ToObject<Coverage>());
                        }

                        return coverages;
                    }
                    else if(coveragesResponse.CoveragesSet.CoveragesSet.Coverages is JObject)
                    {
                        JObject obj = coveragesResponse.CoveragesSet.CoveragesSet.Coverages as JObject;
                        return new Coverage[]{obj.ToObject<Coverage>()};
                    }
                }
                return new List<Coverage>();
            }
            catch(Exception ex)
            {
                throw new Exception("unable to get the coverages for the layer group. check geoserver log for details", ex);
            }
        }

        public void ModifyLayerGroup(String layerGroup, IEnumerable<String> targetLayers, String workspace = null)
        {
            String layerGroupPath = String.IsNullOrEmpty(workspace) ? "/layergroups/" + layerGroup : "/workspaces/" + workspace + "/layergroups/" + layerGroup;
            String requestUrl = RestServiceUrl + layerGroupPath;

            String status = String.Empty;

            //NOTE: quick and dirty implementation using XML
            Object requestPayload = "<layerGroup><name>" + layerGroup + "</name><layers>[LAYERS]</layers><styles>[STYLES]</styles></layerGroup>";
            StringBuilder layers = new StringBuilder();
            StringBuilder styles = new StringBuilder();
            foreach (string targetLayer in targetLayers)
            {
                layers.Append("<layer>");
                layers.Append(targetLayer);
                layers.Append("</layer>");
                styles.Append("<style/>");
            }

            requestPayload = (requestPayload as String).Replace("[LAYERS]", layers.ToString());
            requestPayload = (requestPayload as String).Replace("[STYLES]", styles.ToString());

            try
            {
                status = SendRestRequest(requestUrl, RequestMethod.Put, typeof(String), ref requestPayload, ContentType.Html, AcceptType.Json);
            }
            catch (Exception ex)
            {
                throw new Exception("Adding layer group failed. Check geoserver log for details", ex); ;
            }
        }

        public void AddCoverageStore(String storeName, String path, String workspace)
        {
            String requestUrl = RestServiceUrl + "/workspaces/" + workspace + "/coveragestores";
            String status = String.Empty;

            FileInfo fi = new FileInfo(path);
            if(fi.Exists)
            {
                //String fileType = MimeMapper.GetFileType(path);
                if (_SupportedFileTypeMap.ContainsKey(fi.Extension))
                {
                    String fileType = _SupportedFileTypeMap[fi.Extension];

                    if (!String.IsNullOrEmpty(fileType))
                    {
                        Object coverageStore = new CoverageStoreRequest(workspace, storeName, path, fileType);
                        try
                        {
                            status = SendRestRequest(requestUrl, RequestMethod.Post, typeof(CoverageStoreRequest), ref coverageStore, ContentType.Json, AcceptType.Json);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Adding coverage store failed. Check GeoServer log for details", ex);
                        }
                    }
                }
                else
                {
                    throw new Exception("File type is not supported");
                }
            }
        }

        public void AddCoverage(String layerName, String workspace, String coverageStore, SrsType desiredSrs)
        {
            String requestUrl = RestServiceUrl + "/workspaces/" + workspace + "/coveragestores/" + coverageStore + "/coverages";
            Object coverage = new CoverageRequest(layerName, _SrsLookup[desiredSrs]);

            String status = String.Empty;

            try
            {
                status = SendRestRequest(requestUrl, RequestMethod.Post, typeof(CoverageRequest), ref coverage, ContentType.Json, AcceptType.Json);
            }
            catch (Exception ex)
            {
                throw new Exception("Adding layer group failed. This could be because a layer group with the same name already exists", ex);
            }
        }

        private String SendRestRequest(String requestUrl, RequestMethod method, Type objectType, ref Object payload, ContentType contentType, AcceptType acceptType)
        {
            String statusCode = "Error";
            if(IsValid)
            {
                HttpWebRequest request = HttpWebRequest.Create(requestUrl) as HttpWebRequest;
                request.Method = _RequestMethodLookup[method];
                request.Credentials = new NetworkCredential(GEOSERVER_USERNAME, GEOSERVER_PASSWORD);
                request.ContentType = _ContentTypeLookup[contentType];

                if(contentType == ContentType.Json)
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(objectType);
                    if (method == RequestMethod.Get)
                    {
                        request.Accept = _AcceptTypeLookup[acceptType];
                        HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                        statusCode = response.StatusDescription;
                        payload = serializer.ReadObject(response.GetResponseStream());
                        response.Close();
                    }
                    else if (method == RequestMethod.Post || method == RequestMethod.Delete || method == RequestMethod.Put)
                    {
                        serializer.WriteObject(request.GetRequestStream(), payload);
                        HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                        statusCode = response.StatusDescription;
                        response.Close();
                    }
                }
                else if(contentType == ContentType.Xml)
                {
                    //needs to be written for XML serializer
                    throw new NotImplementedException("needs implementation for XML");
                }
                else
                {
                    request.ContentType = "text/xml";
                    if (method == RequestMethod.Get)
                    {
                        request.Accept = _AcceptTypeLookup[acceptType];
                        HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                        statusCode = response.StatusDescription;
                        payload = new StreamReader(response.GetResponseStream()).ReadToEnd();
                        response.Close();
                    }
                    else if (method == RequestMethod.Post || method == RequestMethod.Delete || method == RequestMethod.Put)
                    {
                        if(payload is String)
                        {
                            byte[] bytes = Encoding.GetEncoding("UTF-8").GetBytes(payload as String);

                            Stream reqStream = request.GetRequestStream();
                            reqStream.Write(bytes, 0, bytes.Length);
                            reqStream.Close();
                        }
                        else
                        {
                            throw new NotImplementedException("method is not implemented for this content type with specified payload");
                        }

                        HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                        statusCode = response.StatusDescription;
                        response.Close();
                    }
                }
                
            }
            

            return statusCode;
        }

        /// <summary>
        /// can be used for debugging the JSON response received
        /// </summary>
        private String GetResponseAsString(String requestUrl, RequestMethod method, Type objectType, ref Object payload, ContentType contentType, AcceptType acceptType)
        {
            String retVal = "Error";

            if(IsValid)
            {
                HttpWebRequest request = HttpWebRequest.Create(requestUrl) as HttpWebRequest;
                request.Method = _RequestMethodLookup[method];
                request.Credentials = new NetworkCredential(GEOSERVER_USERNAME, GEOSERVER_PASSWORD);
                request.ContentType = _ContentTypeLookup[contentType];

                DataContractJsonSerializer serializer = new DataContractJsonSerializer(objectType);
                if (method == RequestMethod.Get)
                {
                    request.Accept = _AcceptTypeLookup[acceptType];
                }
                else if (method == RequestMethod.Post)
                {
                    serializer.WriteObject(request.GetRequestStream(), payload);
                }

                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                retVal = new StreamReader(response.GetResponseStream()).ReadToEnd();
            }

            return retVal;
        }

        /// <summary>
        /// can be used to debug your custom JSON object
        /// </summary>
        public String GetJsonString(Object objToConvert)
        {
            String retVal = String.Empty;
            if(objToConvert != null)
            {
                Type objType = objToConvert.GetType();
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(objType);

                using(MemoryStream ms = new MemoryStream())
                {
                    serializer.WriteObject(ms, objToConvert);
                    ms.Position = 0;
                    using(StreamReader sr = new StreamReader(ms))
                    {
                        retVal = sr.ReadToEnd();
                    }
                }
                
            }
            return retVal;
        }

        public enum RequestMethod
        {
            Delete,
            Get,
            Post,
            Put
        }

        public enum ContentType
        {
            Html,
            Json,
            Xml
        }

        public enum AcceptType
        {
            Html,
            Json,
            Xml
        }

        public enum SrsType
        {
            Epsg4326
        }
    }
}
