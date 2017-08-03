using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Xml.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WebServer
{
    public class WebServer
    {
        private readonly HttpListener _listener = new HttpListener();
        private readonly Func<HttpListenerRequest, string> _responderMethod;

        public WebServer(IReadOnlyCollection<string> prefixes, Func<HttpListenerRequest, string> method)
        {
            if (!HttpListener.IsSupported)
            {
                throw new NotSupportedException("Needs Windows XP SP2, Server 2003 or later.");
            }

            // URI prefixes are required eg: "http://localhost:6000/test/"
            if (prefixes == null || prefixes.Count == 0)
            {
                throw new ArgumentException("URI prefixes are required");
            }

            if (method == null)
            {
                throw new ArgumentException("responder method required");
            }

            foreach (var s in prefixes)
            {
                _listener.Prefixes.Add(s);
            }

            _responderMethod = method;
            _listener.Start();
            _listener.AuthenticationSchemes = AuthenticationSchemes.Basic;

        }

        public WebServer(Func<HttpListenerRequest, string> method, params string[] prefixes)
            : this(prefixes, method)
        {
        }

        public void Run()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                Console.WriteLine("Webserver running...");
                try
                {
                    while (_listener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem(c =>
                        {
                            var ctx = c as HttpListenerContext;
                            try
                            {
                                if (ctx == null)
                                {
                                    return;
                                }

                                Do_Authentication(ctx.Request, ctx.Response);

                                var twitterFeed =  GetTwitterFeed();

                                var buf = Encoding.UTF8.GetBytes(twitterFeed);
                                ctx.Response.ContentLength64 = buf.Length;
                                ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                            }
                            catch(Exception ex)
                            {
                                // ignored
                                Console.WriteLine(ex.Message);
                            }
                            finally
                            {
                                // always close the stream
                                if (ctx != null)
                                {
                                    ctx.Response.OutputStream.Close();
                                }
                            }
                        }, _listener.GetContext());
                    }
                }
                catch (Exception ex)
                {
                    // ignored
                }
            });
        }

        public void Do_Authentication(HttpListenerRequest request, HttpListenerResponse response)
        {
            string authorization = request.Headers["Authorization"];
            string userInfo;
            string username = "";
            string password = "";
            if (authorization != null)
            {
                byte[] tempConverted = Convert.FromBase64String(authorization.Replace("Basic ", "").Trim());
                userInfo = System.Text.Encoding.UTF8.GetString(tempConverted);
                string[] usernamePassword = userInfo.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                username = usernamePassword[0];
                password = usernamePassword[1];
            }

            if (Validate(username, password))
            {
                //HttpListenerContext.Current.User = new BasicPrincipal(username);
                var rstr = _responderMethod(request);
                var buf = Encoding.UTF8.GetBytes(rstr);
                //response.ContentLength64 = buf.Length;
                //response.OutputStream.Write(buf, 0, buf.Length);
            }
            else
            {
                response.AddHeader("WWW-Authenticate", "Basic realm=\"Test\"");
                response.StatusCode = 401;
                //response.End();
                
            }
            
        }

        public bool Validate(string username, string password)
        {
            return (username == "testuser" && password == "testuser");
        }


        public void Stop()
        {
            _listener.Stop();
            _listener.Close();
        }

        public string GetTwitterFeed()
        {
            var jsonTwitterFeed = string.Empty;

            string query = "salesforce";

            string resource_url = "https://api.twitter.com/1.1/search/tweets.json?q=%40salesforce&src=typd";


            //SET the header for twitter call.
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

                /*var bearer_token1 =
                        "ckt1OWg5OWxaREVjMDZkSEozR2hXc3UwQzpRNlJWc2d2ZEk1dkY1Y1gzempXNGJGVUVRTkZuYWw4YUY3OXJJclRVa2dsZkdKZGNTYw==";
                  */
                HttpWebRequest request1 = (HttpWebRequest)WebRequest.Create("https://api.twitter.com/oauth2/token?grant_type=client_credentials");
                request1.Headers.Add("Authorization", "Basic " + basic_token1);
                request1.Method = "POST";
                request1.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
                var response1 = (HttpWebResponse)request1.GetResponse();
                var reader1 = new StreamReader(response1.GetResponseStream());
                var objText1 = reader1.ReadToEnd();

                var oauth_token1 = JsonConvert.DeserializeObject<oauth_token>(objText1);






                //Get feed from twitter api.
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(resource_url);
            //request.Headers.Add("Authorization", authHeader);
            request.Headers.Add("Authorization", "bearer " + oauth_token1.access_token);
            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";


            var response = (HttpWebResponse)request.GetResponse();
            var reader = new StreamReader(response.GetResponseStream());
            var objText = reader.ReadToEnd();
            //myDiv.InnerHtml = objText;
            /*string html = "";
           
                 JArray jsonDat = JArray.Parse(objText);
                  for (int x = 0; x < jsonDat.Count(); x++)
                  {
                      //html += jsonDat[x]["id"].ToString() + "<br/>";
                      html += jsonDat[x]["text"].ToString() + "<br/>";
                      // html += jsonDat[x]["name"].ToString() + "<br/>";
                      html += jsonDat[x]["created_at"].ToString() + "<br/>";

                  }
                  //myDiv.InnerHtml = html;
             * */
                jsonTwitterFeed = objText.ToString();
            }
            catch (Exception ex)
            {
                //myDiv.InnerHtml = html + twit_error.ToString();
                throw ex;
            }


            //make HTTP request to twitter API to fetch feed.


            return jsonTwitterFeed;

        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }

    internal class Program
    {
        public static string SendResponse(HttpListenerRequest request)
        {
            return string.Format("<HTML><BODY>WelCome {0}!!</BODY></HTML>", DateTime.Now);
        }

        private static void Main(string[] args)
        {
            var ws = new WebServer(SendResponse, "http://localhost:4000/test/");
            ws.Run();
            Console.WriteLine("A simple webserver. Press a key to quit.");
            Console.ReadKey();
            ws.Stop();
        }

        
    }

    public class oauth_token
    {
        public string token_type;
        public string access_token;
    }
}
