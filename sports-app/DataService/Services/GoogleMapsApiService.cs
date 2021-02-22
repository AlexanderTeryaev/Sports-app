using System;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Web;
using Newtonsoft.Json;

namespace DataService.Services
{
    [DataContract]
    public class AddressResponse
    {
        [DataMember(Name = "origin_addresses")]
        public string[] OriginAddresses { get; set; }
        [DataMember(Name = "destination_addresses")]
        public string[] DestinationAddresses { get; set; }
        [DataMember(Name = "rows")]
        public Row[] Rows { get; set; }
        [DataMember(Name = "status")]
        public string Status { get; set; }
    }

    [DataContract]
    public class Row
    {
        [DataMember(Name = "elements")]
        public Element[] Elements { get; set; }
    }

    [DataContract]
    public class Element
    {
        [DataMember(Name = "duration")]
        public Distance Duration { get; set; }
        [DataMember(Name = "distance")]
        public Distance Distance { get; set; }
        [DataMember(Name = "status")]
        public string Status { get; set; }
    }
    [DataContract]
    public class Distance
    {
        [DataMember(Name = "text")]
        public string Text { get; set; }
        [DataMember(Name = "value")]
        public long Value { get; set; }
    }
    
    public class GoogleMapsApiService
    {
        private const string API_KEY = "AIzaSyBiwCq-EDCvfhQ786WvFgSQvrSxCgjzsYM";
        private const string URL = "https://maps.googleapis.com/maps/api/distancematrix/json?origins={origin}&destinations={destination}&mode=driving&language=en-Us&key={key}";

        private string GetStringUrl(string originAddress, string destinationAddress)
        {
            return URL.Replace("{origin}", HttpUtility.UrlEncode(originAddress))
                .Replace("{destination}", HttpUtility.UrlEncode(destinationAddress))
                .Replace("{key}", API_KEY);
        }

        public AddressResponse GetAddressResponse(string originAddress, string destinationAddress)
        {
            var url = GetStringUrl(originAddress, destinationAddress);
            var client = new HttpClient();
            var json = client.GetStringAsync(url).Result;
            AddressResponse jsonObject = JsonConvert.DeserializeObject<AddressResponse>(json);
            return jsonObject;
        }


        public double GetDistance(string originAddress, string destinationAddress)
        {
            var distanceAddressObject = GetAddressResponse(originAddress, destinationAddress);
            var distance = distanceAddressObject.Rows.Select(c=>c.Elements).Select(c=>c.Select(q=>q.Distance)).Select(c=>c.FirstOrDefault()).FirstOrDefault();
            return distance?.Value != null ? Convert.ToInt32(distance.Value)/1000.00 : 0D;
        }

        public double GetDistance(string originAddress, string destinationAddress, string additionalAddress)
        {
            var distanceAddressObject1 = GetAddressResponse(originAddress, destinationAddress);
            var distance1 = distanceAddressObject1.Rows.Select(c => c.Elements).Select(c => c.Select(q => q.Distance)).Select(c => c.FirstOrDefault()).FirstOrDefault();
            double distance1Value = distance1?.Value != null ? Convert.ToInt32(distance1.Value) / 1000.00 : 0D;
            var distanceAddressObject2 = GetAddressResponse(destinationAddress, additionalAddress);
            var distance2 = distanceAddressObject2.Rows.Select(c => c.Elements).Select(c => c.Select(q => q.Distance)).Select(c => c.FirstOrDefault()).FirstOrDefault();
            double distance2Value = distance2?.Value != null ? Convert.ToInt32(distance2.Value) / 1000.00 : 0D;
            return distance1Value + distance2Value;
        }
    }
}
