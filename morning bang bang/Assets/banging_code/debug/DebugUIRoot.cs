using banging_code.debug.Console;
using TMPro;
using UnityEngine;

namespace banging_code.debug
{
    public sealed class DebugUIRoot : MonoBehaviour
    {
        public ConsoleView ConsoleView { get; private set; }
        public TMP_Text FPSText { get; private set; }

        private void Awake()
        {
            ConsoleView = GetComponentInChildren<ConsoleView>();
            FPSText = transform.Find("FPS").GetComponent<TMP_Text>();
        }
    }   
}
