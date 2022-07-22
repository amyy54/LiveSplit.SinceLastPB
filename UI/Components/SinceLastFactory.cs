using LiveSplit.Model;
using LiveSplit.UI.Components;
using System;

[assembly: ComponentFactory(typeof(SinceLastFactory))]

namespace LiveSplit.UI.Components
{
    public class SinceLastFactory : IComponentFactory
    {
        public string ComponentName => "Since Last PB";

        public string Description => "Displays how many attempts or days since the last Personal Best. (Made by Minibeast)";

        public ComponentCategory Category => ComponentCategory.Information; 

        public IComponent Create(LiveSplitState state) => new SinceLast(state);

        public string UpdateName => ComponentName;

        public string XMLURL => "https://minibeast.github.io/files/LiveSplit.SinceLastPB/update.LiveSplit.SinceLastPB.xml";

        public string UpdateURL => "https://minibeast.github.io/files/";

        public Version Version => Version.Parse("1.0.2");
    }
}
