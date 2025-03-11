using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour
{
    public enum Status
    {
        Standing,
        Walking,
    }

    private Animator animator;
    private int currentIndex;
    private List<Vector3> movePath;

    public Vector3 TargetPos { get; private set; }
    private Vector3 startPos;
    private float moveStartTime;
    private Coroutine coroutine;

    public Status MoveStatus { get; private set; }
    public float walkSpeed = 1f;
    public UnityEvent<Vector3, Vector3> Moved;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        movePath = new List<Vector3>();
        currentIndex = 0;
    }

    private void Start()
    {
        animator.speed = 0f;
        MoveStatus = Status.Standing;
    }

    public void SetRoute(List<Vector3> path)
    {
        movePath = path;
        currentIndex = 0;

        if (coroutine != null)
        {
            currentIndex = -1;
            StopCoroutine(coroutine);
        }
        coroutine = StartCoroutine(moveCoroutine());
    }

    private IEnumerator moveCoroutine()
    {
        MoveStatus = Status.Walking;
        animator.speed = 1f;
        while (currentIndex < movePath.Count)
        {
            if (currentIndex >= 0)
            {
                TargetPos = movePath[currentIndex];
                startPos = transform.position;
                moveStartTime = Time.time;
            }
            ++currentIndex;

            while (Vector3.SqrMagnitude(transform.position - TargetPos) > 0.001f)
            {
                transform.position = Vector3.Lerp(startPos, TargetPos, (Time.time - moveStartTime) * walkSpeed);
                yield return null;
            }
            transform.position = TargetPos;
            Moved?.Invoke(startPos, TargetPos);
        }
        animator.speed = 0f;
        MoveStatus = Status.Standing;
    }
}
