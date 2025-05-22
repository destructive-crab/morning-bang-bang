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