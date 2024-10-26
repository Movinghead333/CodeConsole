using NUnit.Framework;
using CodeConsole;
using System.Collections.Generic;

public class CommandParserTests
{
    private CommandParser SetupParser()
    {
        CommandParser parser = new CommandParser();
        parser.RegisterCommand(
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

        return parser;
    }

    [Test]
    public void TestCorrectComandInputOnlyQuotedArgs()
    {
        CommandParser parser = SetupParser();

        CommandInstance ci = parser.TryParseCommandString("cl -ln \"Testlobby\" -mp \"6\"");

        Assert.AreEqual(ci.CommandName, "cl");
        Assert.True(ci.ArgumentInstances.ContainsKey("-ln"));
        Assert.True(ci.ArgumentInstances.ContainsKey("-mp"));
        Assert.AreEqual(ci.ArgumentInstances["-ln"].ArgumentValue, "Testlobby");
        Assert.AreEqual(ci.ArgumentInstances["-mp"].ArgumentValue, 6);
    }

    [Test]
    public void TestCorrectComandInputOnlyNonQuotedArgs()
    {
        CommandParser parser = SetupParser();

        CommandInstance ci = parser.TryParseCommandString("cl -ln Testlobby -mp 6");

        Assert.AreEqual(ci.CommandName, "cl");
        Assert.True(ci.ArgumentInstances.ContainsKey("-ln"));
        Assert.True(ci.ArgumentInstances.ContainsKey("-mp"));
        Assert.AreEqual(ci.ArgumentInstances["-ln"].ArgumentValue, "Testlobby");
        Assert.AreEqual(ci.ArgumentInstances["-mp"].ArgumentValue, 6);
    }

    [Test]
    public void TestCorrectComandInputMixedQuotedAndNonQuotedArgs()
    {
        CommandParser parser = SetupParser();

        CommandInstance ci = parser.TryParseCommandString("cl -ln \"Test Lobby\" -mp 6");

        Assert.AreEqual(ci.CommandName, "cl");
        Assert.True(ci.ArgumentInstances.ContainsKey("-ln"));
        Assert.True(ci.ArgumentInstances.ContainsKey("-mp"));
        Assert.AreEqual(ci.ArgumentInstances["-ln"].ArgumentValue, "Test Lobby");
        Assert.AreEqual(ci.ArgumentInstances["-mp"].ArgumentValue, 6);
    }
}
