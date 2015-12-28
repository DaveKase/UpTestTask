using Microsoft.AspNet.Mvc;
using Newtonsoft.Json.Linq;
using System;

namespace UpTestTask.Controllers
{
    public class AmazonController : Controller
    {

        public JArray Index(string searchString, string oldCurrency, string newCurrency)
        {
            JArray jArray = MakeAmazonQuery(searchString, oldCurrency, newCurrency);
            return jArray;
        }

        private JArray MakeAmazonQuery(string searchString, string oldCurrency, string newCurrency)
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

        public JArray ChangeCurrency(string resultString, string oldCurrency, string newCurrency)
        {
            JArray jQueryResult = JArray.Parse(resultString);
            JArray jArray = new JArray();

            foreach(JObject jObject in jQueryResult)
            {
                string title = jObject.GetValue("title").ToString();
                string price = jObject.GetValue("price").ToString();
                
                if (int.Parse(oldCurrency) == 0 && int.Parse(newCurrency) == 1)
                {
                    JObject newJObject = ChangePrice(price, title, 1.36, "E");
                    jArray.Add(newJObject);
                }

                else if (int.Parse(oldCurrency) == 0 && int.Parse(newCurrency) == 2)
                {
                    JObject newJObject = ChangePrice(price, title, 1.49, "S");
                    jArray.Add(newJObject);
                }

                else if (int.Parse(oldCurrency) == 1 && int.Parse(newCurrency) == 2)
                {
                    JObject newJObject = ChangePrice(price, title, 1.49, "S");
                    jArray.Add(newJObject);
                }

                else if (int.Parse(oldCurrency) == 2 && int.Parse(newCurrency) == 1)
                {
                    JObject newJObject = ChangePrice(price, title, 1.36, "E");
                    jArray.Add(newJObject);
                }

                else if (int.Parse(oldCurrency) == 2 && int.Parse(newCurrency) == 0)
                {
                    JObject newJObject = ChangePrice(price, title, 1, "P");
                    jArray.Add(newJObject);
                }

                else if (int.Parse(oldCurrency) == 1 && int.Parse(newCurrency) == 0)
                {
                    JObject newJObject = ChangePrice(price, title, 1, "P");
                    jArray.Add(newJObject);
                }
            }

            return jArray;
        }

        private JObject ChangePrice(string price, string title, double multiplier, string currency)
        {
            string priceStr = price.Remove(0, 1);
            double priceVal = double.Parse(priceStr, System.Globalization.NumberStyles.Currency);
            double newPrice = priceVal * multiplier;
            newPrice = Math.Round(newPrice, 2);
            string newPriceStr = currency + newPrice;
            newPriceStr = newPriceStr.Replace(",", ".");
            JObject newJObject = new JObject();
            newJObject.Add("title", title);
            newJObject.Add("price", newPriceStr);
            return newJObject;
        }
    }
}
