using Dirt.GameServer.Managers;

namespace Dirt.GameServer.Web
{
    public interface IWebRouteHandler
    {
        void SetupRoutes(WebService webServer);
    }
}
