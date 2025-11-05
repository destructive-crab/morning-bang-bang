using TMPro;

namespace banging_code.debug.Console
{
    public sealed class ConsoleOutput
    {
        private TMP_Text text;

        public ConsoleOutput(TMP_Text text)
        {
            this.text = text;
        }

        public void Refresh(string[] outputHistory)
        {
            Clear();
            for (var i = 0; i < outputHistory.Length; i++)
            {
                PushFromNewLine(outputHistory[i]);
            }
        }
        
        public void PushFromNewLine(string line)
        {
            text.text += "\n";
            text.text += line;
        }

        public void Clear()
        {
            text.text = "";
        }
    }
}