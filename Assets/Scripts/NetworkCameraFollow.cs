using UnityEngine;

public class NetworkCameraFollow : MonoBehaviour
{
    public Transform target;         // 跟随目标（角色）
    public Transform lookTarget;     // 视角目标（通常是角色头部或相机目标点）
    public float distance = 5.0f;    // 相机距离
    public float height = 2.0f;      // 相机高度偏移
    public float smoothSpeed = 10.0f;// 平滑速度
    
    private Vector3 targetPosition;
    private Vector3 currentVelocity;
    
    void Start()
    {
        // 如果没有设置lookTarget，使用target
        if (lookTarget == null && target != null)
            lookTarget = target;
    }
    
    void LateUpdate()
    {
        if (target == null)
            return;
            
        // 计算目标位置 - 考虑角色的旋转
        // 使用角色的后方向量乘以距离，加上高度偏移
        targetPosition = target.position - target.forward * distance + Vector3.up * height;
        
        // 使用SmoothDamp平滑移动相机
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref currentVelocity,
            1f / smoothSpeed
        );
        
        // 如果有lookTarget，相机看向它，否则看向角色
        if (lookTarget != null)
        {
            // 计算看向的位置 - 考虑垂直旋转
            Vector3 lookPosition = lookTarget.position + lookTarget.up * 0.5f;
            transform.LookAt(lookPosition);
        }
        else
        {
            transform.LookAt(target.position + Vector3.up * 1.5f);
        }
    }
} 