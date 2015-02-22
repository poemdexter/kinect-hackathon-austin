using UnityEngine;
using System.Collections;

public class Goal : MonoBehaviour
{
    public int playerScored;
    public GameManager gameManager;

    private void OnTriggerEnter2D(Collider2D col)
    {
        gameManager.Scored(playerScored);
        Destroy(col.gameObject);
    }
}