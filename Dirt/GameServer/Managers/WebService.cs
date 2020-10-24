using Dirt.Game;
using Dirt.GameServer.Web;
using Dirt.Log;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Dirt.GameServer.Managers
{
    using ResponseDelegate = System.Func<HttpListenerRequest, string>;
    using ResponseDelegateInt = System.Func<HttpListenerRequest, int, string>;

    public class WebService : IGameManager
    {
        private bool m_Abort;
        private HttpListener m_Listener;
        private Dictionary<string, ResponseDelegate> m_Routes;

        private List<IWebRouteHandler> m_Handlers;

        public WebService(string host, int port)
        {
            m_Handlers = new List<IWebRouteHandler>();
            m_Listener = new HttpListener();
            m_Listener.Prefixes.Add($"http://{host}:{port}/");
            m_Routes = new Dictionary<string, ResponseDelegate>();
        }

        public async void Start()
        {
            m_Abort = false;
            m_Listener.Start();

            while (!m_Abort)
            {
                HttpListenerContext ctx = await m_Listener.GetContextAsync();
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;
                resp.AppendHeader("Access-Control-Allow-Origin", "*");
                ResponseDelegate responseDelegate = DefaultResponseDelegate;

                if ( req.Url.Segments.Length > 1)
                {
                    string mainRoute = req.Url.Segments[1].Replace("/", "");
                    if (!m_Routes.TryGetValue(mainRoute, out responseDelegate))
                        responseDelegate = DefaultResponseDelegate;
                }

                string serverResponse = responseDelegate(req);
                byte[] encoded = Encoding.UTF8.GetBytes(serverResponse);
                resp.ContentLength64 = encoded.Length;
                resp.OutputStream.Write(encoded, 0, encoded.Length);
                resp.OutputStream.Close();
            }

            m_Listener.Close();
        }


        public void RegisterHandler(IWebRouteHandler routeHandler)
        {
            m_Handlers.Add(routeHandler);
            routeHandler.SetupRoutes(this);
        }
        public void HandleRoute(string route, ResponseDelegate respDelegate)
        {
            Console.Assert(!m_Routes.ContainsKey(route), $"Route {route} already defined");
            m_Routes.Add(route, respDelegate);
            Console.Message($"Added route {route}");
        }

        public void HandleRoute(string route, ResponseDelegateInt respDelegate)
        {
            Console.Assert(!m_Routes.ContainsKey(route), $"Route {route} already defined");
            m_Routes.Add(route, (req) => {
                string[] strParams = req.Url.Segments.Skip(2).Select(s => s.Replace("/", "")).ToArray();
                if (strParams.Length < 1)
                    return DefaultResponseDelegate(req);
                return respDelegate(req, int.Parse(strParams[0]));
            });
            Console.Message($"Added route {route}");
        }
        public void Update(float deltaTime)
        {
        }

        private string DefaultResponseDelegate(HttpListenerRequest request)
        {
            return $"{{\"error\": \"Unknown route {request.Url.AbsolutePath}\"}}";
        }
    }
}
