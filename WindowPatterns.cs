using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using WorkIndicator.Properties;

namespace WorkIndicator
{
    public class WindowPatterns : ObservableCollection<WindowPattern>
    {
        public static event EventHandler Changed;

        public static WindowPatterns Load()
        {
            var windowPatterns = Load(Settings.Default.WindowPatterns);

            return windowPatterns;
        }

        private static WindowPatterns Load(string serializedData)
        {
            var windowPatterns = JsonConvert.DeserializeObject<WindowPatterns>(serializedData) ?? new WindowPatterns();

            return windowPatterns;
        }

        public void Save()
        {
            Settings.Default.WindowPatterns = Serialize();

            Settings.Default.Save();

            Changed?.Invoke(this, null);
        }

        private string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }

        public WindowPatterns Clone()
        {
            var data = Serialize();

            return Load(data);
        }
    }
}
