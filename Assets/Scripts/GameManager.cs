using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public GameObject ball;

    private int player1Score, player2Score;

    private void Start()
    {
        ball.GetComponent<BallMovement>().Play();
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
}