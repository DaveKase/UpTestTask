using Microsoft.AspNet.Mvc;
using Newtonsoft.Json.Linq;
using System;

namespace UpTestTask.Controllers
{
    public class AmazonController : Controller
    {
        JArray jQueryResult = new JArray();

        public JArray Index(string searchString)
        {
            jQueryResult = MakeAmazonQuery(searchString);
            return jQueryResult;
        }

        private JArray MakeAmazonQuery(string searchString)
        {
            if (String.IsNullOrEmpty(searchString))
            {
                return new JArray();
            }

            JObject keysJObject = GetKeys();
            string accessKey = keysJObject.GetValue("accessKey").ToString();
            string secretKey = keysJObject.GetValue("secretKey").ToString();
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
            JArray jArray = amazon.GetData(requestUrl);
            return jArray;
        }

        private JObject GetKeys()
        {
            string jsonString = System.IO.File.ReadAllText(
                "C:/Users/Taavi/Documents/Visual Studio 2015/Projects/UpTestTask/amazon.json");

            JObject jObject = JObject.Parse(jsonString);
            return jObject;
        }

        public JArray ChangeCurrency(string oldCurrency, string newCurrency)
        {
            JArray jArray;

            foreach(JObject jObject in jQueryResult)
            {

            }

            return jQueryResult;
        }
    }
}
