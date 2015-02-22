using UnityEngine;
using System.Collections;

public class PaddleCollision : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("ball"))
            col.gameObject.GetComponent<BallMovement>().IncreaseSpeed();
    }
}