using System.Collections.Generic;
using System.Text;
using ServerBrowser.UI.Toolkit;
using ServerBrowser.UI.Toolkit.Components;

namespace ServerBrowser.Models
{
    public class ServerFilterParams
    {
        #region Param def
        
        public class Param
        {
            public readonly string Key;
            public readonly string Label;
            public readonly bool InitialValue;
        
            public Param(string key, string label, bool initialInitialValue = false)
            {
                Key = key;
                Label = label;
                InitialValue = initialInitialValue;
            }

            public TkToggleControl CreateControl(LayoutContainer container)
            {
                var control = container.AddToggleControl();
                control.SetLabel(Label);
                control.SetValue(InitialValue);
                return control;
            }
        }
        
        #endregion

        #region Static params list
        
        public static readonly List<Param> AllParams;

        public const string HidePlayingLevel = "HidePlayingLevel";
        public const string HideFull = "HideFull";
        public const string HideEmpty = "HideEmpty";
        public const string HideOfficial = "HideOfficial";
        public const string HideQuickPlay = "HideQuickPlay";

        static ServerFilterParams()
        {
            AllParams = new()
            {
                new Param(HidePlayingLevel, "Hide Playing Level"),
                new Param(HideFull, "Hide Full"),
                new Param(HideEmpty, "Hide Empty"),
                new Param(HideOfficial, "Hide Official"),
                new Param(HideQuickPlay, "Hide Quick Play"),
            };
        }
        
        #endregion

        #region Param values object
        
        private readonly List<string> _params;
        
        public ServerFilterParams()
        {
            _params = new();
            Clear(); // set initial values
        }
        
        public void Clear()
        {
            _params.Clear();
            
            foreach (var param in AllParams)
                if (param.InitialValue)
                    _params.Add(param.Key);
        }
        
        public void SetValue(string key, bool value)
        {
            if (value)
            {
                if (!_params.Contains(key))
                    _params.Add(key);
            }
            else
            {
                if (_params.Contains(key))
                    _params.Remove(key);
            }
        }

        public bool GetValue(string key) => _params.Contains(key);
        
        public bool GetValue(Param param) => GetValue(param.Key);

        public string Describe()
        {
            var sb = new StringBuilder();
            foreach (var param in AllParams)
            {
                if (!GetValue(param.Key))
                    continue;
                
                if (sb.Length > 0)
                    sb.Append(", ");
                sb.Append(param.Label);
            }
            return sb.ToString();
        }
        
        #endregion
    }
}