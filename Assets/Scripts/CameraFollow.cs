using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector2 offset = Vector2.zero;
    [SerializeField, Range(0.01f, 1f)] private float smoothSpeed = 0.125f;

    private float zPos;

    void Awake()
    {
        // preserve camera z so we don't accidentally change it
        zPos = transform.position.z;

        if (target == null)
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null) target = player.transform;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos = new Vector3(target.position.x + offset.x, target.position.y + offset.y, zPos);
        Vector3 smoothed = Vector3.Lerp(transform.position, desiredPos, 1f - Mathf.Pow(1f - smoothSpeed, Time.deltaTime * 60f));
        transform.position = smoothed;
    }
}
