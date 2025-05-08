# GameLiftServerSdk CSharp
## Documentation
You can find the official Amazon GameLift documentation [here](https://aws.amazon.com/documentation/gamelift/).

## Line Endings
Please make sure that the line endings are LF and not CRLF, especially in the .sh scripts and config files. 
If they are in CRLF, this will make prefabs fail in becoming available, causing a SEV2. 

## Building the SDK
### Minimum Requirements:
* Visual Studio 2015 or later
* Xamarin Studio 6.1 or later
* Mono develop 6.1 or later

The package contains one solution: 
* GameLiftServerSDK.sln for .Net framework 4.6

To build, simply load up the solution in one of the supported IDEs, restore the Nuget packages and build it from there.

### Configuring the C# linter

Each linter option is specified by a series of letters and numbers, like S5753. Disable options globally in GameLiftServerSDK.ruleset, or disable them locally using:

```csharp
#pragma warning disable S1147 // Disables S1147 until it is re-enabled using 'restore' after
            Environment.Exit(-1);
#pragma warning restore S1147
```

If you wish to know more about a SonarLint option, search it up here: https://rules.sonarsource.com/csharp/RSPEC-6354, but replace 6354 with the number after the "S" of the option you want to search. Eg for "S1147", it would be https://rules.sonarsource.com/csharp/RSPEC-1147

## Unity
### Add Amazon GameLift libraries to Unity Editor
The 4.6 solution is compatible with Unity. In the Unity Editor, import the following libraries produced by the build. Be sure to pull all the DLLs into the Assets/Plugins directory:
* GameLiftServerSDK.dll
* log4net.dll
* Newtonsoft.Json.dll
* Polly.dll
* System.Buffers.dll
* System.Collections.Immutable.dll
* System.Memory.dll
* System.Runtime.CompilerServices.Unsafe.dll
* websocket-sharp.dll

#### Newtonsoft.Json conflict

If your Unity project already declares dependencies on Newtonsoft.Json, skip pulling in **Newtonsoft.Json.dll** from the Amazon GameLift Server SDK, and use Newtonsoft.Json from Unity instead.

### Check Scripting Runtime Version setting
Make sure you set the Scripting Runtime Version to the .Net solution you're using. 
Otherwise, Unity will throw errors when importing the DLLs.  
From the Unity editor, go to:  
File->Build Settings->Player Settings. Under Other Settings->Configuration->Scripting Runtime Version

At this point, you should be ready to start playing with the SDK!

### Example code
Below is a simple MonoBehavior that showcases a simple game server initialization with Amazon GameLift.
```csharp
using UnityEngine;
using Aws.GameLift.Server;
using System.Collections.Generic;

public class GameLiftServerExampleBehavior : MonoBehaviour
{
    //This is an example of a simple integration with Amazon GameLift server SDK that will make game server processes go active on Amazon GameLift!
    public void Start()
    {
        //Identify port number (hard coded here for simplicity) the game server is listening on for player connections
        var listeningPort = 7777;

        //WebSocketUrl from RegisterHost call
        var webSocketUrl = "wss://us-west-2.api.amazongamelift.com";

        //Unique identifier for this process
        var processId = "myProcess";

        //Unique identifier for your host that this process belongs to
        var hostId = "myHost";

        //Unique identifier for your fleet that this host belongs to
        var processId = "myFleet";

        ServerParameters serverParameters = new ServerParameters(
            webSocketUrl,
            processId,
            hostId,
            fleetId);

        //InitSDK will establish a local connection with Amazon GameLift's agent to enable further communication.
        var initSDKOutcome = GameLiftServerAPI.InitSDK(serverParameters);
        if (initSDKOutcome.Success)
        {
            ProcessParameters processParameters = new ProcessParameters(
                (gameSession) => {
                    //When a game session is created, Amazon GameLift sends an activation request to the game server and passes along the game session object containing game properties and other settings.
                    //Here is where a game server should take action based on the game session object.
                    //Once the game server is ready to receive incoming player connections, it should invoke GameLiftServerAPI.ActivateGameSession()
                    GameLiftServerAPI.ActivateGameSession();
                },
                (updateGameSession) => {
                    //When a game session is updated (e.g. by FlexMatch backfill), Amazon GameLift sends a request to the game
                    //server containing the updated game session object.  The game server can then examine the provided
                    //matchmakerData and handle new incoming players appropriately.
                    //updateReason is the reason this update is being supplied.
                },
                () => {
                    //OnProcessTerminate callback. Amazon GameLift will invoke this callback before shutting down an instance hosting this game server.
                    //It gives this game server a chance to save its state, communicate with services, etc., before being shut down.
                    //In this case, we simply tell Amazon GameLift we are indeed going to shutdown.
                    GameLiftServerAPI.ProcessEnding();
                }, 
                () => {
                    //This is the HealthCheck callback.
                    //GameLift will invoke this callback every 60 seconds or so.
                    //Here, a game server might want to check the health of dependencies and such.
                    //Simply return true if healthy, false otherwise.
                    //The game server has 60 seconds to respond with its health status. Amazon GameLift will default to 'false' if the game server doesn't respond in time.
                    //In this case, we're always healthy!
                    return true;
                },
                listeningPort, //This game server tells Amazon GameLift that it will listen on port 7777 for incoming player connections.
                new LogParameters(new List<string>()
                {
                    //Here, the game server tells Amazon GameLift what set of files to upload when the game session ends.
                    //Amazon GameLift will upload everything specified here for the developers to fetch later.
                    "/local/game/logs/myserver.log"
                }));

            //Calling ProcessReady tells Amazon GameLift this game server is ready to receive incoming game sessions!
            var processReadyOutcome = GameLiftServerAPI.ProcessReady(processParameters);
            if (processReadyOutcome.Success)
            {
                print("ProcessReady success.");
            }
            else
            {
                print("ProcessReady failure : " + processReadyOutcome.Error.ToString());
            }
        }
        else
        {
            print("InitSDK failure : " + initSDKOutcome.Error.ToString());
        }
    }

    void OnApplicationQuit()
    {
        //Make sure to call GameLiftServerAPI.Destroy() when the application quits. This resets the local connection with Amazon GameLift's agent.
        GameLiftServerAPI.Destroy();
    }
}
```
