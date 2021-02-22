using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Script.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace MetascanHelper
{
    public class MetadataRequest
    {
        public String guid { get; set; }
        public String fileName { get; set; }
        public byte[] file { get; set; }
        public MetaScanAction metaScanAction {get;set;}

        private String metaScanBaseUri = ConfigurationManager.AppSettings.Get("MetaScanBaseUri");
        private String metaScanCookieName = ConfigurationManager.AppSettings.Get("MetaScanCookieName");
        private int maxFileSize = int.Parse(ConfigurationManager.AppSettings.Get("MaxFileSize")) * 1000;
        private List<String> fileExtensionsAllowed = ConfigurationManager.AppSettings.Get("ExtensionsAllowed").Split(',').ToList();
        //private List<String> fileCategoriesAllowed = ConfigurationManager.AppSettings.Get("CategoriesAllowed").Split(',').ToList();

        public MetadataRequest(String mGuid, String mFileName, byte[] mFile, MetaScanAction mMetaScanAction)
        {
            guid = mGuid;
            file = mFile;
            fileName = mFileName;
            metaScanAction = mMetaScanAction;
        }

    
        public String PostFileForScanning()
        {
            try
            {
                var metaScanUri = String.Format("{0}", metaScanBaseUri);
                var request = (HttpWebRequest)WebRequest.Create(metaScanBaseUri);
                request.Method = "POST";
                request.CookieContainer = new CookieContainer();
                Cookie cookie = new Cookie()
                {
                    Domain = request.RequestUri.Host,
                    Expires = DateTime.Now.AddDays(1),
                    Name = metaScanCookieName,
                    Path = "/",
                    Secure = false,
                    Value = guid
                };
                request.CookieContainer.Add(cookie);
                request.Headers.Add(new System.Collections.Specialized.NameValueCollection() { { "filename", fileName } });

                using (Stream requestStream = request.GetRequestStream())
                {
                    using (MemoryStream memoryStream = new MemoryStream(file))
                    {
                        byte[] buffer = new byte[4096];
                        int bytesRead = 0;
                        while ((bytesRead = memoryStream.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            requestStream.Write(buffer, 0, bytesRead);
                        }
                    }
                }

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(responseStream))
                        {
                            String respString = reader.ReadToEnd();
                            MetaScanPostResponse rsp = new JavaScriptSerializer().Deserialize<MetaScanPostResponse>(respString);
                            return rsp.data_id;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //handle exception - log
            }
            return string.Empty;
        }

        public bool CheckFileScan(String data_id, out MetaScanScanStatus metaScanScanStatus) //4c3bf9e8b2ad4fb9929a077aaf41bca3
        {
            try
            {
                var metaScanUri = String.Format("{0}/{1}", metaScanBaseUri, data_id);
                var request = (HttpWebRequest)WebRequest.Create(metaScanUri);
                request.CookieContainer = new CookieContainer();
                Cookie cookie = new Cookie()
                {
                    Domain = request.RequestUri.Host,
                    Expires = DateTime.Now.AddDays(1),
                    Name = metaScanCookieName,
                    Path = "/",
                    Secure = false,
                    Value = guid
                };
                request.CookieContainer.Add(cookie);
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(responseStream))
                        {
                            String respString = reader.ReadToEnd();
                            try
                            {
                                ScanResults rsp = new JavaScriptSerializer().Deserialize<ScanResults>(respString);
                                if (rsp != null && rsp.data_id != null)
                                {
                                    if (int.Parse(rsp.scan_results.progress_percentage) != 100)
                                    {
                                        metaScanScanStatus = MetaScanScanStatus.ProccessNotCompleted;
                                        return false;
                                    }
                                    if (rsp.scan_results.scan_details.Ahnlab.scan_result_i > 0 || rsp.scan_results.scan_details.Avira.scan_result_i > 0 || rsp.scan_results.scan_details.ClamWin.scan_result_i > 0 || rsp.scan_results.scan_details.ESET.scan_result_i > 0 ||
                                        !String.IsNullOrEmpty(rsp.scan_results.scan_details.Ahnlab.threat_found) || !String.IsNullOrEmpty(rsp.scan_results.scan_details.Avira.threat_found) || !String.IsNullOrEmpty(rsp.scan_results.scan_details.ClamWin.threat_found) || !String.IsNullOrEmpty(rsp.scan_results.scan_details.ESET.threat_found))
                                    {
                                        metaScanScanStatus = MetaScanScanStatus.ThreatsFound;
                                        return false;
                                    }
                                    if (!fileExtensionsAllowed.Contains(rsp.file_info.file_type_extension.ToLower()))
                                    {
                                        metaScanScanStatus = MetaScanScanStatus.ExtensionNotAllowed;
                                        return false;
                                    }
                                    //if (!fileCategoriesAllowed.Contains(rsp.file_info.file_type_category.ToLower()))
                                    //{
                                    //    metaScanScanStatus = MetaScanScanStatus.ExtensionNotAllowed;
                                    //    return false;
                                    //}
                                    if (rsp.file_info.file_size > maxFileSize)
                                    {
                                        metaScanScanStatus = MetaScanScanStatus.ValidSizeError;
                                        return false;
                                    }
                                    metaScanScanStatus = MetaScanScanStatus.Valid;
                                    return true;
                                }
                            }
                            catch (Exception ex)
                            {
                                KeyValuePair<string, string> rsp = JsonConvert.DeserializeObject<KeyValuePair<string, string>>(respString);
                                if (rsp.Key == data_id && rsp.Value == "Not Found")
                                {
                                    metaScanScanStatus = MetaScanScanStatus.NotFound;
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {

                //handle exception - log
            }
            metaScanScanStatus = MetaScanScanStatus.GeneralError;
            return false;
        }

        
    }
}
