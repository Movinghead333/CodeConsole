using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CodeConsole
{
    public class CommandParser
    {
        public static Dictionary<Type, Func<string, dynamic>> TypeConverters = new Dictionary<Type, Func<string, dynamic>>
        {
            {typeof(string), (string value) => { return value; } },
            {typeof(int), (string value) => {return int.Parse(value); } },
            {typeof(bool), (string value) => {return bool.Parse(value); } },
            {typeof(float), (string value) => {return float.Parse(value); } },
            {typeof(double), (string value) => {return  double.Parse(value); } },
        };

        Dictionary<string, CommandDefinition> RegistredCommands = new Dictionary<string, CommandDefinition>();

        public void RegisterCommand(CommandDefinition commandDefinition)
        {
            RegistredCommands.Add(commandDefinition.Name, commandDefinition);
        }

        public void UnregisterCommand(CommandDefinition commandDefinition)
        {
            RegistredCommands.Remove(commandDefinition.Name);
        }

        public CommandInstance TryParseCommandString(string commandString)
        {
            string commandRegex = "(?<command>\\w+)(\\s+(?<argumenttag>-\\w+)\\s+(?<argumentvalue>\"[\\w\\s]+\"))*";

            // Example command: "test -x \"test arg1 \" -p \"test arrrge 2\""
            Match match = Regex.Match(commandString, commandRegex);

            Console.WriteLine(match.Groups["command"].Value);
            Console.WriteLine(match.Groups["argumenttag"].Captures.Count);
            for (int i = 0; i < match.Groups["argumenttag"].Captures.Count; i++)
            {
                Console.WriteLine(match.Groups["argumenttag"].Captures[i].Value);
                Console.WriteLine(match.Groups["argumentvalue"].Captures[i].Value);
            }

            if (match.Groups["command"] == null)
            {
                throw new ArgumentException("Provided string does match the command syntax.");
            }

            string commandName = match.Groups["command"].Value;

            if (!RegistredCommands.ContainsKey(commandName))
            {
                throw new InvalidOperationException($"Cannot parse unknown command: <{commandName}>");
            }

            Dictionary<string, string> commandArgumentTagsWithValues = new Dictionary<string, string>();
            for (int i = 0; i < match.Groups["argumenttag"].Captures.Count; i++)
            {
                string argumentTag = match.Groups["argumenttag"].Captures[i].Value;
                string argumentValue = match.Groups["argumentvalue"].Captures[i].Value;

                commandArgumentTagsWithValues[argumentTag] = argumentValue.Substring(1, argumentValue.Length - 2);
            }

            CommandDefinition parsedCommandDefinition = RegistredCommands[commandName];
            Dictionary<string, ArgumentInstance> argumentInstances = new();

            foreach (var commandArgumentWithValue in commandArgumentTagsWithValues)
            {
                // Check if the argument name is contained in the definition of the command
                if (parsedCommandDefinition.ArgumentDefinitions.Keys.Contains(commandArgumentWithValue.Key))
                {
                    ArgumentDefinition parsedArgumentDefinition = parsedCommandDefinition.ArgumentDefinitions[commandArgumentWithValue.Key];
                    Type type = parsedArgumentDefinition.T;
                    //dynamic value = ParseData(type, commandArgumentWithValue.Value);
                    dynamic value = TypeConverters[type](commandArgumentWithValue.Value);

                    argumentInstances.Add(commandArgumentWithValue.Key, new ArgumentInstance(commandArgumentWithValue.Key, value, type));
                }
            }

            List<string> missingRequiredArguments = new();
            foreach (var argumentDefinition in parsedCommandDefinition.ArgumentDefinitions.Values)
            {
                if (argumentInstances.ContainsKey(argumentDefinition.ArgumentTag))
                {
                    continue;
                }

                if (argumentDefinition.Required)
                {
                    missingRequiredArguments.Add(argumentDefinition.ArgumentTag);
                }
                else
                {
                    argumentInstances[argumentDefinition.ArgumentTag] = new ArgumentInstance(argumentDefinition.ArgumentTag, argumentDefinition.DefaultValue, argumentDefinition.T);
                }
            }

            if (missingRequiredArguments.Count > 0)
            {
                throw new InvalidOperationException($"The following required arguments are missing: {string.Join(", ", missingRequiredArguments)}");
            }

            return new CommandInstance(commandName, argumentInstances.Values.ToList());
        }

        public void Test()
        {
            RegisterCommand(
                new CommandDefinition(
                    "createlobby",
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
        }

        //public static dynamic ParseData(Type t, string data)
        //{
        //    if (t == typeof(string))
        //    {
        //        return data;
        //    }
        //    else if (t == typeof(int))
        //    {
        //        return int.Parse(data);
        //    }
        //    else if (t == typeof(bool))
        //    {
        //        return bool.Parse(data);
        //    }
        //    else 
        //    {
        //        throw new ArgumentException("Invalid type cannot be converted.");
        //    }
        //}
    }

    public class CommandDefinition
    {
        public string Name;
        public string HelpText;
        public Dictionary<string, ArgumentDefinition> ArgumentDefinitions;

        public CommandDefinition(string name, string helpText, List<ArgumentDefinition> argumentDefinitions)
        {
            Name = name;
            HelpText = helpText;
            ArgumentDefinitions = argumentDefinitions.ToDictionary((ArgumentDefinition argumentDefinition) => { return argumentDefinition.ArgumentTag; });
        }
    }

    public class CommandInstance
    {
        public string CommandName;
        public Dictionary<string, ArgumentInstance> ArgumentInstances;

        public CommandInstance(string commandName, List<ArgumentInstance> argumentInstances)
        {
            CommandName = commandName;
            ArgumentInstances = argumentInstances.ToDictionary((ArgumentInstance argumentDefinition) => { return argumentDefinition.ArgumentTag; });
        }
    }

    public class ArgumentDefinition
    {
        public string ArgumentTag;
        public string ArgumentName;
        public string HelpText;
        public bool Required;
        public dynamic DefaultValue;
        public Type T;

        public ArgumentDefinition(string argumentTag, string argumentName, Type t, string helpText, string defaultValue, bool required)
        {
            ArgumentTag = argumentTag;
            ArgumentName = argumentName;
            T = t;
            HelpText = helpText;
            // For some reason this does not work in builds
            //DefaultValue =  Convert.ChangeType(defaultValue, t, CultureInfo.InvariantCulture);
            if (!required)
            {
                //DefaultValue = CommandParser.ParseData(t, defaultValue);
                DefaultValue = CommandParser.TypeConverters[t](defaultValue);
            }
            Required = required;
        }
    }

    public class ArgumentInstance
    {
        public string ArgumentTag;
        public dynamic ArgumentValue;
        public Type T;

        public ArgumentInstance(string argumentTag, dynamic argumentValue, Type t)
        {
            ArgumentTag = argumentTag;
            ArgumentValue = argumentValue;
            T = t;
        }
    }


}
