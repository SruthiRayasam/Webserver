using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml.Schema;

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
            _listener.AuthenticationSchemes = AuthenticationSchemes.Digest;

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
                            }
                            catch
                            {
                                // ignored
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
                response.ContentLength64 = buf.Length;
                response.OutputStream.Write(buf, 0, buf.Length);
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
}