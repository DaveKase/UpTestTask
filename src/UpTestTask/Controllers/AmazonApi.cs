using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;

using System.Web;
using System.Xml;

namespace UpTestTask.Controllers
{
    public class AmazonApi
    {
        private string _accessKey;
        private string _secretKey;
        private string _endPoint;
        private string _destination;

        private const string NAMESPACE = "http://webservices.amazon.com/AWSECommerceService/2011-08-01";
        private const string ITEM_ID = "0545010225";
        private const string REQUEST_URI = "/onca/xml";
        private const string REQUEST_METHOD = "GET";

        private byte[] _secret;
        private HMAC _signer;

        public AmazonApi(string accessKey, string secretKey, string destination) 
        {
            _accessKey = accessKey;
            _secretKey = secretKey;
            _destination = destination;

            _endPoint = destination.ToLower();
            _secret = Encoding.UTF8.GetBytes(_secretKey);
            _signer = new HMACSHA256(this._secret);
        }
        
        public string Sign(string queryString)
        {
            IDictionary<string, string> request = this.CreateDictionary(queryString);
            return Sign(request);
        }

        public Dictionary<string, string> GetData(string url)
        {
            Dictionary<string, string> titleAndPriceDict = new Dictionary<string, string>();

            try
            {
                XmlDocument doc = GetXmlDoc(url);
                XmlNodeList errorMessageNodes = doc.GetElementsByTagName("Message", NAMESPACE);

                if (errorMessageNodes != null && errorMessageNodes.Count > 0)
                {
                    String message = errorMessageNodes.Item(0).InnerText;
                    titleAndPriceDict.Add("Error", message + " (but signature worked)");
                    return titleAndPriceDict;
                }

                XmlNodeList titleNodes = doc.GetElementsByTagName("Title", NAMESPACE);
                XmlNodeList priceNodes = doc.GetElementsByTagName("FormattedPrice", NAMESPACE);
                List<string> titleList = CreateTitleList(titleNodes);
                List<string> priceList = CreatePriceList(priceNodes);
                
                for (int i = 0; i < titleNodes.Count; i++)
                {
                    if (!titleAndPriceDict.ContainsKey(titleList[i]))
                    {
                        titleAndPriceDict.Add(titleList[i], priceList[i]);
                    }
                }

                return titleAndPriceDict;
            }
            catch (Exception e)
            {
                titleAndPriceDict = new Dictionary<string, string>();
                titleAndPriceDict.Add("Error", e.Message + " " + e.StackTrace);
                return titleAndPriceDict;
            }
        }

        private string Sign(IDictionary<string, string> request)
        {
            ParamComparer pc = new ParamComparer();
            SortedDictionary<string, string> sortedMap = new SortedDictionary<string, string>(request, pc);
            
            sortedMap["AWSAccessKeyId"] = _accessKey;
            sortedMap["Timestamp"] = GetTimestamp();
            
            string canonicalQS = ConstructCanonicalQueryString(sortedMap);
            
            StringBuilder builder = new StringBuilder();
            builder.Append(REQUEST_METHOD).Append("\n").Append(_endPoint).Append("\n")
                .Append(REQUEST_URI).Append("\n").Append(canonicalQS);

            string stringToSign = builder.ToString();
            byte[] toSign = Encoding.UTF8.GetBytes(stringToSign);
            
            byte[] sigBytes = _signer.ComputeHash(toSign);
            string signature = Convert.ToBase64String(sigBytes);
            
            StringBuilder qsBuilder = new StringBuilder();
            qsBuilder.Append("http://").Append(_endPoint).Append(REQUEST_URI).Append("?").Append(canonicalQS)
                .Append("&Signature=").Append(PercentEncodeRfc3986(signature));

            return qsBuilder.ToString();
        }

        private IDictionary<string, string> CreateDictionary(string queryString)
        {
            Dictionary<string, string> map = new Dictionary<string, string>();
            string[] requestParams = queryString.Split('&');

            for (int i = 0; i < requestParams.Length; i++)
            {
                if (requestParams[i].Length < 1)
                {
                    continue;
                }

                char[] sep = { '=' };
                string[] param = requestParams[i].Split(sep, 2);
                for (int j = 0; j < param.Length; j++)
                {
                    param[j] = HttpUtility.UrlDecode(param[j], Encoding.UTF8);
                }
                switch (param.Length)
                {
                    case 1:
                        {
                            if (requestParams[i].Length >= 1)
                            {
                                if (requestParams[i].ToCharArray()[0] == '=')
                                {
                                    map[""] = param[0];
                                }
                                else
                                {
                                    map[param[0]] = "";
                                }
                            }
                            break;
                        }
                    case 2:
                        {
                            if (!string.IsNullOrEmpty(param[0]))
                            {
                                map[param[0]] = param[1];
                            }
                        }
                        break;
                }
            }

            return map;
        }

        private string GetTimestamp()
        {
            DateTime currentTime = DateTime.UtcNow;
            string timestamp = currentTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
            return timestamp;
        }

        private string ConstructCanonicalQueryString(SortedDictionary<string, string> sortedParamMap)
        {
            StringBuilder builder = new StringBuilder();

            if (sortedParamMap.Count == 0)
            {
                builder.Append("");
                return builder.ToString();
            }

            foreach (KeyValuePair<string, string> kvp in sortedParamMap)
            {
                builder.Append(this.PercentEncodeRfc3986(kvp.Key));
                builder.Append("=");
                builder.Append(this.PercentEncodeRfc3986(kvp.Value));
                builder.Append("&");
            }
            string canonicalString = builder.ToString();
            canonicalString = canonicalString.Substring(0, canonicalString.Length - 1);

            return canonicalString;
        }

        private string PercentEncodeRfc3986(string str)
        {
            str = HttpUtility.UrlEncode(str, System.Text.Encoding.UTF8);
            str = str.Replace("'", "%27").Replace("(", "%28").Replace(")", "%29").Replace("*", "%2A").
                Replace("!", "%21").Replace("%7e", "~").Replace("+", "%20");

            StringBuilder sbuilder = new StringBuilder(str);
            for (int i = 0; i < sbuilder.Length; i++)
            {
                if (sbuilder[i] == '%')
                {
                    if (Char.IsLetter(sbuilder[i + 1]) || Char.IsLetter(sbuilder[i + 2]))
                    {
                        sbuilder[i + 1] = Char.ToUpper(sbuilder[i + 1]);
                        sbuilder[i + 2] = Char.ToUpper(sbuilder[i + 2]);
                    }
                }
            }

            return sbuilder.ToString();
        }

        private XmlDocument GetXmlDoc(string url)
        {
            WebRequest request = HttpWebRequest.Create(url);
            WebResponse response = request.GetResponse();
            XmlDocument doc = new XmlDocument();
            doc.Load(response.GetResponseStream());

            return doc;
        }

        private List<string> CreateTitleList(XmlNodeList titleNodes)
        {
            List<string> titleList = new List<string>();

            foreach (XmlNode node in titleNodes)
            {
                string titleNode = node.InnerXml;
                titleList.Add(titleNode);
            }

            return titleList;
        }

        private List<string> CreatePriceList(XmlNodeList priceNodes)
        {
            List<string> pricesList = new List<string>();

            foreach (XmlNode node in priceNodes)
            {
                string price = node.InnerXml;
                pricesList.Add(price);
            }

            return pricesList;
        }
    }

    class ParamComparer : IComparer<string>
    {
        public int Compare(string p1, string p2)
        {
            return string.CompareOrdinal(p1, p2);
        }
    }
}
