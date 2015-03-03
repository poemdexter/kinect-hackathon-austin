using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public enum GameState
{
    PlayerSelect,
    Handshake,
    Instructions,
    GamePlay,
    Winner
}

public enum Rules
{
    Pong,
    Burger,
    Texan
}

public class GameManager : MonoBehaviour
{
    public GameObject ball;
    private GameState currentState;
    private Rules currentRule;
    private int currentRuleNumber;
    public Text p1Text, p2Text, infoText, instructionText;
    private bool playersSelected = false;
    private bool showingInstructions = false;
    private bool ballSpawned = false;
    private const int numberOfRules = 3;
    private int winScore = 3;
    private int winner = 0;

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

    IEnumerator WaitForTimeToStart(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        currentState = GameState.GamePlay;
        showingInstructions = false;
    }

    public void HandsShook()
    {
        currentRule = Rules.Pong;
        currentRuleNumber = 1;
        currentState = GameState.Instructions;
    }

    public void Scored(int player)
    {
        Debug.Log("player scored: " + player);
        if (player == 1) player1Score++;
        if (player == 2) player2Score++;
        if (player1Score >= winScore)
            Winner(1);
        else if (player2Score >= winScore)
            Winner(2);
        p1Text.text = player1Score.ToString();
        p2Text.text = player2Score.ToString();
        NextRound();
    }

    private void Winner(int player)
    {
        currentState = GameState.Winner;
        winner = player;
    }

    public void NextRound()
    {
        GetNextRuleNumber();
        GetNextRule();
        currentState = GameState.Instructions;
        ballSpawned = false;
    }

    private void GetNextRuleNumber()
    {
        if (currentRuleNumber + 1 > numberOfRules)
        {
            currentRuleNumber = 1;
        }
        else
        {
            currentRuleNumber++;
        }

    }

    private void GetNextRule()
    {
        switch (currentRuleNumber)
        {
            case 1:
                currentRule = Rules.Pong;
                break;
            case 2:
                currentRule = Rules.Burger;
                break;
            case 3:
                currentRule = Rules.Texan;
                break;
        }
    }

    public Rules GetCurrentRule()
    {
        return currentRule;
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
            if (!showingInstructions)
            {
                StartCoroutine(WaitForTimeToStart(10));
                showingInstructions = true;
            }
        }
        else if (currentState == GameState.GamePlay)
        {
            instructionText.enabled = false;
            if (!ballSpawned)
            {
                ballSpawned = true;
                GameObject sportball = (GameObject)Instantiate(ball);
                sportball.GetComponent<BallMovement>().Play();
            }
        }
        else if (currentState == GameState.Winner)
        {
            instructionText.enabled = true;
            instructionText.text = "PONGEMONIUM CHAMPION PLAYER " + winner;
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
            case Rules.Texan:
                return "new rule: shoot gun in air for great victory";
        }
        return "foul";
    }
}