using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardController : MonoBehaviour
{
    public List<RowController> Rows;

    public Player player;
    public List<Card> CardsInPlay;
    public GameObject weatherField;
    public AudioSource AudioPlayer;
    public AudioSource MusicPlayer;

    public AudioClip[] sounds;

    [SerializeField] CardOverlay overlay;

    internal void PlaySound(int soundId)
    {
        AudioPlayer.clip = sounds[soundId];
        AudioPlayer.Play();
    }

    public void Update()
    {
        if (AudioPlayer.isPlaying && AudioPlayer.clip != sounds[0])
            MusicPlayer.volume = 0.2f;
        else 
            MusicPlayer.volume = 1f;
    }

    internal GameObject PlayCard(Card selectedCard, GameObject row = null, bool asEnemy = false)
    {
        PlaySound(0);
        if (selectedCard.CardInfo.type == CardObject.Type.Special)
        {
            CardsInPlay.Add(selectedCard);
            selectedCard.CurentState = Card.State.Played;
            switch (selectedCard.CardInfo.name)
            {
                case "spec_p1":
                    selectedCard.transform.SetParent(weatherField.transform);
                    Rows.Where(o => o.RowType == CardObject.Row.Close).ToList().ForEach(o => o.ChangeWeather());
                    return null;
                case "spec_p2":
                    selectedCard.transform.SetParent(weatherField.transform);
                    Rows.Where(o => o.RowType == CardObject.Row.Ranged).ToList().ForEach(o => o.ChangeWeather());
                    return null;
                case "spec_p3":
                    selectedCard.transform.SetParent(weatherField.transform);
                    Rows.Where(o => o.RowType == CardObject.Row.Siege).ToList().ForEach(o => o.ChangeWeather());
                    return null;
                case "spec_p4":
                    RemoveCard(selectedCard);
                    CardsInPlay.Where(u => u.CardInfo.name.Contains("spec_p")).ToList().ForEach(o => RemoveCard(o));
                    Rows.ForEach(o => o.ChangeWeather(false));
                    return null;
                case "spec_2": // HORN
                    selectedCard.transform.SetParent(row.transform);
                    Rows.Where(o => o.HornSlot == row).First().BoostHorn();
                    if (!asEnemy) PlaySound(1);
                    return null;
                default:break;
            }
        }

        RowController rowController;
        if (row == null)
        {
            rowController = GetRowFromCard(selectedCard);
            row = rowController.transform.gameObject;
        }
        else
            rowController = row.GetComponent<RowController>();

        selectedCard.rowController = rowController;
        selectedCard.gameObject.transform.SetParent(row.transform);
        if (rowController.IsBadWeather)
            selectedCard.Power = 1;

        rowController.AddCard(selectedCard);
        selectedCard.CurentState = Card.State.Played;

        switch (selectedCard.CardInfo.ability)
        {
            case CardObject.Ability.MoraleBoost:
                rowController.BoostMorale(selectedCard);
                if (!asEnemy) PlaySound(1);
                break;
            case CardObject.Ability.TightBond:
                var bondCards = rowController.RowCards.Where(u => u.CardInfo.name == selectedCard.CardInfo.name);
                foreach (Card card in bondCards)
                {
                    card.BondLevel++;
                    if (card == selectedCard) continue;
                    card.Power *= 2;
                }
                for (int i = 1; i < bondCards.Count(); i++)
                    selectedCard.Power *= 2;
                break;
            case CardObject.Ability.Horn:
                rowController.BoostHorn();
                selectedCard.Power /= 2;
                if (!asEnemy) PlaySound(1);
                break;
            case CardObject.Ability.Scorch:
                int maxPower = CardsInPlay.OrderByDescending(u => u.Power).First().Power;
                Card[] currentCards = new Card[CardsInPlay.Count];
                CardsInPlay.CopyTo(0, currentCards, 0, CardsInPlay.Count);
                foreach (Card card in currentCards)
                    if (card.Power == maxPower)
                        RemoveCard(card);
                break;
            case CardObject.Ability.Muster:
                if (asEnemy) return row;
                if (rowController.RowCards.Count(u => u.CardInfo.name == selectedCard.CardInfo.name) > 1)
                    break;
                CardObject[] cardsInDeck = player.DeckCards.Where(u => u.name == selectedCard.CardInfo.name).ToArray();
                foreach (CardObject card in cardsInDeck)
                {
                    PlayCard(CardObjectToGameObject(card).Item2, rowController.gameObject);
                    player.DeckCards.Remove(card);
                }
                break;
            case CardObject.Ability.Spy:
                if (asEnemy) return row;
                for (int i = 0; i < 2; i++)
                {
                    player.DrawCard(player.DeckCards[0]);
                }
                if (!asEnemy) PlaySound(2);
                break;
            case CardObject.Ability.Medic:
                if (asEnemy) break;
                if (player.PlayedCards.Count == 0) break;
                overlay.ReviveOverlay();
                break;
            default: break;
        }
        CardsInPlay.Add(selectedCard);
        return row;
    }

    private RowController GetRowFromCard(Card card)
    {
        if (card.CardInfo.row == CardObject.Row.Agile)
            return Rows.Where(u => u.RowType == CardObject.Row.Close && !u.IsEnemy).First();

        return Rows.Where(u => u.RowType == card.CardInfo.row && u.IsEnemy == (card.CardInfo.ability == CardObject.Ability.Spy)).First();
    }

    internal void RemoveCard(Card card)
    {
        CardsInPlay.Remove(card);
        if (card.CardInfo.type == CardObject.Type.Unit)
        {
            card.rowController.RowCards.Remove(card);
            card.rowController = null;
        }
        if (card.CardInfo.isHero == false)
            player.PlayedCards.Add(card.CardInfo);
        GameObject.Destroy(card.transform.gameObject);
    }
    public static (GameObject, Card) CardObjectToGameObject(CardObject obj)
    {
        GameObject newCard = Instantiate(Resources.Load<GameObject>("Card"));
        Card temp = newCard.GetComponent<Card>();
        temp.CardInfo = obj;
        temp.Start();
        return (newCard, temp);
    }
}