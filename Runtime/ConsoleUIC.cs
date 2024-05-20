using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using System;



namespace CodeConsole
{
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

        public void AddListener(UnityAction<string> callback)
        {
            OnTextInputSubmitted.AddListener(callback);
        }

        public void RemoveListener(UnityAction<string> callback)
        {
            OnTextInputSubmitted.RemoveListener(callback);
        }

        public void AddCommandReceivedListener(UnityAction<CommandInstance> callback)
        {
            OnCommandReceived.AddListener(callback);
        }

        public void RemoveCommandReceivedListener(UnityAction<CommandInstance> callback)
        {
            OnCommandReceived.RemoveListener(callback);
        }

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

        private void Start()
        {
            ConsoleInputField.onSubmit.AddListener(OnSubmitTextInput);
            
        }

        public void OnSendButtonClicked()
        {
            OnSubmitTextInput(ConsoleInputField.text);
        }

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
