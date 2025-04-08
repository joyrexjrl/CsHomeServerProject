using System;
using System.Collections.Generic;
using System.Reflection;
using System.Net;
using System.Threading.Tasks;

namespace HomeServerApp.Utils
{
    public static class RouteRegistry
    {
        static readonly Dictionary<string, Action<HttpListenerRequest, HttpListenerResponse>> _routeHandlers
            = new Dictionary<string, Action<HttpListenerRequest, HttpListenerResponse>>();

        public static void RegisterGetRoute(string route, Action<HttpListenerRequest, HttpListenerResponse> handler) => _routeHandlers[route] = handler;

        public static void RegisterPostRoute(string method, string route, Action<HttpListenerRequest, HttpListenerResponse> handler) => _routeHandlers[route] = handler;

        public static bool TryHandleRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            string rawUrl = request.RawUrl.Split('?')[0];
            if (_routeHandlers.TryGetValue(rawUrl, out var handler))
            {
                handler(request, response);
                return true;
            }
            return false;
        }
    }
}
