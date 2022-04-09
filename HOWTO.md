# Setup a Dirt Project

## Project

If you want to keep your project loosely coupled with Unity you can follow these steps.

## VS Project creation

1. Set the Dirt.git\nuget folder as a source for visual studio
1. Create your class library project (I use .net framework 4.6)
1. Install the desired packages (Game, Common, Simulation, Network...)
1. For each class library you should ensure to have its binaries exported in a shared folder
    (i.e.: add post build event `xcopy $(TargetPath) $(SolutionDir)..\Libraries\ /Y`)
1. A content folder will be needed, you can copy the Content available on this repo to start with a base

## Unity Project creation

1. Create project with Unity HUB
1. Create a folder to import Dirt collection (Externals/Dirt/Plugins)
1. Import Dirt collection (Dirt.Game, Dirt.Simulation, Dirt.log, Dirt.Unity, Dirt.Unity..Logger) within the created folder
1. Import Newtonsoft
1. Import Editor.Coroutine package from Unity
1. Import Dirt.Unity.Editor
1. Configure Dirt Project Information (Project Settings/Dirt)
1. the csc.rsp should be installed at the top most directory (Assets/) if asmdef are not used.

### Setup Project Watcher (for importing external changes)
Helps auto import dll when updated

1. Configure Project Settings > Dirt > Watcher
1. Make sure to enable it


## Import Utilities (optional)

GameContent.exe - Create a json map based on an input directory
> usage: # GameContent.exe <inputdir> <output>

## Setup Multiplayer

### Headless Server

1. Create a console project
1. Add nuget package Dirt.ServerApplication
1. Configure the App.config
1. Add and configure the following keys
```
    <appSettings>
        <add key="ServerPort" value="{SERVER_PORT}" />
        <add key="MaxClient" value="{MAX_CLIENT}" />
        <add key="PluginFile" value="{PLUGIN_NAME}" />
        <add key="PluginClass" value="{PLUGIN_CLASS}" />
        <add key="ContentRoot" value="{CONTENT_PATH}" />
        <add key="ContentVersion" value="{CONTENT_MAP_NAME}" />
        <add key="WebServerPort" value="{WEB_SERVICE_PORT}" />
        <add key="TickRate" value="{UPDATE_RATE}" />
    </appSettings>
```
1. Use Dirt.ServerApplication.ServerApp class
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

### Unity Client

1. Import Dirt.Unity.Network, Dirt.Network, NetSerializer, Mud within unity project (can be found altogether by building Dirt.Unity.Network project)
1. Use MudConnector system to connect to server
1. Call StartSocket(ip, port, playerName) to connect


