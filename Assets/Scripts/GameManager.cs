using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public enum GameState
{
    PlayerSelect,
    Handshake,
    Instructions,
    GamePlay
}

public class GameManager : MonoBehaviour
{
    public GameObject ball;
    private GameState currentState;
    public Text p1Text, p2Text;

    private int player1Score, player2Score;

    private void Start()
    {
        currentState = GameState.PlayerSelect;
    }

    public GameState GetCurrentState()
    {
        return currentState;
    }

    public void Scored(int player)
    {
        Debug.Log("player scored: " + player);
        if (player == 1) player1Score++;
        if (player == 2) player2Score++;

        NextRound();
    }

    public void NextRound()
    {
        ball.GetComponent<BallMovement>().Reset();
        ball.GetComponent<BallMovement>().Play();
    }

    private void Update()
    {
        if (currentState == GameState.PlayerSelect)
        {
            p1Text.text = "P1";
            p2Text.text = "P2";
        }
    }
}