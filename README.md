
# MiniServerList

**MiniServerList** is a simple Unity client/server project to keep an online list of your currently running/hosted unity games.

![](https://github.com/Cpasjuste/MiniServerList/raw/master/Assets/MiniServerList/Scenes/TestScene.png)

## Installation

- Client: copy the "MiniServerList" folder to your "Assets" folder
- Server: open the project in Unity, build/export (in headless mode) the "MiniServerList" scene then run it on your dedicated server.

## Client usage

- Attach the "[MiniClient.cs](https://github.com/Cpasjuste/MiniServerList/blob/master/Assets/MiniServerList/Scripts/MiniClient.cs)" script to a GameObject

- Register/update your game to your hosted "MiniServerList" with "MiniClient.Register"
	 ```
	if (MiniClient.Register(miniserver.ip, miniserver.port, miniHostData))
	{
		Debug.Log("MiniClient: registration success");
	}
	```
- Unregister your game from your hosted "MiniServerList" with "MiniClient.UnRegister"
	 ```
	if (MiniClient.UnRegister(miniserver.ip, miniserver.port, miniHostData))
	{
		Debug.Log("MiniClient: un-registration success");
	}
	```
- Get a list of running games from your hosted "MiniServerList" with "MiniClient.GetServerList"
	```
	List<MiniHostData> serverList = MiniClient.GetServerList(miniserver.ip, miniserver.port);
	Debug.Log("MiniClient: servers found: " + serverList.Count);
	```
- See [MiniClient.cs](https://github.com/Cpasjuste/MiniServerList/blob/master/Assets/MiniServerList/Scripts/MiniClient.cs) for a working example, and/or run the "TestScene" from unity editor (be sure to enable the "Testing" option from "Client" GameObject).

## Notes

- If your game data change in-game ([MiniHostData](https://github.com/Cpasjuste/MiniServerList/blob/master/Assets/MiniServerList/Scripts/MiniHostData.cs)), be sure to update your game to your hosted “MiniServerList” with “MiniClient.Register” so the “MiniServerList” server is aware of you changes.
- “MiniServerList” server will check game list for connectivity every minutes by default. If a game is not running anymore, it will automatically remove it from the game list.
- "[MiniUtility.cs](https://github.com/Cpasjuste/MiniServerList/blob/master/Assets/MiniServerList/Scripts/MiniUtility.cs)" have a "GetIpInfo" function. This use [ipinfo](http://ipinfo.io/json) (or [icanhazip](http://icanhazip.com/) if the previous fail) to get your real public address. You should probably use it when setting your [MiniHostData](https://github.com/Cpasjuste/MiniServerList/blob/master/Assets/MiniServerList/Scripts/MiniHostData.cs) ip address to ensure your game public address is correct. See [MiniClient.cs](https://github.com/Cpasjuste/MiniServerList/blob/master/Assets/MiniServerList/Scripts/MiniClient.cs) for an example. Please note that if you are being a router, you'll have to do some port forwarding as "MiniServerList" does not handle nat transversal. 
- Client <-> Server communication are encrypted, see [MiniUtility.cs](https://github.com/Cpasjuste/MiniServerList/blob/master/Assets/MiniServerList/Scripts/MiniUtility.cs) to change the default encryption key.
