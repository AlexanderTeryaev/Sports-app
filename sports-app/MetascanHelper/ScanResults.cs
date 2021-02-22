using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetascanHelper
{
    public class ScanResults
    {
        public String file_id { get; set; }
        public scan_results scan_results { get; set; }
        public file_info file_info { get; set; }
        public String data_id { get; set; } //"4c3bf9e8b2ad4fb9929a077aaf41bca3"
        public String rescan_count { get; set; } //1,
        public String share_file { get; set; } //1,
        public String source { get; set; } //"172.31.34.104",
        public String scanned_on { get; set; } //"172.31.45.215"
    }

    public class scan_results
    {
        public scan_details scan_details { get; set; }
        public String rescan_available {get;set;}
        public String data_id {get;set;}
        public String scan_all_result_i {get;set;}
        public String start_time {get;set;} //": "2015-06-30T10:39:57.798Z",
        public String total_time{get;set;}//": 1.0,
        public String total_avs {get;set;}//": 4,
        public String progress_percentage {get;set;}//": 100,
        public String in_queue {get;set;}//": 0,
        public String scan_all_result_a {get;set;}//": "Clean"
    }

    public class scan_details
    {
        public Ahnlab Ahnlab { get; set; }
        public Ahnlab Avira { get; set; }
        public Ahnlab ClamWin { get; set; }
        public Ahnlab ESET { get; set; }
    }

    public abstract class ScanDetailsMethod
    {
        public String threat_found { get; set; }
        public int scan_result_i { get; set; }
        public String def_time { get; set; }
        public String scan_time { get; set; }
    }

    public class Ahnlab : ScanDetailsMethod
    {

    }
    public class Avira : ScanDetailsMethod
    {

    }
    public class ClamWin : ScanDetailsMethod
    {

    }
    public class ESET : ScanDetailsMethod
    {

    }

    public class file_info
    {
        public long file_size { get; set; }
        public String upload_timestamp { get; set; }
        public String md5 { get; set; }//": "B164AB3C5606B2D4931EBA44B46EAB4E",
        public String sha1 { get; set; }//": "0763121455133437FB943F5900651E4F8A0A78C9",
        public String sha256 { get; set; }//": "2EAD2EEEE836482AD19D7355EBB11CA44768A0542D628E66AF56E1677518DB5F",
        public String file_type_category { get; set; }//": "G",
        public String file_type_description { get; set; }//": "Portable Network Graphics",
        public String file_type_extension { get; set; }//": "PNG",
        public String display_name { get; set; }//": "stats.png"
    }
}
