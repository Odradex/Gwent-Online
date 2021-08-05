using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;
using System.IO;
using System;

public class DeckBuilder : MonoBehaviour
{
    public Player.Faction faction;

    List<GameObject> DeckList;
    List<GameObject> CollectionList;

    public List<CardObject> neutralCards;
    public List<CardObject> specialCards;
    public List<CardObject> factionCards;

    public TMP_Dropdown factionDropdown;
    public Image LeaderImage;
    public List<Sprite> LeaderImages;
    public GameObject collection;
    public GameObject deck;
    public GameObject cardPrefab;
    public TMP_Text cardsAmount;
    public TMP_Text unitsAmount;

    // Start is called before the first frame update
    void Start()
    {
        faction = Player.Faction.NorthernRealms;
        DeckList = new List<GameObject>();
        CollectionList = new List<GameObject>();
        neutralCards = new List<CardObject>();
        specialCards = new List<CardObject>();
        factionCards = new List<CardObject>();

        neutralCards.AddRange(Resources.LoadAll<CardObject>("Card Objects/Units/Neutral"));
        factionCards.AddRange(Resources.LoadAll<CardObject>("Card Objects/Units/Northern Realms"));
        specialCards.AddRange(Resources.LoadAll<CardObject>("Card Objects/Special"));

        LoadCollection();
        LoadDeck("NorthernRealms");
    }

    private void LoadCollection()
    {
        foreach (GameObject item in CollectionList)
        {
            GameObject.Destroy(item);
        }
        CollectionList = new List<GameObject>();
        foreach (CardObject card in neutralCards) CreateCardGameObject(card);
        foreach (CardObject card in specialCards) CreateCardGameObject(card);
        foreach (CardObject card in factionCards) CreateCardGameObject(card);
    }

    private GameObject CreateCardGameObject(CardObject card)
    {
        GameObject cardObject = GameObject.Instantiate(cardPrefab);
        cardObject.GetComponent<Image>().sprite = card.FullImage;
        cardObject.tag = "DeckCard";
        cardObject.name = card.name;
        if (card.type == CardObject.Type.Unit)
            cardObject.name += " unit";
        cardObject.transform.SetParent(collection.transform);
        CollectionList.Add(cardObject);
        return cardObject;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hitData = Physics2D.Raycast(new Vector2(worldPos.x, worldPos.y), Vector2.zero, 0);
        // Debug.Log(hitData.transform.name);
        if (hitData.transform != null && Input.GetMouseButtonDown(0) && hitData.transform.tag == "DeckCard")
        {
            if(hitData.transform.parent == deck.transform)
            {
                hitData.transform.SetParent(collection.transform);
                DeckList.Remove(hitData.transform.gameObject);
                CollectionList.Add(hitData.transform.gameObject);
            }
            else
            {
                MoveCardToDeck(hitData.transform.gameObject);
            }
            int cardCount = DeckList.Count;
            int unitCount = DeckList.Where(u => u.name.Contains("unit")).Count();
            cardsAmount.text = $"Карт: {cardCount}/22";
            cardsAmount.color = cardCount < 22 ? new Color(202, 0, 0, 1) : new Color(202, 202, 202, 1);
            unitsAmount.text = $"Отрядов: {unitCount}/10";
            unitsAmount.color = unitCount < 10 ? new Color(202, 0, 0, 1) : new Color(202, 202, 202, 1);
        }
    }

    private void MoveCardToDeck(GameObject cardObject)
    {
        cardObject.transform.SetParent(deck.transform);
        DeckList.Add(cardObject.transform.gameObject);
        CollectionList.Remove(cardObject.transform.gameObject);
    }

    public void SaveDeck()
    {
        Debug.Log(Application.persistentDataPath);
        StreamWriter writer = new StreamWriter(@$"{Application.persistentDataPath}\{faction.ToString()}_deck.txt");
        foreach (GameObject item in DeckList)
            writer.Write(item.name + "\n");

        writer.Close();
    }

    public void UpdateFaction(int fac)
    { 
        SaveDeck();
        faction = (Player.Faction)fac;
        factionCards = new List<CardObject>();
        switch (faction)
        {
            case Player.Faction.Monsters:
                foreach (CardObject card in Resources.LoadAll<CardObject>("Card Objects/Units/Monsters"))
                    factionCards.Add(card);
                LeaderImage.sprite = LeaderImages[0];
                break;
            case Player.Faction.Nilfgaard:
                foreach (CardObject card in Resources.LoadAll<CardObject>("Card Objects/Units/Nilfgaard"))
                    factionCards.Add(card);
                LeaderImage.sprite = LeaderImages[1];
                break;
            case Player.Faction.NorthernRealms:
                foreach (CardObject card in Resources.LoadAll<CardObject>("Card Objects/Units/Northern Realms"))
                    factionCards.Add(card);
                LeaderImage.sprite = LeaderImages[2];
                break;
            case Player.Faction.Scoiatael:
                foreach (CardObject card in Resources.LoadAll<CardObject>("Card Objects/Units/Scoia'Tael"))
                    factionCards.Add(card);
                LeaderImage.sprite = LeaderImages[3];
                break;
            default: break;
        }
        LoadCollection();
        LoadDeck(faction.ToString());
    }

    private void LoadDeck(string fac)
    {
        DeckList.ForEach(u => GameObject.Destroy(u));
        DeckList = new List<GameObject>();
        List<CardObject> allcards = new List<CardObject>();
        allcards.AddRange(factionCards);
        allcards.AddRange(specialCards);
        allcards.AddRange(neutralCards);
        StreamReader reader;
        try
        {        
            reader = new StreamReader(@$"{Application.persistentDataPath}\{fac}_deck.txt");
        }
        catch (System.Exception)
        {
            return;
        }
        foreach (string item in reader.ReadToEnd().Split(Environment.NewLine.ToCharArray()))
        {
            if (item == "") continue;
            GameObject obj = CollectionList.Where(u => u.name == item).First();
            // GameObject obj = CreateCardGameObject(allcards.Where(u => u.name == item.Split(' ').First()).First());
            MoveCardToDeck(obj);
        }
    }

    public void ExitToMenu()
    {
        SaveDeck();
        GameObject.Destroy(GameObject.FindGameObjectWithTag("Net"));
        SceneManager.LoadScene("MainMenu");
    }
}
