using System.Linq;
using UnityEngine;

namespace ServerBrowser.UI.Components
{
    public class BssbLoadingControl
    {
        #region Template/init
        private static GameObject? _templateCached;

        private static GameObject? Template
        {
            get
            {
                // Intentionally nullable as native GameServersListTableView may be removed from the game at some point
                
                if (_templateCached == null)
                {
                    var nativeGameServerList = Resources.FindObjectsOfTypeAll<GameServersListTableView>()
                        .FirstOrDefault();
                    
                    if (nativeGameServerList != null)
                        _templateCached = nativeGameServerList.transform.Find("TableView/Viewport/MainLoadingControl")
                            .gameObject;
                }

                return _templateCached;
            }
        }

        public static LoadingControl? Create(Transform parent)
        {
            if (Template == null)
                return null;
            
            var clone = Object.Instantiate(Template, parent);
            clone.gameObject.name = "BssbLoadingControl";
            clone.gameObject.SetActive(true);

            var loadingControl = clone.GetComponent<LoadingControl>();
            loadingControl.gameObject.SetActive(true);
            loadingControl.Hide();
            return loadingControl;
        }
        #endregion
    }
}