// ClaimRewardAd.cs
using UnityEngine;
using UnityEngine.UI;
using GoogleMobileAds.Api;  // <- required for the AdMob API
using TMPro;

public class ClaimRewardAd : MonoBehaviour
{
    [SerializeField] private Button claimButton; // assign in inspector
    [SerializeField] private TextMeshProUGUI counterText;   // assign in inspector

    private RewardedAd rewardedAd = null; // current loaded rewarded ad object
    private int claimCount = 0;
    private int maxClaims = 35;

    // Use test ad unit IDs (replace with your production IDs later)
#if UNITY_ANDROID
    private string adUnitId = "ca-app-pub-3940256099942544/5224354917";
#elif UNITY_IOS
    private string adUnitId = "ca-app-pub-3940256099942544/1712485313";
#else
    private string adUnitId = "unexpected_platform";
#endif

    void Start()
    {
        // Initialize the Mobile Ads SDK once at app start
        MobileAds.Initialize(initStatus => { });

        // Wire button and UI
        if (claimButton != null)
            claimButton.onClick.AddListener(OnClaimButtonClicked);

        UpdateCounterUI();

        // Start loading the first rewarded ad using the new API
        LoadRewardedAd();
    }

    /// <summary>
    /// Load a rewarded ad using the static RewardedAd.Load(...) method.
    /// Newer plugin versions return the loaded RewardedAd in the completion callback.
    /// (Constructor-based approach was removed — that's the cause of your error.)
    /// </summary>
    private void LoadRewardedAd()
    {
        // Build request
        var adRequest = new AdRequest();


        // Call the static Load method. The callback gives you either a RewardedAd or an error.
        RewardedAd.Load(adUnitId, adRequest, (RewardedAd ad, LoadAdError loadError) =>
        {
            if (loadError != null || ad == null)
            {
                Debug.LogWarning("Rewarded ad failed to load: " + (loadError?.GetMessage() ?? "unknown"));
                rewardedAd = null;
                return;
            }

            // Save the loaded ad instance
            rewardedAd = ad;
            Debug.Log("Rewarded ad loaded successfully.");

            // Hook optional full-screen events (close, fail, impression, click)
            rewardedAd.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Rewarded ad closed by user.");
                // preload next ad after close
                LoadRewardedAd();
            };

            rewardedAd.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogWarning("Rewarded ad failed to open: " + error.GetMessage());
                // attempt reload (be careful to avoid tight retry loops)
                LoadRewardedAd();
            };

            rewardedAd.OnAdImpressionRecorded += () => Debug.Log("Ad impression recorded.");
            rewardedAd.OnAdClicked += () => Debug.Log("Ad clicked.");
            rewardedAd.OnAdPaid += (AdValue value) => Debug.Log("Ad paid event.");
        });
    }

    /// <summary>
    /// Called when the Claim button is pressed.
    /// Checks that the ad can be shown and then shows it with a reward callback.
    /// </summary>
    private void OnClaimButtonClicked()
    {
        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            // Show accepts a reward callback that runs when the user earns the reward.
            rewardedAd.Show((Reward reward) =>
            {
                // This callback runs when the user should be rewarded (watched the ad).
                if (claimCount < maxClaims)
                {
                    claimCount++;
                    UpdateCounterUI();
                }
            });

            // Note: rewardedAd is single-use — we reload on Close event above.
        }
        else
        {
            Debug.Log("Rewarded ad not ready yet.");
            // Optionally: give user feedback in UI that ad is loading.
        }
    }

    private void UpdateCounterUI()
    {
        if (counterText != null)
            counterText.text = $"Claim ({claimCount}/{maxClaims})";
    }

    private void OnDestroy()
    {
        // Clean up the rewardedAd object if needed to avoid leaks
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }
    }
}
