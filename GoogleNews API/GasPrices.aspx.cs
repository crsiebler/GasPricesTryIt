using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Services;
using System.Net;
using System.IO;
using System.Text;
using System.Data;
using System.Xml.Linq;

/// Name:   Cory Siebler
/// ASUID:  1000832292
/// Email:  csiebler@asu.edu
/// Class:  ASU CSE 445 (#11845)
namespace GasPrices_API
{
    public partial class GasPrices : System.Web.UI.Page
    {
        /// <summary>
        /// Page Load Method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        [WebMethod]
        public static string GetPrices(string address)
        {
            Location location = new Location();
            location.latitude = 0.00;
            location.longitude = 0.00;

            // Initialize the List of News Articles
            List<GasPrice> gasPrices = new List<GasPrice>();

            // Initialize the Google Maps request
            HttpWebRequest geocodeRequest = (HttpWebRequest)WebRequest.Create(
                "https://maps.googleapis.com/maps/api/geocode/xml?address=" + HttpUtility.UrlEncode(address) + "&key=AIzaSyBAKxqcK17_ngThmKCHFLw4g1_aB1eOHDY"
            );

            // Specify GET Method Request
            geocodeRequest.Method = "GET";

            HttpWebResponse geocodeResponse = (HttpWebResponse)geocodeRequest.GetResponse();

            // Check the Response State Code for Success
            if (geocodeResponse.StatusCode == HttpStatusCode.OK)
            {
                var xdoc = XDocument.Load(geocodeResponse.GetResponseStream());

                var result = xdoc.Element("GeocodeResponse").Element("result");
                var locationElement = result.Element("geometry").Element("location");
                var lat = locationElement.Element("lat");
                var lng = locationElement.Element("lng");

                location.latitude = double.Parse(lat.Value);
                location.longitude = double.Parse(lng.Value);
            }
            else
            {
                return "";
            }

            // Initialize the Google News RSS Feed request
            HttpWebRequest gasPriceRequest = (HttpWebRequest)WebRequest.Create(
                "http://devapi.mygasfeed.com/stations/radius/" + location.latitude.ToString() + "/" + location.longitude.ToString() + "/5/reg/distance/rfej9napna.json"
            );

            // Specify GET Method Request
            gasPriceRequest.Method = "GET";

            // Perform the Request
            HttpWebResponse gasPriceResponse = (HttpWebResponse)gasPriceRequest.GetResponse();

            // Check the Response State Code for Success
            if (gasPriceResponse.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = gasPriceResponse.GetResponseStream();
                StreamReader readStream = null;

                // Check the Character Set of the Response
                if (gasPriceResponse.CharacterSet == "")
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(gasPriceResponse.CharacterSet));
                }

                // Convert the Stream in a JSON string
                string data = readStream.ReadToEnd();

                return data;
            }

            return "";
        }

        public class Location
        {
            public double latitude { get; set; }
            public double longitude { get; set; }
        }

        public class GasPrice
        {
            public string title { get; set; }
            public string url { get; set; }
            public string date { get; set; }
            public string id { get; set; }
            public string description { get; set; }
        }
    }
}