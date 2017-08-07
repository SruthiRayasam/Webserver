using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace WebServer
{
    public class TwitterFeedReader
    {
        /// <summary>
        /// Fetch Twitter account tweets.
        /// </summary>
        /// <param name="searchText"></param>
        /// <returns></returns>
        public string FetchTwitterFeed(string searchText)
        {
            var jsonTwitterFeed = string.Empty;

            //TwitterAPI default url
            string resource_url = "https://api.twitter.com/1.1/search/tweets.json?q=from%3Asalesforce{0}&result_type=recent&count=10";

            if (searchText != null)
            {
                resource_url = string.Format(resource_url, searchText.Length > 0 ? "%20" + UrlEncode(searchText) : "");
            }
            else
            {
                resource_url = string.Format(resource_url, "");
            }
            
            //SET the header for twitter call..
            // oauth application keys
            var oauth_token = "376771251-mSLLbwzPvHNbZnWRkgBNQxUyr43IFAXhvNKL6FvU"; //"insert here...";
            var oauth_token_secret = "1NgsgT1BDu0ZCP4oF1MSZHuIuIte57qLngIMMuowwumLS"; //"insert here...";
            var oauth_consumer_key = "rKu9h99lZDEc06dHJ3GhWsu0C";// = "insert here...";
            var oauth_consumerKeY_urlencode = "rKu9h99lZDEc06dHJ3GhWsu0C";
            var oauth_consumer_secret = "Q6RVsgvdI5vF5cX3zjW4bFUEQNFnal8aF79rIrTUkglfGJdcSc";// = "insert here...";
            var oauth_consumer_secret_urlencode = "Q6RVsgvdI5vF5cX3zjW4bFUEQNFnal8aF79rIrTUkglfGJdcSc";// = "insert here...";

            try
            {

                //Get access_token from twitter oauth api.
                string postData = "grant_type=client_credentials";
                ASCIIEncoding encoding = new ASCIIEncoding();
                byte[] byte1 = encoding.GetBytes(postData);
                var basic_token1 = Base64Encode(oauth_consumerKeY_urlencode + ":" + oauth_consumer_secret_urlencode);

                //Post Credentilas to TwitterAPI to fetch the Oauth_token.
                HttpWebRequest auth_request = (HttpWebRequest)WebRequest.Create("https://api.twitter.com/oauth2/token?grant_type=client_credentials");
                auth_request.Headers.Add("Authorization", "Basic " + basic_token1);
                auth_request.Method = "POST";
                auth_request.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
                var auth_response = (HttpWebResponse)auth_request.GetResponse();
                var reader1 = new StreamReader(auth_response.GetResponseStream());
                var auth_responseText = reader1.ReadToEnd();

                var oauth_token1 = JsonConvert.DeserializeObject<Oauth_Token>(auth_responseText);

                //Fetch tweets feed from TwitterApi by passing oauth_token
                HttpWebRequest feed_request = (HttpWebRequest)WebRequest.Create(resource_url);
                feed_request.Headers.Add("Authorization", "bearer " + oauth_token1.access_token);
                feed_request.Method = "GET";
                feed_request.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
                var feed_response = (HttpWebResponse)feed_request.GetResponse();
                var reader = new StreamReader(feed_response.GetResponseStream());
                var objText = reader.ReadToEnd();
                
                jsonTwitterFeed = objText.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return jsonTwitterFeed;

        }

        /// <summary>
        /// Convert PlainText to URLEncode text.
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public string UrlEncode(string plainText)
        {
            var urlEncodeText = HttpUtility.UrlEncode(plainText);
            return urlEncodeText;
        }

        /// <summary>
        /// Convert PlainText to Base64Encoded text.
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }
}
