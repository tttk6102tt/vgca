namespace Sign.itext.text
{
    public class TabSettings
    {
        public const float DEFAULT_TAB_INTERVAL = 36f;

        private List<TabStop> tabStops = new List<TabStop>();

        private float tabInterval = 36f;

        public virtual List<TabStop> TabStops
        {
            get
            {
                return tabStops;
            }
            set
            {
                tabStops = value;
            }
        }

        public virtual float TabInterval
        {
            get
            {
                return tabInterval;
            }
            set
            {
                tabInterval = value;
            }
        }

        public static TabStop getTabStopNewInstance(float currentPosition, TabSettings tabSettings)
        {
            if (tabSettings != null)
            {
                return tabSettings.GetTabStopNewInstance(currentPosition);
            }

            return TabStop.NewInstance(currentPosition, 36f);
        }

        public TabSettings()
        {
        }

        public TabSettings(List<TabStop> tabStops)
        {
            this.tabStops = tabStops;
        }

        public TabSettings(float tabInterval)
        {
            this.tabInterval = tabInterval;
        }

        public TabSettings(List<TabStop> tabStops, float tabInterval)
        {
            this.tabStops = tabStops;
            this.tabInterval = tabInterval;
        }

        public virtual TabStop GetTabStopNewInstance(float currentPosition)
        {
            TabStop tabStop = null;
            if (tabStops != null)
            {
                foreach (TabStop tabStop2 in tabStops)
                {
                    if ((double)(tabStop2.Position - currentPosition) > 0.001)
                    {
                        tabStop = new TabStop(tabStop2);
                        break;
                    }
                }
            }

            if (tabStop == null)
            {
                tabStop = TabStop.NewInstance(currentPosition, tabInterval);
            }

            return tabStop;
        }
    }
}
