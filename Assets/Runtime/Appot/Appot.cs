using UnityEngine;
using System.Collections;

namespace ProjectX
{
    public class Appot : MonoBehaviour
    {
        public enum Store
        {
            AppStore,
            GooglePlay,
            Amazon,
            XiaoMi,
        }

        private const string msAppotURL = "http://47.88.12.232:8080/appot/";
        private static string[] msStoreId = new string[]
        {
            "1001", "2002", "2003", "2004"
        };

        public Store store = Store.AppStore;
        public string appId = "000000";

        public string productId 
        {
            get { return this.ThisProductId(this.store, this.appId); }
        }

        public IEnumerator LoginAsync(System.Action<WWW> onFinish = null)
        {
            string deviceGuid = SystemInfo.deviceUniqueIdentifier;
            string deviceName = SystemInfo.deviceName;

            WWWForm form = new WWWForm();
            form.AddField("device_guid", deviceGuid);
            form.AddField("device_name", deviceName);
            form.AddField("product_id", this.productId);
            WWW www = new WWW(msAppotURL + "login_v3.php", form);
            yield return www;

            this.InvokeAction(onFinish, www);
            if(!string.IsNullOrEmpty(www.error))
            {
                Debug.LogError(www.error);
            }
        }

        public IEnumerator MoreGameAsync(System.Action<WWW> onFinish = null)
        {
            WWWForm form = new WWWForm();
            form.AddField("product_id", this.productId);
            WWW www = new WWW(msAppotURL + "moregame.php", form);
            yield return www;

            this.InvokeAction(onFinish, www);          
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.LogError(www.error);
            }
        }      

        private string ThisProductId(Store store, string appid)
        {
            if (appid.Length < 6)
                throw new System.ArgumentException("Format of argument appid is invalid.");
            return msStoreId[(int)store] + appid;
        }

        private void InvokeAction(System.Action<WWW> action, WWW www)
        {
            if (action == null)
                return;
            action.Invoke(www);
        }
    }
}
