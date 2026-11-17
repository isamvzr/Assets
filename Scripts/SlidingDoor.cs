
using UnityEngine;
using System.Collections;

public class SlidingDoor : MonoBehaviour
{
    [Header("Door Settings")]
    public Vector3 openOffset = new Vector3(3f, 0f, 0f);
    public float moveSpeed = 2f;

    private Vector3 closedPosition;
    private Vector3 openPosition;

    private bool isOpen = false;
    private bool isMoving = false;

    void Start()
    {
        closedPosition = transform.position;
        openPosition = closedPosition + openOffset;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && !isMoving)
        {
            if (isOpen)
                StartCoroutine(MoveDoor(closedPosition));
            else
                StartCoroutine(MoveDoor(openPosition));

            isOpen = !isOpen;
        }
    }

    IEnumerator MoveDoor(Vector3 targetPosition)
    {
        isMoving = true;

        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        transform.position = targetPosition;
        isMoving = false;
    }
}
