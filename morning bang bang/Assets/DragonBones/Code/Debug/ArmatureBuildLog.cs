using System.Collections.Generic;
using System.Linq;

namespace DragonBones.Code.Debug
{
    public sealed class ArmatureBuildLog
    {
        class Section
        {
            public Section Parent;
            public string name;
            public string tabs;
            public const string tab = ">    ";

            public Section(Section parent, string name)
            {
                tabs = tab;
                if (parent != null)
                {
                    Parent = parent;
                    tabs += Parent.tabs;   
                }
                this.name = name;
            }
        }

        public ArmatureBuildLog Parent;

        public readonly string name;
        private string log="";
        
        private Section currentSection = null;
        private Section initialSection;
        
        public ArmatureBuildLog(string logName)
        {
            name = logName;
            initialSection = new Section(null, logName);
            currentSection = initialSection;
            PushLine($"[SECTION START] [{logName}]");
        }

        public void AddEntry(string what, string who, string additionalInfo = "")
        {
             PushLine(what  + ": " + who + $" ({additionalInfo});");
        }

        public void StartSection(string sectionName)
        {
            PushLine($"[SECTION START] [{sectionName}]");
            currentSection = new Section(currentSection, sectionName);
        }

        public void EndSection()
        {
            currentSection = currentSection.Parent;
            PushLine("[SECTION END]");
        }

        public void FinishLog()
        {
            EndSection(currentSection);
        }

        private void EndSection(Section section)
        {
            do
            {
                EndSection();
            }
            while (currentSection != section.Parent);
        }

        private void PushLine(string line)
        {
            log += "\n";
            if(currentSection != null) log += currentSection.tabs + line;
            if (currentSection == null) log += line;
        }

        public override string ToString() => log;
    }
}