using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using System;


namespace CodeConsole
{
    /// <summary>
    /// The <c>ConsoleUIC</c> short for ConsoleUserInterfaceController is the current main Component handling the
    /// operations of the CodeConsole Prefab.
    /// </summary>
    public class ConsoleUIC : MonoBehaviour
    {
        public bool ShowTimestamps = true;

        public CommandParser commandParser = new CommandParser();

        [SerializeField]
        private TMP_Text ConsoleText;

        [SerializeField]
        private TMP_InputField ConsoleInputField;

        [SerializeField]
        private ScrollRect ConsoleTextScrollRect;

        private string ConsoleContent = "";

        private UnityEvent<string> OnTextInputSubmitted = new();

        private UnityEvent<CommandInstance> OnCommandReceived = new();

        /// <summary>
        ///  Listen to new text messages being submitted to the console.
        /// </summary>
        /// <param name="callback">Handler callback which receives a new string submitted to the console.</param>
        public void AddListener(UnityAction<string> callback)
        {
            OnTextInputSubmitted.AddListener(callback);
        }

        /// <summary>
        /// Remove a previous added listener.
        /// </summary>
        /// <param name="callback">The callback to be removed.</param>
        public void RemoveListener(UnityAction<string> callback)
        {
            OnTextInputSubmitted.RemoveListener(callback);
        }

        /// <summary>
        /// Add a listener to act upon parsed commands.
        /// </summary>
        /// <param name="callback">A callback which receives a <c>CommandInstance</c> of command given by a previously registered <c>CommandDefinition</c></param>
        public void AddCommandReceivedListener(UnityAction<CommandInstance> callback)
        {
            OnCommandReceived.AddListener(callback);
        }

        /// <summary>
        /// Remove a previously added listener.
        /// </summary>
        /// <param name="callback">The callback to be removed.</param>
        public void RemoveCommandReceivedListener(UnityAction<CommandInstance> callback)
        {
            OnCommandReceived.RemoveListener(callback);
        }

        /// <summary>
        /// Log a string to the console.
        /// </summary>
        /// <param name="text">The text to be logged.</param>
        public void Log(string text)
        {
            ConsoleContent += $"{text}\n";
            ConsoleText.text = ConsoleContent;
            StartCoroutine(ForceScrollDown());
        }

        private void OnEnable()
        {
            Application.logMessageReceived += UnityOnLogMessageReceived;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= UnityOnLogMessageReceived;
        }

        private void UnityOnLogMessageReceived(string logMessage, string stackTrace, LogType type)
        {
            if (type == LogType.Log)
            {
                Log(logMessage);
            }
            else if (type == LogType.Error || type == LogType.Assert || type == LogType.Exception)
            {
                Log($"<color=\"red\">{logMessage}</color>"); 
            }
            else if (type == LogType.Warning)
            {
                Log($"<color=\"yellow\">{logMessage}</color>");
            }
        }

        // Handle command submission via the onSubmit event of the text input field
        private void Start()
        {
            ConsoleInputField.onSubmit.AddListener(OnSubmitTextInput);
        }

        // Handle command submission via the Send button
        public void OnSendButtonClicked()
        {
            OnSubmitTextInput(ConsoleInputField.text);
        }

        // Process a command entered by the user
        private void OnSubmitTextInput(string textInput)
        {
            // Update console window
            string timestamp = ShowTimestamps ? DateTime.Now.TimeOfDay.ToString("hh':'mm':'ss") : "";
            ConsoleContent += $"[{timestamp}]: {textInput}\n";
            ConsoleText.text = ConsoleContent;
            ConsoleInputField.text = "";
            ConsoleInputField.Select();
            ConsoleInputField.ActivateInputField();

            try
            {
                CommandInstance commandInstance = commandParser.TryParseCommandString(textInput);
                if (commandInstance != null)
                {
                    OnCommandReceived?.Invoke(commandInstance);
                }
            }
            catch (Exception e)
            {
                Log(e.Message);
            }

            OnTextInputSubmitted?.Invoke(textInput);

            StartCoroutine(ForceScrollDown());
        }

        private IEnumerator ForceScrollDown()
        {
            // Wait for end of frame AND force update all canvases before setting to bottom.
            yield return new WaitForEndOfFrame();
            Canvas.ForceUpdateCanvases();
            ConsoleTextScrollRect.normalizedPosition = Vector3.zero;
            Canvas.ForceUpdateCanvases();
        }
    }
}
