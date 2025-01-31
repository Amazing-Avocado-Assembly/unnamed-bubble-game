using System.Linq;
using NaughtyAttributes;
using UnityEngine;

public class FlyingEnemyController : EnemyController
{
    enum State { Idle, Chase }

    [SerializeField] float speed = 5.0f;
    [SerializeField] float maxAcceleration = 1.0f;
    [SerializeField] float avoidRadius = 1.0f;
    [SerializeField] float avoidAcceleration = 0.5f;
    [SerializeField] float rotationMultiplier = 10.0f;
    [ShowNonSerializedField] State state = State.Idle;

    protected override void Update()
    {
        base.Update();
        if (rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            var angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg - 90;
            angle = Mathf.LerpAngle(rb.rotation, angle, Time.deltaTime * rotationMultiplier);
            rb.SetRotation(angle);
        }
    }

    void FixedUpdate()
    {
        Vector2 acceleration = Vector2.zero;

        // Avoid other enemies
        var enemyPositions = Physics2D.OverlapCircleAll((Vector2)transform.position, avoidRadius, 1 << gameObject.layer)
                                      .Where(c => c.CompareTag("Enemy") && c.transform.parent != transform)
                                      .Select(c => c.transform.position);
        foreach (var enemyPosition in enemyPositions)
        {
            Vector2 direction = transform.position - enemyPosition;
            if (direction.sqrMagnitude < 0.01f)
            {
                direction = Random.insideUnitCircle * 0.01f;
            }
            acceleration += direction.normalized * avoidAcceleration / direction.sqrMagnitude;
        }

        if (state == State.Idle)
        {
            acceleration += -rb.linearVelocity.normalized * maxAcceleration;
        }
        else // (state == State.Chase)
        {
            // Desired velocity => instantly move to the player position or go through the player if too close
            var desiredVelocity = PlayerDirection.sqrMagnitude > maxAcceleration * maxAcceleration
                                      ? PlayerDirection
                                      : PlayerDirection.normalized * speed;
            var steer = desiredVelocity - rb.linearVelocity;
            Debug.DrawLine(transform.position, transform.position + (Vector3)steer, Color.red);
            acceleration += steer;
        }

        acceleration = Vector2.ClampMagnitude(acceleration, maxAcceleration);
        rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity + acceleration * Time.fixedDeltaTime, speed);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            state = State.Chase;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            state = State.Idle;
        }
    }
}
