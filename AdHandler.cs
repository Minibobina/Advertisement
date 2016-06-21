using Prime31;
using SimpleJSON;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Advertisements;

#if UNITY_ANDROID
using chartboost = Prime31.ChartboostAndroid;
#else
using chartboost = Prime31.ChartboostBinding;
#endif

public class AdHandler : MonoBehaviour
{
	[SerializeField] bool testMode = false;
	[SerializeField] bool debugMode = false;
	[SerializeField] ServerSettings settings = new ServerSettings ();

	List<string> interstitialOrder = new List<string> () { "Chartboost", "Unity", "AdColony", "Mopub"};
	List<string> videoOrder = new List<string> ()  { "Chartboost", "Unity", "AdColony", "Mopub" };
	int interstitialIndex = 0, videoIndex = 0;

#if UNITY_ANDROID
	// Unity Ads Info
	[Space(10)]
	[SerializeField] string unityAdsId = "83205";
	
	// Chartboost Info
	[Space(10)]
	[SerializeField] string chartboostId = "561d4e9004b01650cc9a8260";
	[SerializeField] string chartboostSignature = "891da6eb4a3153854dfd4d92ecea09f3c3d6d5ca";
	
	// AdColony Info
	[Space(10)]
	[SerializeField] string adColonyId  = "app34c56bb654814ac3a2";
	[SerializeField] List<VideoZone> adColonyGameZones = new List<VideoZone>();

#else
	
	// Unity Ads Info
	[Space(10)]
	[SerializeField] string unityAdsId = "83207";
	
	// Chartboost Info
	[Space(10)]
	[SerializeField] string chartboostId = "561d4e8e04b01650cc9a825e";
	[SerializeField] string chartboostSignature = "c53c0d002e0abbb045641eb566b41520ec6b03a1";
	
	// AdColony Info
	[Space(10)]
	[SerializeField] string adColonyId = "app0f788e30b8164dd8a7";
	[SerializeField] List<VideoZone> adColonyGameZones = new List<VideoZone> ();

#endif
	
#region Singleton Pattern
	private static AdHandler _instance;
	public static AdHandler instance {
		get {
			if (_instance == null) {
				_instance = GameObject.FindObjectOfType<AdHandler> ();
				DontDestroyOnLoad (_instance.gameObject);
			}
			return _instance;
		}
	}
#endregion

#region Getters/Setters
	public List<string> GetInterstitialOrder
	{
		get{return interstitialOrder;}
	}
	public List<string> GetVideoRewardOrder
	{
		get{return videoOrder;}
	}
	public int GetAdInterstitialCount
	{
		get{return PlayerPrefs.GetInt("InterstitialCount");}
	}
	public int GetAdVideoRewardCount
	{
		get{return PlayerPrefs.GetInt("VideoRewardCount");}
	}
	public int GetTotalAdCount
	{
		get{return PlayerPrefs.GetInt("TotalAdCount");}
	}
	public void RefreshServer ()
	{
		StartCoroutine (CheckServer ());
	}
#endregion

	void Awake ()
	{
		// Keep this Prefab from being deleted in each scene
		if (_instance == null) {
			_instance = this;
			DontDestroyOnLoad (this);
		} else {
			if (this != _instance)
				Destroy (this.gameObject);
		}
		
		// Configure AdColony
		AdColony.Configure ("version:1.0,store:apple", 
		                   adColonyId,
		                   GetVideoZoneIdsAsStringArray ());
		
		// Configure Unity Ads
		//if(Advertisement.isSupported)
		//	Advertisement.Initialize (unityAdsId, testMode);
		//else
		//	if(debugMode)Debug.Log ("Unity Ads is not supported on this device.");
		
		//Advertisement.debugLevel = Advertisement.DebugLevel.None;
		
		// Configure Chartboost
		ChartboostBinding.init (chartboostId, chartboostSignature, true);
		ChartboostBinding.cacheRewardedVideo ("defaultZone");
		ChartboostBinding.cacheInterstitial ("defaultZone");
	

		ChartboostManager.didFinishInterstitialEvent += OnInterstitialEventFinish;
		ChartboostManager.didCompleteRewardedVideoEvent += didFinishRewardedVideoEvent;

		// Check to see if any default strings match example
		CheckDefaultSettings();
		// Check Server, If connected, Reconfigure list of played ads 
		StartCoroutine (CheckServer ());
	}
	void CheckDefaultSettings ()
	{
		if(adColonyId == ADCOLONY_ID)
			Debug.Log("Adcolony ID hasnt be changed and is still using the default settings.");
		if(unityAdsId == UNITY_ADS_ID)
			Debug.Log("Unity ID hasnt be changed and is still using the default settings.");
		if(chartboostId == CHARTBOOST_ID)
			Debug.Log("Chartboost ID hasnt be changed and is still using the default settings.");
		if(chartboostSignature == CHARTBOOST_SIGNATURE)
			Debug.Log("Chartboost Signature hasnt be changed and is still using the default settings.");
	}
	IEnumerator CheckServer ()
	{
		//new adordering system
		WWW www = new WWW (settings.serverURL);
		yield return www;
		
		string orderSTR = www.text;
		www.Dispose ();
		
		if (string.IsNullOrEmpty (orderSTR)) {
			yield break;	
		}
		
		var orderObj = JSON.Parse (orderSTR);
		
		
		int platformInt = -1;
#if UNITY_IOS
		platformInt = 0;
#elif UNITY_ANDROID
		platformInt = 1;
#endif
		interstitialOrder = new List<string> (parseString (orderObj [platformInt] ["Int_Order"].ToString ()));
		videoOrder = new List<string> (parseString (orderObj [platformInt] ["Vid_Order"].ToString ()));
		
		settings.interval = orderObj [platformInt] ["Count"].AsInt; // Count till ad display

		chartboost.cacheRewardedVideo ("defaultZone");
		chartboost.cacheInterstitial ("defaultZone");

		// Display the order of the list of ad companys
		for(int i = 0; i < interstitialOrder.Count; i++)
			if(debugMode)Debug.Log("Company: " + interstitialOrder[i] + " Interstitial Ad");
		for(int j = 0; j < videoOrder.Count; j++)
			if(debugMode)Debug.Log("Company: " + videoOrder[j] + " Video Reward Ad");
	}
	
	public void ShowVideoRewardAd ()
	{
		VideoRewardWaterfall ();
		//if(debugMode) Debug.Log ("Show Video Reward Ad Methond Called");
	}
	public void ShowInterstitial ()
	{
		InterstitialWaterfall ();
		//if(debugMode) Debug.Log ("Show Interstitial Ad Methond Called");
	}
	
	void InterstitialWaterfall ()
	{
		//Debug.Log (interstitialIndex);
		//Debug.Log (interstitialOrder [interstitialIndex].ToLower());
		switch (interstitialOrder [interstitialIndex].ToLower())
		{
			case "chartboost":
			if (chartboost.hasCachedInterstitial ("defaultZone")) 
			{
				chartboost.showInterstitial ("defaultZone");
				interstitialIndex = 0;
			} 
			else 
			{
				chartboost.cacheInterstitial ("defaultZone");
				HandleInterstitialIncrement ();
			}
			break;
			
//		case "unity":
//			if (Advertisement.IsReady ()) {
//				Advertisement.Show ();
//				settings.Advertisementsuccessful(VideoZoneType.Interstitial);
//				interstitialIndex = 0;
//			} else {
//				HandleInterstitialIncrement ();
//			}
//			break;
			
		case "adcolony":
			if (AdColony.IsVideoAvailable (GetAdvertisementZoneId(VideoZoneType.Interstitial))) {
				AdColony.ShowVideoAd (GetAdvertisementZoneId(VideoZoneType.Interstitial));
				interstitialIndex = 0;
			} else {
				HandleInterstitialIncrement ();
			}
			break;
			
		case "mopub":
			HandleInterstitialIncrement ();
			if(debugMode) Debug.Log ("MoPub isnt configured, if needed please set this up.");
			break;
			
		default:
			if(debugMode) Debug.Log ("Advertisement Company [" + interstitialOrder [interstitialIndex].ToLower () + "] do not match any company provided, please check switch statement.");
			break;
		}
	}
	void VideoRewardWaterfall ()
	{
		if(debugMode) Debug.Log (videoOrder [videoIndex].ToLower());
		switch (videoOrder [videoIndex].ToLower ()) {
		case "chartboost":
			if (chartboost.hasCachedRewardedVideo ("defaultZone")) {
				chartboost.showRewardedVideo ("defaultZone");
				videoIndex = 0;
			} else {
				chartboost.cacheRewardedVideo ("defaultZone");
				HandleVideoIncrement ();
			}
			break;
			
//		case "unity":
//			if (Advertisement.IsReady ("rewardedVideoZone")) {
//				Advertisement.Show ("rewardedVideoZone", new ShowOptions {
//					resultCallback = result => 
//					{
//						if (result == ShowResult.Finished) 
//						{
//							PlayerPrefs.SetInt(settings.currency, PlayerPrefs.GetInt(settings.currency) + settings.rewardAmount);
//							settings.Advertisementsuccessful(VideoZoneType.VideoReward);
//							#if UNITY_IOS
//							EtceteraBinding.showAlertWithTitleMessageAndButtons("Success!", "You earned " + settings.rewardAmount + " Coin!", new string[] {"OK"});
//							#elif UNITY_ANDROID
//							EtceteraAndroid.showAlert("Success!", "You earned " + settings.rewardAmount + " Coin!", "OK");
//							#endif
//							videoIndex = 0;
//						}
//					}
//				});
//			} else {
//				HandleVideoIncrement ();
//			}
//			
//			break;
		case "adcolony":
			if (AdColony.IsV4VCAvailable (GetAdvertisementZoneId(VideoZoneType.VideoReward))) {
				AdColony.ShowV4VC(true,GetAdvertisementZoneId(VideoZoneType.VideoReward));
			} else {
				HandleVideoIncrement ();
			}
			break;
			
		case "mopub":
			HandleVideoIncrement ();
			if(debugMode) Debug.Log ("MoPub isnt configured, if needed please set this up.");
			break;
		default:
			if(debugMode) Debug.Log ("Advertisement Company [" + videoOrder [videoIndex].ToLower () + "] do not match any company provided, please check switch statement.");
			break;
		}
	}
	
	void HandleInterstitialIncrement ()
	{
		if (interstitialIndex < interstitialOrder.Count) 
		{
			interstitialIndex++;
			ShowInterstitial ();
		} 
		else 
		{
			interstitialIndex = 0;
			if(debugMode) Debug.Log ("We have checked every company");
		}
	}
	void HandleVideoIncrement ()
	{
		if (videoIndex < videoOrder.Count) 
		{
			videoIndex++;
			ShowInterstitial ();
		} 
		else 
		{
			videoIndex = 0;
			if(debugMode) Debug.Log ("We have checked every company");
		}
	}

	void Pause() 
	{
		this.GetComponent<AudioSource>().Pause();
	}
	void Resume() 
	{
		this.GetComponent<AudioSource>().Play();
	}
	
	#region AdColony Delegate Methods
	private void OnVideoStarted()
	{
		Pause();
		if(debugMode) Debug.Log("On Video Start");
	}
	private void OnVideoFinished(bool ad_was_shown)
	{
		Resume();
		settings.Advertisementsuccessful(VideoZoneType.Interstitial);
		if(debugMode) Debug.Log("On Video Finished");
	}
	private void OnV4VCResult(bool success, string name, int amount)
	{
		if(success)
		{
			if(debugMode) Debug.Log("V4VC SUCCESS: name = " + name + ", amount = " + amount);

			if(settings.rewardAmount > amount && settings.overrideAmount)
			{
				PlayerPrefs.SetInt(settings.currency, PlayerPrefs.GetInt(settings.currency) + settings.rewardAmount);
				settings.Advertisementsuccessful(VideoZoneType.VideoReward);
			}
			else
			{
				PlayerPrefs.SetInt(settings.currency, PlayerPrefs.GetInt(settings.currency) + amount);
			}
		}
		else
		{
			if(debugMode) Debug.LogWarning("V4VC FAILED!");
		}
	}
#endregion

#region Chartboost Delegate Methods
	private void OnInterstitialEventFinish(string location, string reason)
	{
		if(debugMode) Debug.Log("On Interstitial Finished");
		settings.Advertisementsuccessful(VideoZoneType.Interstitial);
	}
	private void didFinishRewardedVideoEvent(int amount)
	{
		if(debugMode) Debug.Log("On Video Finished");
		settings.Advertisementsuccessful(VideoZoneType.VideoReward);
	}
#endregion
	
	string[] parseString (string roughString)
	{
		string cleanString = roughString;
		cleanString = cleanString.Replace ("\"", "");
		cleanString = cleanString.Replace (",-", "");
		cleanString = cleanString.Replace ("-,", "");
		
		string[] theOrder = cleanString.Split (',');
		
		return theOrder;
	}
	string[] GetVideoZoneIdsAsStringArray ()
	{
		string[] allZones = new string[adColonyGameZones.Count];
		for (int i = 0; i < adColonyGameZones.Count; i++)
			allZones [i] = adColonyGameZones [i].zoneId;
		return allZones;
	}
	string GetAdvertisementZoneId (VideoZoneType AdvertisementType)
	{
		if (AdvertisementType == VideoZoneType.Interstitial) {
			return GetAdvertisementZoneIdFromList(VideoZoneType.Interstitial);
		} else if (AdvertisementType == VideoZoneType.VideoReward) {
			return GetAdvertisementZoneIdFromList(VideoZoneType.VideoReward);
		} else {
			if(debugMode) Debug.Log("Video zone type is None, Please correct this responce to achieve the correct Advertisement");
			return null;
		}
	}
	string GetAdvertisementZoneIdFromList(VideoZoneType AdvertisementType)
	{
		for (int i = 0; i < adColonyGameZones.Count; i++)
		{
			if(adColonyGameZones[i].zoneType == AdvertisementType)
				return adColonyGameZones[i].zoneId;
		}
		return null;
	}

	// For Checking if all default settings have been changed
	#if UNITY_ANDROID
	private const string ADCOLONY_ID = "app34c56bb654814ac3a2";
	private const string UNITY_ADS_ID = "83205";
	private const string CHARTBOOST_ID = "561d4e9004b01650cc9a8260";
	private const string CHARTBOOST_SIGNATURE = "891da6eb4a3153854dfd4d92ecea09f3c3d6d5ca";
	#else
	private const string ADCOLONY_ID = "app0f788e30b8164dd8a7";
	private const string UNITY_ADS_ID = "83207";
	private const string CHARTBOOST_ID = "561d4e8e04b01650cc9a825e";
	private const string CHARTBOOST_SIGNATURE = "c53c0d002e0abbb045641eb566b41520ec6b03a1";
	#endif
}
