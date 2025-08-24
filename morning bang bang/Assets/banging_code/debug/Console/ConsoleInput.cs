using System;
using System.Collections.Generic;
using MothDIed.InputsHandling;
using TMPro;
using UnityEngine;

namespace banging_code.debug.Console
{
    public class ConsoleInput
    {
        public string CurrentInput => inputFieldInstance.text;
        private readonly TMP_InputField inputFieldInstance;

        public event Action<string> OnSubmit;
        public event Action<object[]> OnSubmitParsed;
        
        public ConsoleInput(TMP_InputField inputField)
        {
            inputFieldInstance = inputField;
            
            inputFieldInstance.onSubmit.AddListener((command) =>
            {
                if(command == "") return;
                
                OnSubmit?.Invoke(command);
                OnSubmitParsed?.Invoke(GetParsed());
            });
        }

        public object[] GetParsed()
        {
            if (CurrentInput == "") return null;
            
            string[] parts = CurrentInput.Split(" ");
            List<object> fullCommand = new();
            
            foreach (string part in parts)
            {
                if (float.TryParse(part, out Single floatArg))
                {
                    if(part.Contains(","))
                    {
                        fullCommand.Add(floatArg);
                    }
                    else if(int.TryParse(part, out Int32 intArg))
                    {
                        fullCommand.Add(intArg);
                    }
                }
                else
                {
                    switch (part)
                    {
                        case "true":
                            fullCommand.Add(true);
                            break;
                        case "false":
                            fullCommand.Add(false);
                            break;
                        default:
                            fullCommand.Add(part);
                            break;
                    }
                }
            }

            return fullCommand.ToArray();
        }
        
        public void Enable()
        {
            inputFieldInstance.ActivateInputField();
            InputService.SwitchTo(InputService.Mode.UI);
        }

        public void Disable()
        {
            inputFieldInstance.DeactivateInputField();
            InputService.BackToPreviousMode();
        }

        public void Clear()
        {
            
        }
    }
}