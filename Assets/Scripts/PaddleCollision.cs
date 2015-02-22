using UnityEngine;
using System.Collections;

public class PaddleCollision : MonoBehaviour
{
    private float xPosition;

    private void Awake()
    {
        xPosition = transform.position.x;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("ball"))
            col.gameObject.GetComponent<BallMovement>().IncreaseSpeed();
    }

    private void Update()
    {
        if (transform.position.x != xPosition)
        {
            rigidbody2D.velocity = Vector2.zero;
            transform.position = new Vector3(xPosition, transform.position.y, transform.position.z);
        }
    }
}