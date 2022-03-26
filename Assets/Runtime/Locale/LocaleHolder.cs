using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ProjectX
{
    public class LocaleHolder : MonoBehaviour
    {
        public enum Locale
        {
            ZH_CN,
            ZH_TW,
            EN_US,
        }
        
        public string baseURL = XUtility.StreamingAssetsPath("Locale");
        public Locale locale = Locale.EN_US;
        
        private XTable data = new XTable();

        public string GetField(string key)
        {
            return this.data.RequiredString(key);
        }
        
        public async Task<bool> SetLocale(Locale locale)
        {
            string url = Path.Combine(baseURL, String.Format("{0}.json", locale.ToString()));
            var www = UnityWebRequest.Get(url);
            await www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.result.ToString());
                return false;
            }
            
            string text = www.downloadHandler.text;
            var newData = XTableExchange.json.StringToTable(text);
            this.data = newData;

            return true;
        }
    }
}
