using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HomeServerApp.Utils
{
    public static class RouteHandlers
    {
        public static void RegisterServerRoutes()
        {
            RouteRegistry.RegisterGetRoute("/", HandleHomePageRequest);
        }

        public static void HandleHomePageRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            string content = "<h1>Welcome to the Character Generator!</h1>";
            byte[] buffer = Encoding.UTF8.GetBytes(content);
            response.ContentType = "text/html";
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
        }
    }
}
