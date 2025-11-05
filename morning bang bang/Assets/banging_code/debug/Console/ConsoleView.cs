using System.Linq;
using MothDIed;
using MothDIed.InputsHandling;
using TMPro;
using UnityEngine;

namespace banging_code.debug.Console
{
    public class ConsoleView : MonoBehaviour
    {
        [SerializeField] private Transform uiRoot;
        
        [SerializeField] private TMP_Text outputText;
        [SerializeField] private TMP_InputField inputField;

        public State CurrentState { get; private set; } = State.Disabled;

        private ConsoleOutput output;

        private ConsoleInput input;

        private void Awake()
        {
            output = new ConsoleOutput(outputText);
            input = new ConsoleInput(inputField);
            
            InputService.OnConsoleSwitchPressed += Switch;
            
            input.OnSubmitParsed += ProcessCommandSubmission;
        }

        private void OnDisable() => InputService.OnConsoleSwitchPressed -= Switch;

        private void OnDestroy() => InputService.OnConsoleSwitchPressed -= Switch;

        private void ProcessCommandSubmission(object[] objects)
        {
            if(objects[0] is not string) return;
            
            if (Game.TG<BangDebugger>(out var debugger))
            {
                debugger.Console.InvokeCommand(true, objects[0] as string, objects.Skip(1).ToArray());
            }
        }

        private void Switch()
        {
            switch (CurrentState)
            {
                case State.Focused:
                    Idle();
                    break;
                case State.Idle:
                    Disable();
                    break;
                case State.Disabled:
                    Focus();
                    break;
            }
        }

        public void PushNewMessage(string message)
        {
            output.PushFromNewLine(message);
        }

        public void Focus()
        {
            uiRoot.gameObject.SetActive(true);
            input.Enable();
            output.Refresh(Game.GDb<BangDebugger>().Console.OutputHistory);

            CurrentState = State.Focused;
        }

        public void Idle()
        {
            input.Disable();
            CurrentState = State.Idle;
        }

        public void Disable()
        {
            uiRoot.gameObject.SetActive(false);
            CurrentState = State.Disabled;
        }

        public enum State
        {
            Focused,
            Idle,
            Disabled
        }
    }
}