using System;
using UnityEngine;

namespace Configgy
{
    public static class ModalDialogue
    {
        /// <summary>
        /// Shows a simple dialogue with a title, message, and two options. Invokes callback if "confirm" is clicked, false if "deny" is clicked.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="onComplete"></param>
        /// <param name="confirm"></param>
        /// <param name="deny"></param>
        public static void ShowSimple(string title, string message, Action<bool> onComplete, string confirm = "Yes", string deny = "No")
        {
            if (ModalDialogueManager.Instance == null)
            {
                ConfiggyPlugin.Log.LogError("No ModalDialogueManager found in scene.");
                return;
            }

            ModalDialogueEvent dialogueEvent = new ModalDialogueEvent();
            dialogueEvent.Title = title;
            dialogueEvent.Message = message;
            dialogueEvent.Options = new DialogueBoxOption[]
            {
                new DialogueBoxOption()
                {
                    Name = confirm,
                    Color = Color.white,
                    OnClick = () => onComplete?.Invoke(true)
                },
                new DialogueBoxOption()
                {
                    Name = deny,
                    Color = Color.white,
                    OnClick = () => onComplete?.Invoke(false)
                }
            };

            ModalDialogueManager.Instance.RaiseDialogue(dialogueEvent);
        }

        /// <summary>
        /// Shows the modal dialogue event provided.
        /// </summary>
        /// <param name="modalDialogueEvent">Raises the given dialogue event.</param>
        public static void ShowDialogue(ModalDialogueEvent modalDialogueEvent)
        {
            if (ModalDialogueManager.Instance == null)
            {
                ConfiggyPlugin.Log.LogError("No ModalDialogueManager found in scene.");
                return;
            }

            ModalDialogueManager.Instance.RaiseDialogue(modalDialogueEvent);
        }

        /// <summary>
        /// Shows a dialogue with a title, message, and a list of options.
        /// </summary>
        /// <param name="title">A message header</param>
        /// <param name="message">The message itself</param>
        /// <param name="options">Dialogue Options that will be shown as buttons.</param>
        public static void ShowComplex(string title, string message, params DialogueBoxOption[] options)
        {
            if (ModalDialogueManager.Instance == null)
            {
                ConfiggyPlugin.Log.LogError("No ModalDialogueManager found in scene.");
                return;
            }

            ModalDialogueManager.Instance.RaiseDialogue(new ModalDialogueEvent()
            {
                Title = title,
                Message = message,
                Options = options
            });
        }

        //Next update :p

        ///// <summary>
        ///// Shows an InputField dialogue with a title, message, and a submit button.
        ///// </summary>
        ///// <param name="title">The title of your message</param>
        ///// <param name="message">The message to display as the body</param>
        ///// <param name="onSubmit">Invokes delegate with true if the user clicked submit or false if the dialogue was exited along with the text inputed by the user</param>
        ///// <param name="onChanged">Invokes delegate every time the input string has changed.</param>
        //public static void InputField(string title, string message, Action<bool, string> onSubmit, Action<string> onChanged = null)
        //{
        //    if(ModalDialogueManager.Instance == null)
        //    {
        //        ConfiggyPlugin.Log.LogError("No ModalDialogueManager found in scene.");
        //        return;
        //    }

        //    //TODO: Implement input field dialogue.
        //}

        ///// <summary>
        ///// Shows a Slider dialogue with a title, message, and a submit button.
        ///// </summary>
        ///// <param name="title">The title of your message</param>
        ///// <param name="message">The body of your message</param>
        ///// <param name="onComplete">invokes delegate on dialogue complete with a confirm or deny as the boolean and the float value of the slider</param>
        ///// <param name="onInstance">invoked when the slider is shown</param>
        ///// <param name="onChanged">invoked when the slider value changes</param>
        //public static void Slider(string title, string message, Action<bool, float> onComplete, Action<Slider> onInstance = null, Action<float> onChanged = null)
        //{
        //    if (ModalDialogueManager.Instance == null)
        //    {
        //        ConfiggyPlugin.Log.LogError("No ModalDialogueManager found in scene.");
        //        return;
        //    }
        //}


    }
}
