using UnityEngine;

public class NetworkCameraFollow : MonoBehaviour
{
    public Transform target;
    public float distance = 5.0f;
    public float height = 2.0f;
    public float smoothSpeed = 10.0f;
    
    private Vector3 targetPosition;
    private Vector3 currentVelocity;
    
    void LateUpdate()
    {
        if (target == null)
            return;
            
        // 计算目标位置
        targetPosition = target.position - target.forward * distance + Vector3.up * height;
        
        // 使用 SmoothDamp 代替 Lerp
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref currentVelocity,
            1f / smoothSpeed  // 将smoothSpeed转换为时间
        );
        
        // 相机始终看向玩家
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
} 