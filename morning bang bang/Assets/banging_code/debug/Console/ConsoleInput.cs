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

        public ConsoleInput(TMP_InputField inputField)
        {
            inputFieldInstance = inputField;
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
                    if(part.Contains("."))
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
            InputService.EnterUIMode();
            
            Debug.Log("Console enabled");
        }

        public void Disable()
        {
            inputFieldInstance.DeactivateInputField();
            InputService.EnterPlayerMode();
            Debug.Log("Console disabled");
        }

        public void Clear()
        {
            
        }
    }
}