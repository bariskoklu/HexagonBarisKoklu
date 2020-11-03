using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;

public class AdsManager : MonoBehaviour
{
    string gameId = "3885967";
    bool testMode = true;

    public static AdsManager instance = null;

    private Button button;

    private string rewardedAdsPlacementId = "rewardedVideo";

    public IntType bombActionCount;
    public int adRewardBombCount;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        Advertisement.Initialize(gameId, testMode);
    }

    public void ShowRewardedVideo(Button buttonForAd)
    {
        Advertisement.Show(rewardedAdsPlacementId);
        button = buttonForAd;
    }
    public void OnUnityAdsReady(string placementId)
    {
        // If the ready Placement is rewarded, activate the button: 
        if (placementId == rewardedAdsPlacementId)
        {
            button.interactable = true;
        }
    }

    public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
    {
        // Define conditional logic for each ad completion status:
        if (showResult == ShowResult.Finished)
        {
            bombActionCount.value += adRewardBombCount;
        }
        else if (showResult == ShowResult.Skipped)
        {
            GameManager.instance.RestartGame();
        }
        else if (showResult == ShowResult.Failed)
        {
            GameManager.instance.RestartGame();
            Debug.LogWarning("The ad did not finish due to an error.");
        }
    }

    public void OnUnityAdsDidError(string message)
    {
        // Log the error.
    }

    public void OnUnityAdsDidStart(string placementId)
    {
        // Optional actions to take when the end-users triggers an ad.
    }
}
