using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RowController : MonoBehaviour
{
    [SerializeField] TMPro.TMP_Text rowPowerText;
    public List<Card> RowCards;
    public GameObject HornSlot;
    public bool IsBoosted;
    public int MoraleBoost;
    public int HornBoost = 1;
    public int RowPower;
    public CardObject.Row RowType;
    public bool IsEnemy;
    public bool IsBadWeather;

    
    public void Start()
    {
        HornBoost = 1;
        MoraleBoost = 0;
        rowPowerText.text = "0";
        RowCards = new List<Card>();
        RowPower = 0;
        IsBadWeather = false;
    }
    void Update()
    {
        RowPower = 0;
        foreach (Card card in RowCards)
            RowPower += card.Power;
        rowPowerText.text = RowPower.ToString();
    }

    internal void AddCard(Card card)
    {
        card.Power += MoraleBoost;
        card.Power *= HornBoost;
        RowCards.Add(card);
    }
    public void ChangeWeather(bool Bad = true)
    {
        IsBadWeather = Bad;
        foreach (Card card in RowCards)
        {
            card.Power = Bad ? 1 : card.CardInfo.basePower;
            if (card.BondLevel > 0)
                card.Power *= (2 * card.BondLevel);
        }

        MoraleBoost = 0;
        foreach (Card card in RowCards.Where(o => o.CardInfo.ability == CardObject.Ability.MoraleBoost))
            BoostMorale(card);

        HornBoost = 1;
        foreach (Card card in RowCards.Where(o => o.CardInfo.ability == CardObject.Ability.Horn))
        {
            HornBoost += 1;
            RowCards.ForEach(u => u.Power *= 2);
            card.Power /= 2;
        }
    }
    internal void BoostMorale(Card boostCard)
    {
        foreach (Card card in RowCards)
            if (card != boostCard)
            {
                card.Power++;
                Debug.Log("MORALE");
            }
        MoraleBoost++;
    }
    internal void BoostHorn()
    {
        HornBoost += 1;
        RowCards.ForEach(u => u.Power *= 2);
    }
}