
using UnityEngine;

public class PatrolPointGizmos : MonoBehaviour
{
    public Color pointColor = Color.green;
    public float pointSize = 0.5f;

    public Transform[] patrolPoints;

    void OnDrawGizmos()
    {
        Gizmos.color = pointColor;

        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] == null)
                continue;

            Gizmos.DrawSphere(patrolPoints[i].position, pointSize);

            int nextIndex = (i + 1) % patrolPoints.Length;

            if (patrolPoints[nextIndex] != null)
            {
                Gizmos.DrawLine(
                    patrolPoints[i].position,
                    patrolPoints[nextIndex].position
                );
            }
        }
    }
}
