using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public enum State
    {
        InDeck,
        InHand,
        Played,
        Discarded
    }

    [SerializeField] private int power;
    [SerializeField] public CardObject CardInfo;
    [SerializeField] TMP_Text powerText;
    [SerializeField] Image cardImage;
    [SerializeField] public CardObject.Row CurentRow;
    [SerializeField] public State CurentState;
    internal RowController rowController;
    public int BondLevel;
    public bool runStart = true;
    public int Power 
    { 
        get => power; 
        set 
        {
            power = CardInfo.isHero? CardInfo.basePower : value; 
        }
    }

    public Card(CardObject obj)
    {
        CardInfo = obj;
    }
    // морская черепашка по имени Наташка
    public void Start()
    {
        if (runStart == false)
            return;

        runStart = false;
        BondLevel = 0;
        if (CardInfo.type == CardObject.Type.Special)
        {
            powerText.gameObject.SetActive(false);
        }
        //CurentState = State.InHand;
        Power = CardInfo.basePower;
        if (rowController != null && rowController.IsBadWeather)
            Power = 1;
        powerText.text = Power.ToString();
        cardImage.sprite = CardInfo.SmallImage;
        CurentRow = CardInfo.row;

        if (CardInfo.isHero)
            powerText.enabled = false;

        // if (rowController != null && rowController.HornBoost > 1)
        //     Power *= (2 * rowController.HornBoost - 1);
    }
    public void Update()
    {
        powerText.text = Power.ToString();
        powerText.faceColor = (Power > CardInfo.basePower)? new Color32(15, 159, 0, 255) : new Color32(0, 0, 0, 255);
        powerText.faceColor = (Power < CardInfo.basePower)? new Color32(157, 0, 0, 255) : new Color32(0, 0, 0, 255);
    }
}