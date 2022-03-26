using UnityEngine;

#if UNITY_ADS
using UnityEngine.Advertisements;
#endif

public class AdsKit
{
    public static bool adsEnabled = true;
    public static float adsInterval = 150; // s

    private static float msLastAdsTime = 0;

    public static bool HasAd
    {
        get
        {
#if UNITY_ADS
            if (!adsEnabled)
                return false;
            if (Time.realtimeSinceStartup - msLastAdsTime < adsInterval)
                return false;
            return Advertisement.IsReady();
#else
            return false;
#endif
        }
    }

    public static bool HasRewardedAd
    {
        get
        {
#if UNITY_ADS
            return Advertisement.IsReady("rewardedVideo");
#else
            return false;
#endif
        }
    }

    public static void ShowAd()
    {
#if UNITY_ADS
        if (!adsEnabled)
            return;
        if (Time.realtimeSinceStartup - msLastAdsTime < adsInterval)
            return;
        msLastAdsTime = Time.realtimeSinceStartup;

        if (Advertisement.IsReady())
        {
            Advertisement.Show();
        }
#endif
    }

    public static void ShowRewardedAd(System.Action<bool> callback)
    {
#if UNITY_ADS
        var options = new ShowOptions();
        options.resultCallback = (result) =>
        {
            if (callback != null)
            {
                callback(result == ShowResult.Finished);
            }
        };

        if (Advertisement.IsReady("rewardedVideo"))
        {
            Advertisement.Show("rewardedVideo", options);
        }
#endif
    }
}
