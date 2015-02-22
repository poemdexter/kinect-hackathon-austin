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

public enum Rules
{
    Pong,
    Burger
}

public class GameManager : MonoBehaviour
{
    public GameObject ball;
    private GameState currentState;
    private Rules currentRule;
    public Text p1Text, p2Text, infoText, instructionText;
    private bool playersSelected = false;

    private int player1Score, player2Score;

    private void Start()
    {
        currentState = GameState.PlayerSelect;
    }

    public GameState GetCurrentState()
    {
        return currentState;
    }

    public void PlayersSelected()
    {
        if (!playersSelected)
        {
            StartCoroutine(WaitForTime(2f));
        }
        playersSelected = true;
    }

    IEnumerator WaitForTime(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        currentState = GameState.Handshake;
    }

    public void HandsShook()
    {
        currentRule = Rules.Pong;
        currentState = GameState.Instructions;
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
            infoText.text = "PONGEMONIUM SPORTS GAME";
        }
        else if (currentState == GameState.Handshake)
        {
            infoText.text = "shake hands for good sport";
        }
        else if (currentState == GameState.Instructions)
        {
            p1Text.enabled = true;
            p2Text.enabled = true;
            infoText.enabled = false;
            instructionText.enabled = true;
            instructionText.text = GetInstructions();
        }
    }

    private string GetInstructions()
    {
        switch (currentRule)
        {
            case Rules.Pong:
                return "the rule: a game like pong use hands to move paddle";
            case Rules.Burger:
                return "new rule: hold hands together near mouth like eating champion burger";
        }
        return "foul";
    }
}