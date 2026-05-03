using UnityEngine;

public class AutoDestroyVFX : MonoBehaviour
{
    [Tooltip("Thời gian tồn tại của hiệu ứng (giây). Hãy chỉnh sao cho khớp với độ dài Animation")]
    public float lifeTime = 0.5f; 

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }
}