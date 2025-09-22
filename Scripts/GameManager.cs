using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class GameManager : MonoBehaviour
{
    public Button claimButton;
    public TextMeshProUGUI balanceText;
    private double balance = 0f;
    private bool isMining = false;
    private double lastClaimTime;
    private const float dailyCooldown = 86400f; // 24 hours
    private double miningRate = 1e-15; // 0.000000000000001 BTC/hour

    void Start()
    {
        if (claimButton != null)
        {
            claimButton.onClick.AddListener(StartDailyMining);
            claimButton.interactable = true;
            Debug.Log("Claim button assigned");
        }
        else
        {
            Debug.LogError("Claim button not assigned");
        }

        if (balanceText != null)
        {
            balanceText.text = "0 BTC";
            Debug.Log("balanceText assigned, initial: " + balanceText.text);
        }
        else
        {
            Debug.LogError("balanceText not assigned");
        }

        LoadSavedData();
        Debug.Log("Loaded: balance=" + balance + ", rate=" + miningRate + ", lastClaim=" + lastClaimTime + ", Time.now=" + Time.time);

        if (lastClaimTime == 0f)
        {
            isMining = true;
            lastClaimTime = Time.time;
            Debug.Log("Initial claim triggered, isMining=true");
        }

        StartCoroutine(MiningLoop());
    }

    private void StartDailyMining()
    {
        if (Time.time - lastClaimTime >= dailyCooldown)
        {
            isMining = true;
            lastClaimTime = Time.time;
            claimButton.interactable = false;
            Debug.Log("Mining claimed, isMining=true");
        }
        else
        {
            double timeLeft = dailyCooldown - (Time.time - lastClaimTime);
            Debug.Log("Cooldown active, time left=" + timeLeft);
        }
    }


    private IEnumerator MiningLoop()
    {
        while (true)
        {
            Debug.Log("Loop running, isMining=" + isMining);
            if (isMining)
            {
                double delta = Time.deltaTime / 3600f;
                balance += miningRate * delta;
                if (balanceText != null)
                {
                    balanceText.text = balance.ToString("F15") + " BTC";

                    Debug.Log("Balance updated to: " + balanceText.text);
                }
                else
                {
                    Debug.LogError("balanceText not assigned");
                }

                SaveData();
            }
            yield return null;
        }
    }

    private void LoadSavedData()
    {
        balance = double.Parse(PlayerPrefs.GetString("Balance", "0"));
        miningRate = double.Parse(PlayerPrefs.GetString("MiningRate", "1e-15"));
        lastClaimTime = double.Parse(PlayerPrefs.GetString("LastClaim", "0"));
    }

    private void SaveData()
    {
        PlayerPrefs.SetString("Balance", balance.ToString());
        PlayerPrefs.SetString("MiningRate", miningRate.ToString());
        PlayerPrefs.SetString("LastClaim", lastClaimTime.ToString());
    }


    void Update()
    {
        if (isMining && Time.time - lastClaimTime >= dailyCooldown)
        {
            isMining = false;
            claimButton.interactable = true;
            Debug.Log("Cooldown complete, isMining=false");
        }
    }
}