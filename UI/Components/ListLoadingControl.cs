using System.Linq;
using UnityEngine;

namespace ServerBrowser.UI.Components
{
    public class ListLoadingControl
    {
        private static Transform _controlTemplate;

        private static void LoadTemplate()
        {
            if (_controlTemplate != null)
                return;
            
            var nativeGameServerList = Resources.FindObjectsOfTypeAll<GameServersListTableView>().First();
            
            if (nativeGameServerList == null)
                return;
            
            _controlTemplate = nativeGameServerList.transform.Find("TableView/Viewport/MainLoadingControl");
        }
        
        public static LoadingControl Create(Transform parentTransform)
        {
            LoadTemplate();

            if (_controlTemplate == null)
                return null;
            
            var newObject = Object.Instantiate(_controlTemplate, parentTransform, false);
            
            var loadingControl = newObject.GetComponent<LoadingControl>();
            loadingControl.gameObject.SetActive(true);
            loadingControl.Hide();

            return loadingControl;
        }
    }
}