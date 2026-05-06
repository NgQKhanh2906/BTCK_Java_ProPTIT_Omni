using UnityEngine;

public class AntiFlip : MonoBehaviour
{
    private Vector3 baseScale;
    private Quaternion baseRot;
    private void Awake()
    {
        baseScale = transform.localScale;
        baseRot = transform.rotation;
    }

    private void LateUpdate()
    {
        transform.rotation = baseRot;
        if (transform.parent != null)
        {
            float dir = Mathf.Sign(transform.parent.localScale.x);
            transform.localScale = new Vector3(Mathf.Abs(baseScale.x) * dir, baseScale.y, baseScale.z);
        }
    }
}