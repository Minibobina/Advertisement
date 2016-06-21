using UnityEngine;
using System.Collections;

[System.Serializable]
public class ServerSettings
{
	public int interval = 5;
	public int rewardAmount = 100;
	public bool overrideAmount = true;
	public string currency = "Currency";
	public string serverURL = "http://MinibobinaStudios.com/AdWaterfall/LiveAdAllocation/server/theJSON.json";

	public void Advertisementsuccessful (VideoZoneType type)
	{
		if(type == VideoZoneType.Interstitial)
		{
			PlayerPrefs.SetInt("InterstitialCount", PlayerPrefs.GetInt("InterstitialCount") + 1);
			PlayerPrefs.SetInt("TotalAdCount", PlayerPrefs.GetInt("TotalAdCount") + 1);
		}
		else if(type == VideoZoneType.VideoReward)
		{
			PlayerPrefs.SetInt("VideoRewardCount", PlayerPrefs.GetInt("VideoRewardCount") + 1);
			PlayerPrefs.SetInt("TotalAdCount", PlayerPrefs.GetInt("TotalAdCount") + 1);
		}
		else
		{
			Debug.Log("Video type was None of null, please correct this as advertisement count will not reflect the correct amount.");
		}
	}
}
