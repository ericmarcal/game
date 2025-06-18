using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    public float speed;
    public float initialSpeed;
    private int index;
    private Animator anim;
    private Rigidbody2D rb;

    public List<Transform> paths = new List<Transform>();
    public bool isDialoguePaused = false;

    private void Start()
    {
        initialSpeed = speed;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {

        }
        if (paths == null || paths.Count == 0)
        {

        }
    }

    private void FixedUpdate()
    {
        if (rb == null || paths == null || paths.Count == 0) return;

        if (isDialoguePaused)
        {
            if (anim != null) anim.SetBool("isWalking", false);
            return;
        }

        if (anim != null) anim.SetBool("isWalking", true);

        Vector2 currentPosition = rb.position;
        Vector2 targetPosition = paths[index].position;
        Vector2 newPosition = Vector2.MoveTowards(currentPosition, targetPosition, speed * Time.fixedDeltaTime);

        rb.MovePosition(newPosition);

        if (Vector2.Distance(currentPosition, targetPosition) < 0.1f)
        {
            if (index < paths.Count - 1)
            {
                index++;
            }
            else
            {
                index = 0;
            }
        }

        Vector2 direction = targetPosition - currentPosition;
        if (direction.x < -0.01f)
        {
            transform.eulerAngles = new Vector2(0, 0);
        }
        else if (direction.x > 0.01f)
        {
            transform.eulerAngles = new Vector2(0, 180);
        }
    }
}