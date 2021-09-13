# HOWTO

## Setup a Unity Network Project using Dirt

### Unity
1. Create a Unity project
2. Installs Dirt Plugins (Dirt.*.dll, Newtonsoft, NetSerializer)
3. Install Dirt Editor framework

### Headless Server
1. Create a Console Application (.Net framework 4.6.1)
2. Install nuget packages Dirt.ServerApplication (and dependencies)
3. Setup your App.config
```
    <add key="ServerPort" value="{SERVER_PORT}" />
    <add key="MaxClient" value="{MAX_CLIENT}" />
    <add key="PluginFile" value="{PLUGIN_NAME}" />
    <add key="PluginClass" value="{PLUGIN_CLASS}" />
    <add key="ContentRoot" value="{CONTENT_PATH}" />
    <add key="ContentVersion" value="{CONTENT_MAP_NAME}" />
    <add key="WebServerPort" value="{WEB_SERVICE_PORT}" />
    <add key="TickRate" value="{UPDATE_RATE}" />
```
4. Use Dirt.ServerApplication.ServerApp class
```
using Dirt.ServerApplication;

namespace Magic.GameServer
{
    public class Program
    {
        static void Main(string[] args)
        {
            ServerApp app = new ServerApp();
            app.Run();
        }
    }
}
```