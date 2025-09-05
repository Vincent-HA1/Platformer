using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HUDManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] LevelManager levelManager;

    [Header("UI References")]
    [SerializeField] TextMeshProUGUI coinAmountText;
    [SerializeField] Transform heartsParent;
    [SerializeField] Transform bigCoinsParent;
    [SerializeField] GameObject heartPrefab;
    [SerializeField] GameObject bigCoinIndicatorPrefab;


    List<Heart> hearts = new List<Heart>();
    List<BigCoinIndicator> bigCoins =  new List<BigCoinIndicator>();

    //Run from the level manager. Fills the UI
    public void InitialiseUI(List<int> bigCoinsFound, float playerMaxHealth = 3, float startingCoins = 0)
    {
        //Inintiailsie the hearts
        for (int i = 0; i < playerMaxHealth; i++)
        {
            GameObject heart = Instantiate(heartPrefab, heartsParent);
            hearts.Add(heart.GetComponent<Heart>());
        }
        //initialise the big coins.
        for (int i = 0; i < bigCoinsFound.Count; i++)
        {
            BigCoinIndicator bigCoin = Instantiate(bigCoinIndicatorPrefab, bigCoinsParent).GetComponent<BigCoinIndicator>();
            bigCoins.Add(bigCoin);
            //If this big coin was found, show it as filled on the HUD
            if (bigCoinsFound[i] == 1)
            {
                bigCoin.SetFound();
            }
        }
        UpdateCoinAmount(startingCoins);
    }


    //Called when the player's health changes
    public void UpdateHealthAmount(float newHealth)
    {
        //So check the hearts amount. Go through each heart and manually fill or unfill it
        for(int i = 0; i < hearts.Count;i++)
        {
            if(i < newHealth)
            {
                hearts[i].SetFilled();
            }
            else
            {
                hearts[i].SetEmpty();
            }
        }
    }

    public void UpdateBigCoinIndicator(int index)
    {
        bigCoins[index].SetFound();
    }

    public void UpdateCoinAmount(float coinAmount)
    {
        string coinAmountString = coinAmount.ToString();
        string finalString = "";
        for (int i = 0; i < coinAmountString.Length; i++)
        {
            finalString += $"<sprite index={coinAmountString[i]}>";
        }
        coinAmountText.text = finalString;
    }
}
