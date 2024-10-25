# Code Console

The Code Console package provides a simple plug and play console for Unity Games.

Its main purpose is to execute actions/commands while developing games and other software with Unity. The commands available for execution can be defined in code and can have parameters with varying settings.

# Usage

1. Use the `CodeConsole` prefab contained in this package and drag it into your desired scene to create the Code Console object.
2. Create a C# script and get a reference to the `ConsoleUIC` component of the `CodeConsole` prefab.
3. To register a command use the syntax shown in the following code example:

```csharp
consoleUIC.commandParser.RegisterCommand(
    new CommandDefinition(
        "cl",
        "Create a lobby",
        new List<ArgumentDefinition>()
        {
            new ArgumentDefinition(
                "-ln",
                "lobbyname",
                typeof(string),
                "The name of the lobby.",
                null,
                true
            ),
            new ArgumentDefinition(
                "-mp",
                "maxplayers",
                typeof(int),
                "The maximum number of players allowed to join the lobby.",
                "6",
                false
            )
        }
    )
);
```

4. Reacting to user-input commands can be done by adding a listener to the command received callback. The callback provides an object of type `CommandInstance` giving you all the information about the entered command together with its parameters. The following code example demonstrates this by showing how an input for the command registered in the example above could be handled. (The callback takes the input and creates a lobby using Unity's multiplayer `LobbyServive`.)

```csharp
consoleUIC.AddCommandReceivedListener(async (CommandInstance commandInstance) => {
    if (commandInstance.CommandName == "cl")
    {
        string lobbyName = commandInstance.ArgumentInstances["-ln"].ArgumentValue;
        int maxPlayers = commandInstance.ArgumentInstances["-mp"].ArgumentValue;

        consoleUIC.Log($"Creating lobby with lobby name {lobbyName} and max players {maxPlayers}");

        CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
        {
            IsPrivate = false,
            Player = GeneratePlayerData(),
            Data = new Dictionary<string, DataObject>
            {
                {"GameMode", new DataObject(DataObject.VisibilityOptions.Public, "TDM", DataObject.IndexOptions.S1) },
            }
        };

        try
        {
            currentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);

            PrintPlayersData(currentLobby);

            consoleUIC.Log($"Created Lobby: {LobbyToString(currentLobby)}");
        }
        catch (LobbyServiceException e)
        {
            consoleUIC.Log(e.ToString());
        }
    }
});
```

5. Simply logging to the Code Console is a simple as calling `consoleUIC.Log("your message string here");`
6. The Code Console does also hook into Unity's `UnityOnLogMessageReceived` callback. This allows the Code Console to display logs, warnings and errors issued via the built-in Unity-functions `Debug.Log`, `Debug.LogWarning` and `Debug.LogError`.
