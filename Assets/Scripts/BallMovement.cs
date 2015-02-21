using UnityEngine;
using System.Collections;

public class BallMovement : MonoBehaviour
{
    public float startSpeed;
    public float speedIncreaseAmount;
    private float currentSpeed;

    public void Reset()
    {
        rigidbody2D.velocity = Vector2.zero;
        transform.position = Vector3.zero;
    }

    public void Play()
    {
        currentSpeed = startSpeed;
        int direction = ((Random.Range(0, 100)) < 50) ? 1 : -1;
        rigidbody2D.velocity = new Vector2(1*currentSpeed*direction, 1*currentSpeed);
    }

    public void IncreaseSpeed()
    {
        rigidbody2D.velocity = rigidbody2D.velocity + (rigidbody2D.velocity*speedIncreaseAmount);
    }
}