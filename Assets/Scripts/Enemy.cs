using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class Enemy : MonoBehaviour
{
    public List<CardObject> DeckCards;
    public List<CardObject> HandCards;
    public List<CardObject> AllCardObjects;
    Player.Faction faction;
    public NetworkManager network;
    public CardController controller;
    public bool passed;
    
    public TMP_Text EnemyName;

    // Start is called before the first frame update
    void Start()
    {
        passed = false;
        network = GameObject.FindWithTag("Net").GetComponent<NetworkManager>();
        network.ReceivedMessage += GetMessage;
        AllCardObjects = GetAllCardObjects();
    }

    void GetMessage(string message)
    {
        Debug.Log("message received: " + message);
        Card card;

        string type = message.Split(':')[0];
        string value = message.Split(':')[1];
        switch (type)
        {
            case "faction":
                faction = (Player.Faction)System.Convert.ToInt32(value);
                break;
            case "deckcard":
                Debug.Log(value);
                DeckCards.Add(AllCardObjects.Where(u => u.name == value).First());
                break;
            case "handcard":
                HandCards.Add(AllCardObjects.Where(u => u.name == value).First());
                break;
            case "move":
                if (value.Split('/').Count() == 3)
                    card = CardController.CardObjectToGameObject(AllCardObjects.Where(u => u.name == value.Split('/')[0]).First()).Item2;
                else 
                    card = CardController.CardObjectToGameObject(HandCards.Where(u => u.name == value.Split('/')[0]).First()).Item2;

                GameObject row = GameObject.FindGameObjectsWithTag("Row").Where(u => u.transform.parent.name == value.Split('/')[1] && u.transform.parent.parent.name == (card.CardInfo.ability != CardObject.Ability.Spy? "EnemyField" : "PlayerField")).First();
                controller.PlayCard(card, row, true);
                HandCards.Remove(card.CardInfo);
                break;
            case "weather":
                card = CardController.CardObjectToGameObject(HandCards.Where(u => u.name == value).First()).Item2;
                controller.PlayCard(card, null, true);
                break; 
            case "horn":
                card = CardController.CardObjectToGameObject(HandCards.Where(u => u.name == "spec_2").First()).Item2;
                GameObject slot = GameObject.FindGameObjectsWithTag("HornSlot").Where(o => o.transform.parent.name == value && o.transform.parent.parent.name == "EnemyField").First();
                controller.PlayCard(card, slot, true);
                break;
            case "pass": 
                passed = true;
                break;
            case "name":
                EnemyName.text = value;
                break;

        }
    }

    public List<CardObject> GetAllCardObjects()
    {   
        List<CardObject> cards = new List<CardObject>();
        cards.AddRange(Resources.LoadAll<CardObject>("Card Objects/Units/Monsters"));
        cards.AddRange(Resources.LoadAll<CardObject>("Card Objects/Units/Neutral"));
        cards.AddRange(Resources.LoadAll<CardObject>("Card Objects/Units/Nilfgaard"));
        cards.AddRange(Resources.LoadAll<CardObject>("Card Objects/Units/Northern Realms"));
        cards.AddRange(Resources.LoadAll<CardObject>("Card Objects/Units/Scoia'Tael"));
        cards.AddRange(Resources.LoadAll<CardObject>("Card Objects/Special"));
        return cards;
    }
}
