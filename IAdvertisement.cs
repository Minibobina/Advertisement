

interface IAdvertisement
{
	string Id 
	{
		get;
		set;
	}
		
	void Init ();
	bool isReady ();

	void playVideoAd ();
	void playVideoRewardAd ();
	void playInterstitial ();
}
