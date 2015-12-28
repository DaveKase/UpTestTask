using Microsoft.AspNet.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace UpTestTask.Controllers
{
    public class HomeController : Controller
    {
        public List<string> titleList;
        private Dictionary<string, string> titlePriceDict;

        public IActionResult Index(string searchString)
        {
            titlePriceDict = MakeAmazonQuery(searchString);
            return View(titlePriceDict);
        }

        private Dictionary<string, string> MakeAmazonQuery(string searchString)
        {
            if(String.IsNullOrEmpty(searchString))
            {
                return new Dictionary<string, string>();
            }

            JObject jObject = GetKeys();
            string accessKey = jObject.GetValue("accessKey").ToString();
            string secretKey = jObject.GetValue("secretKey").ToString();
            string destination = "ecs.amazonaws.co.uk";

            string requestString = "Service=AWSECommerceService"
                    + "&Version=2009-03-31"
                    + "&Operation=ItemSearch"
                    + "&SearchIndex=All"
                    + "&ResponseGroup=Medium"
                    + "&AssociateTag=212437046868"
                    + "&Keywords=" + searchString
                    ;

            AmazonApi amazon = new AmazonApi(accessKey, secretKey, destination);
            string requestUrl = amazon.Sign(requestString);
            titlePriceDict = amazon.GetData(requestUrl);
            return titlePriceDict;
        }

        private JObject GetKeys()
        {
            string jsonString = System.IO.File.ReadAllText(
                "C:/Users/Taavi/Documents/Visual Studio 2015/Projects/UpTestTask/amazon.json");

            JObject jObject = JObject.Parse(jsonString);
            return jObject;
        }

        public JArray GetJson()
        {
            JArray jArray = new JArray();

            foreach (string key in titlePriceDict.Keys)
            {
                string price = titlePriceDict[key];
                JObject jObject = new JObject();
                jObject.Add("title", key);
                jObject.Add("price", price);
                jArray.Add(jObject);
            }

            return jArray;
        }
    }
}
