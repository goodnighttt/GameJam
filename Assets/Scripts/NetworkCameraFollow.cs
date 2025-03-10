using UnityEngine;
using Photon.Realtime;
using Photon.Pun;


public class NetworkCameraFollow : MonoBehaviour
{
    public Transform target;         // 跟随目标（角色）
    public float distance = 5.0f;    // 相机距离
    public float height = 2.0f;      // 相机高度偏移
    public float smoothSpeed = 10.0f;// 平滑速度
    
    [Header("相机旋转设置")]
    public float mouseSensitivity = 3f;
    public float minVerticalAngle = -15f; // 向下看的最大角度
    public float maxVerticalAngle = 30f;  // 向上看的最大角度
    
    private float verticalRotation = 0f;  // 垂直旋转角度
    private Vector3 targetPosition;
    private Vector3 currentVelocity;
    
    void Start()
    {
        if (target == null)
            return;
            
        // 初始化垂直角度
        verticalRotation = 20f; // 默认稍微向下看一点
    }
    
    void LateUpdate()
    {
        if (target == null)
            return;
            
        // 处理鼠标垂直输入（只有本地玩家的相机才处理）
        if (target.GetComponent<PhotonView>().IsMine)
        {
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
            
            // 更新垂直旋转角度
            verticalRotation -= mouseY; // 注意这里是减，因为鼠标向上是正值
            verticalRotation = Mathf.Clamp(verticalRotation, minVerticalAngle, maxVerticalAngle);
        }
        
        // 计算相机位置
        // 1. 从目标位置开始
        // 2. 向后移动distance距离
        // 3. 根据垂直旋转角度调整高度
        
        // 计算相机的水平位置（在角色后方）
        Vector3 horizontalOffset = -target.forward * distance;
        
        // 计算垂直旋转的影响
        float verticalRad = verticalRotation * Mathf.Deg2Rad;
        float verticalOffset = height + distance * Mathf.Sin(verticalRad);
        
        // 调整水平距离（根据垂直角度）
        float adjustedHorizontalDistance = distance * Mathf.Cos(verticalRad);
        horizontalOffset = -target.forward * adjustedHorizontalDistance;
        
        // 最终相机位置
        targetPosition = target.position + horizontalOffset + Vector3.up * verticalOffset;
        
        // 使用SmoothDamp平滑移动相机
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref currentVelocity,
            1f / smoothSpeed
        );
        
        // 相机始终看向角色（稍微高一点的位置，通常是头部）
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
} 