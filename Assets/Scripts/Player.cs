using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using System;
using System.Linq;
using TMPro;

public class Player : MonoBehaviour
{
    public enum Faction
    {
        Monsters,
        Nilfgaard,
        NorthernRealms,
        Scoiatael
    }
    public TMP_Text PlayerName;

    
    internal Card selectedCard;
    [SerializeField] CardController cardController;
    [SerializeField] public GameObject Hand;
    public Button passButton;
    public List<CardObject> DeckCards;
    public List<CardObject> PlayedCards;
    public List<CardObject> HandCards;
    Faction faction;

    public Image FullCardImage;
    public bool ShowFullImage;

    public bool hisTurn;
    public bool enemyPassed;
    public bool passed;
    [SerializeField] internal CardOverlay overlay;
    public NetworkManager network;
    
    public Card SelectedCard { get => selectedCard; set => selectedCard = value; }
    public bool HisTurn { get => hisTurn; set => hisTurn = enemyPassed? true : value; }

    void Start()
    {
        StreamReader reader;
        reader = new StreamReader(@$"{Application.persistentDataPath}\playername.txt");
        PlayerName.text = reader.ReadLine();
        reader.Close();

        HandCards = new List<CardObject>();
        network = GameObject.FindWithTag("Net").GetComponent<NetworkManager>();
        faction = network.faction;
        DeckCards = GetDeck();
        network.ReceivedMessage += GetMessage;
        HisTurn = false;
        passed = false;
        enemyPassed = false;

        foreach (CardObject card in DeckCards.GetRange(6, 7))
        {
            DrawCard(card);
        }
        foreach (CardObject card in DeckCards)
        { 
            network.Send("deckcard:" + card.name);
        }
        // foreach (CardObject card in HandCards)
        // {
        //     network.Send("handcard:" + card.name);
        // }
        if (network.isServer)
        {
            if (UnityEngine.Random.Range(0, 2) == 0)
                network.Send("firstmove:firstmove");
            else
                HisTurn = true;
        }
        network.Send("name:" + PlayerName.text);

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }

    public void DrawCard(CardObject cardObject = null)
    {
        if (cardObject == null)
            cardObject = DeckCards[UnityEngine.Random.Range(0, DeckCards.Count)];
        if (!DeckCards.Contains(cardObject)) return;

        Card card = CardController.CardObjectToGameObject(cardObject).Item2;
        card.CurentState = Card.State.InHand;
        HandCards.Add(card.CardInfo);
        DeckCards.Remove(cardObject);
        network.Send("handcard:" + card.CardInfo.name);

        card.transform.SetParent(Hand.transform);
    }

    void GetMessage(string message)
    {
        string type = message.Split(':')[0];
        string value = message.Split(':')[1];
        switch (type)
        {
            case "move":
            case "firstmove":
            case "weather":
            case "horn":
                HisTurn = true;
                passButton.interactable = true;
                break;
            case "pass":
                enemyPassed = true;
                HisTurn = true;
                passButton.interactable = true;
                break;
        }
    }
    private List<CardObject> GetDeck()
    {
        List<CardObject> cards = new List<CardObject>();
        List<CardObject> deck = new List<CardObject>();

        cards.AddRange(Resources.LoadAll<CardObject>("Card Objects/Special"));
        cards.AddRange(Resources.LoadAll<CardObject>("Card Objects/Units/Neutral"));

        StreamReader reader = null;
        switch (faction)
        {
            case Faction.NorthernRealms: 
                reader = new StreamReader(@$"{Application.persistentDataPath}\NorthernRealms_deck.txt");
                cards.AddRange(Resources.LoadAll<CardObject>("Card Objects/Units/Northern Realms"));
                break;
            case Faction.Nilfgaard: 
                reader = new StreamReader(@$"{Application.persistentDataPath}\Nilfgaard_deck.txt");
                cards.AddRange(Resources.LoadAll<CardObject>("Card Objects/Units/Nilfgaard"));
                break;
            case Faction.Monsters: 
                reader = new StreamReader(@$"{Application.persistentDataPath}\Monsters_deck.txt");
                cards.AddRange(Resources.LoadAll<CardObject>("Card Objects/Units/Monsters"));
                break;
            case Faction.Scoiatael: 
                reader = new StreamReader(@$"{Application.persistentDataPath}\Scoiatael_deck.txt");
                cards.AddRange(Resources.LoadAll<CardObject>("Card Objects/Units/Scoia'Tael"));
                break;
            default: throw new System.Exception();
        }
        foreach (string item in reader.ReadToEnd().Split(Environment.NewLine.ToCharArray()))
        {
            if (item == "") continue;
            deck.Add(cards.Where(u => u.name == item.Split(' ')[0]).First());
        }
        reader.Close();
        System.Random rng = new System.Random();  

        int n = cards.Count;  
        while (n > 1)
        {  
            n--;  
            int k = rng.Next(n + 1);  
            CardObject value = cards[k];  
            cards[k] = cards[n];  
            cards[n] = value;  
        }
        return deck;
    }


    // Update is called once per frame
    void Update()
    {
        if (HandCards.Count == 0)
        {
            network.Send("pass:pass");
            passed = true;
            passButton.interactable = false;
        }

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hitData = Physics2D.Raycast(new Vector2(worldPos.x, worldPos.y), Vector2.zero, 0);

        FullCardImage.transform.position = new Vector3(worldPos.x + 97.75f, worldPos.y + 184.5f, -5);
        if (ShowFullImage && hitData.transform != null && hitData.transform.tag == "Card")
        {
            FullCardImage.transform.gameObject.SetActive(true);
            FullCardImage.sprite = hitData.transform.gameObject.GetComponent<Card>().CardInfo.FullImage;
        }
        else 
            FullCardImage.transform.gameObject.SetActive(false);
        
        if (Input.GetKeyDown(KeyCode.Z)) 
            ShowFullImage = ShowFullImage ? false : true;
        if (Input.GetMouseButtonDown(0) && hitData.transform != null && !passed)
        {
            switch (hitData.transform.tag)
            {
                case "Card":
                    SelectedCard = hitData.transform.gameObject.GetComponent<Card>();
                    break;
                case "Row":
                    if (HisTurn && SelectedCard != null && SelectedCard.CurentState == Card.State.InHand && RowCheck(selectedCard.CurentRow, hitData.transform.parent.gameObject.name))
                    {
                        if (!enemyPassed)
                            passButton.interactable = false;
                        if ((SelectedCard.CardInfo.ability != CardObject.Ability.Spy && hitData.transform.parent.parent.name == "EnemyField") || (selectedCard.CardInfo.ability == CardObject.Ability.Spy && hitData.transform.parent.parent.name != "EnemyField"))
                            break;
                        cardController.PlayCard(selectedCard, hitData.transform.gameObject);
                        HandCards.Remove(selectedCard.CardInfo);
                        network.Send("move:" + selectedCard.CardInfo.name + "/" + hitData.transform.parent.name);
                        HisTurn = false;
                    }
                    break;
                case "WeatherRow":
                    if (HisTurn && SelectedCard != null && SelectedCard.CurentState == Card.State.InHand && selectedCard.CardInfo.name.Contains("spec_p"))
                    {
                        cardController.PlayCard(selectedCard, hitData.transform.gameObject);
                        network.Send("weather:" + selectedCard.CardInfo.name);
                        HisTurn = false;
                    }
                    break;
                case "HornSlot":
                    if (HisTurn && SelectedCard != null && SelectedCard.CurentState == Card.State.InHand && selectedCard.CardInfo.name == "spec_2")
                    {
                        cardController.PlayCard(selectedCard, hitData.transform.gameObject);
                        network.Send("horn:" + hitData.transform.parent.name);
                        HisTurn = false;
                    }
                    break;
            }
        }
    }

    private bool RowCheck(CardObject.Row cardRow, string rowName)
    {
        switch (cardRow)
        {
            case CardObject.Row.Agile: return rowName == "Close Row" || rowName == "Ranged Row";
            case CardObject.Row.Close: return rowName == "Close Row";
            case CardObject.Row.Ranged: return rowName == "Ranged Row";
            case CardObject.Row.Siege: return rowName == "Siege Row";
        }
        return false;
    }
}
