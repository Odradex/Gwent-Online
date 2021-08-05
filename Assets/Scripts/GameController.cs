using TMPro;
using UnityEngine;
using System.Linq;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public TMP_Text PlayerName;


    public TMP_Text PlayerScoreText;
    int playerScore;
    public int RoundsWon;
    public GameObject playerGem;

    public TMP_Text EnemyScoreText;
    int enemyScore;
    public int EnemyRoundsWon;
    public GameObject enemyGem;

    public GameObject passPanel;
    NetworkManager network;

    public Player player;
    public Enemy enemy;
    public CardController controller;

    [Header("Results Settings")]
    public GameObject ResultPanel;
    public VideoPlayer videoPlayer;
    public TMP_Text resultText;

    public VideoClip winVideo;
    public VideoClip drawVideo;
    public VideoClip looseVideo;

    // Start is called before the first frame update
    void Start()
    {
        RoundsWon = 0;
        network = GameObject.FindWithTag("Net").GetComponent<NetworkManager>();
        PlayerScoreText.text = "0";
        EnemyScoreText.text = "0";
        
    }

    // Update is called once per frame
    void Update()
    {
        enemyScore = controller.Rows.Where(u => u.IsEnemy).Sum(o => o.RowPower);
        playerScore = controller.Rows.Where(u => u.IsEnemy == false).Sum(o => o.RowPower);

        EnemyScoreText.text = enemyScore.ToString();
        PlayerScoreText.text = playerScore.ToString();

        if (enemy.passed && player.passed) NextRound(false);

        if (Input.GetKeyDown(KeyCode.Escape))
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void Pass()
    {
        passPanel.SetActive(true);
        network.Send("pass:pass");
        player.passed = true;

        if (enemy.passed)
            NextRound(true);
    }

    public void NextRound(bool lastMoveByPlayer)
    {
        player.passed = false;
        player.enemyPassed = false;
        enemy.passed = false;
        passPanel.SetActive(false);

        player.HisTurn = lastMoveByPlayer;
        
        if (playerScore > enemyScore)
        {
            RoundsWon++;
            playerGem.SetActive(true);
        }
        else if (playerScore == enemyScore)
        {
            RoundsWon++;
            EnemyRoundsWon++;
            playerGem.SetActive(true);
            enemyGem.SetActive(true);
        }
        else 
        {
            EnemyRoundsWon++;
            enemyGem.SetActive(true);
        }

        if (RoundsWon == EnemyRoundsWon && RoundsWon == 2) EndGame(2);
        else if (RoundsWon == 2) EndGame(1);
        else if (EnemyRoundsWon == 2) EndGame(3);

        if (RoundsWon == 2 || EnemyRoundsWon == 2) return;
        PlayerScoreText.text = "0";
        EnemyScoreText.text = "0";
        enemyScore = 0;
        playerScore = 0;

        controller.CardsInPlay.Where(o => !o.rowController.IsEnemy && o.CardInfo.type == CardObject.Type.Unit).ToList().ForEach(o => controller.RemoveCard(o));
        controller.CardsInPlay.ForEach(o => GameObject.Destroy(o.transform.gameObject));
        controller.CardsInPlay = new System.Collections.Generic.List<Card>();
        
        controller.Rows.ForEach(o => o.Start());
        player.DrawCard();
        player.DrawCard();
    }

    private void EndGame(int result)
    {
        AudioSource music = GetComponent<AudioSource>();
        music.Stop();
        switch (result)
        {
            case 1: //WIN
                videoPlayer.clip = winVideo;
                resultText.text = "ВЫ ПОБЕДИЛИ!";
                resultText.color = new Color32(2, 140, 0, 255);
                break;
            case 2: //DRAW
                videoPlayer.clip = drawVideo;
                resultText.text = "НИЧЬЯ...";
                resultText.color = new Color32(240, 240, 240, 255);
                break;
            case 3: //LOOSE
                videoPlayer.clip = looseVideo;
                resultText.text = "ВЫ ПРОИГРАЛИ";
                resultText.color = new Color32(176, 2, 0, 255);
                break;
        }
        ResultPanel.SetActive(true);
        videoPlayer.Play();
    }
    public void LoadMenu() 
    {
        Destroy(network);
        SceneManager.LoadScene("MainMenu"); 
    } 
}
