using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
public class CardOverlay : MonoBehaviour
{
    [SerializeField] List<Image> Images = new List<Image>(5);
    [SerializeField] Player player;
    [SerializeField] CardController controller;

    private List<CardObject> collection;
    private int offset;

    internal void ReviveOverlay()
    {
        offset = 0;
        this.gameObject.SetActive(true);
        collection = player.PlayedCards;

        for (int i = 0; i < (player.PlayedCards.Count >= 5 ? 5 : player.PlayedCards.Count); i++)
        {
            Images[i].gameObject.SetActive(true);
            Images[i].sprite = player.PlayedCards[i].FullImage;
        }
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hitData = Physics2D.Raycast(new Vector2(worldPos.x, worldPos.y), Vector2.zero, 0);
            if (hitData.transform.tag == "Overlay Card")
            {
                this.gameObject.SetActive(false);  
                controller.PlaySound(3);
                Card card = CardController.CardObjectToGameObject(player.PlayedCards.Where(u => u.FullImage == hitData.transform.GetComponent<Image>().sprite).First()).Item2;
                GameObject row = controller.PlayCard(card);
                player.network.Send("move:" + card.CardInfo.name + "/" + row.transform.parent.name + "/r");
            }
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow)) MoveCards(false);
        if (Input.GetKeyDown(KeyCode.RightArrow)) MoveCards(true);
    }
    private void MoveCards(bool moveRight)
    {
        offset = moveRight ? offset + 1 : offset - 1;
        if (offset < -2)
        {
            offset = -2;
            return;
        }
        if (offset > player.PlayedCards.Count - 3)
        {
            offset = player.PlayedCards.Count - 3;
            return;
        }
        for (int i = 0; i < 5; i++)
        {
            if (i + offset < 0 || i + offset >= player.PlayedCards.Count)
            {
                Images[i].gameObject.SetActive(false);   
                continue;
            }
            Images[i].gameObject.SetActive(true);
            Images[i].sprite = player.PlayedCards[i + offset].FullImage;
        }
    }
}
