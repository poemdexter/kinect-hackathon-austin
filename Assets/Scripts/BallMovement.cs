using UnityEngine;
using System.Collections;

public class BallMovement : MonoBehaviour
{
    public float speed;

    public void Reset()
    {
        rigidbody2D.velocity = Vector2.zero;
        transform.position = Vector3.zero;
    }

    public void Play()
    {
        int direction = ((Random.Range(0, 100)) < 50) ? 1 : -1;
        rigidbody2D.velocity = new Vector2(1*speed*direction, 1*speed);
    }
}