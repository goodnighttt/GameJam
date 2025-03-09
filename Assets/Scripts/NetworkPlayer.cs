using UnityEngine;
using Photon.Pun;

public class NetworkPlayer : MonoBehaviourPun, IPunObservable
{
    [Header("移动设置")]
    public float moveSpeed = 5f;
    public float jumpForce = 8f;
    public float gravity = -9.81f;
    
    [Header("旋转设置")]
    public float rotationSpeed = 5f;
    public float minVerticalAngle = -60f;
    public float maxVerticalAngle = 60f;
    
    [Header("组件引用")]
    public CharacterController controller;
    public Transform cameraTarget; // 相机目标点，可以是角色头部
    
    // 移动相关变量
    private Vector3 velocity;
    private bool isGrounded;
    
    // 旋转相关变量
    private float rotationX = 0f; // 垂直旋转角度
    private float rotationY = 0f; // 水平旋转角度
    private Quaternion networkRotation;
    
    // 网络同步变量
    private Vector3 networkPosition;
    private Vector3 networkVelocity;
    
    private void Awake()
    {
        // 获取组件引用
        if (controller == null)
            controller = GetComponent<CharacterController>();
            
        // 如果没有设置相机目标，使用当前Transform
        if (cameraTarget == null)
            cameraTarget = transform;
            
        // 初始化网络同步变量
        networkPosition = transform.position;
        networkVelocity = Vector3.zero;
        networkRotation = transform.rotation;
        
        // 如果是本地玩家，锁定并隐藏鼠标
        if (photonView.IsMine)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    private void Update()
    {
        // 只处理本地玩家的输入
        if (photonView.IsMine)
        {
            // 获取地面状态
            isGrounded = controller.isGrounded;
            
            // 重置Y轴速度（如果在地面上）
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f; // 使用小的负值确保与地面接触
            }
            
            // 处理旋转
            HandleRotation();
            
            // 处理移动
            HandleMovement();
            
            // 处理跳跃
            if (isGrounded && Input.GetButtonDown("Jump"))
            {
                velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            }
            
            // 应用重力
            velocity.y += gravity * Time.deltaTime;
            
            // 应用垂直移动
            controller.Move(velocity * Time.deltaTime);
        }
        // 远程玩家平滑移动
        else
        {
            // 平滑移动到网络位置
            Vector3 targetPosition = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 10);
            controller.Move(targetPosition - transform.position);
            
            // 平滑旋转到网络旋转
            transform.rotation = Quaternion.Slerp(transform.rotation, networkRotation, Time.deltaTime * 10);
        }
    }
    
    private void HandleRotation()
    {
        // 获取鼠标输入
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        
        // 计算水平旋转（左右）
        rotationY += mouseX * rotationSpeed;
        
        // 计算垂直旋转（上下）- 仅用于相机，不影响角色
        rotationX -= mouseY * rotationSpeed; // 注意这里是减，因为鼠标向上是正值
        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);
        
        // 应用水平旋转到角色
        transform.rotation = Quaternion.Euler(0, rotationY, 0);
        
        // 垂直旋转信息存储在cameraTarget中，相机会使用这个信息
        cameraTarget.localRotation = Quaternion.Euler(rotationX, 0, 0);
    }
    
    private void HandleMovement()
    {
        // 获取输入
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        // 计算移动方向（相对于角色朝向）
        Vector3 direction = new Vector3(horizontal, 0, vertical);
        
        // 限制对角线移动速度
        if (direction.magnitude > 1f)
            direction.Normalize();
            
        // 将方向转换为世界坐标
        Vector3 moveDirection = transform.TransformDirection(direction);
        
        // 应用移动
        controller.Move(moveDirection * moveSpeed * Time.deltaTime);
    }
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(velocity);
            stream.SendNext(isGrounded);
            stream.SendNext(transform.rotation);
            stream.SendNext(cameraTarget.localRotation);
        }
        else
        {
            networkPosition = (Vector3)stream.ReceiveNext();
            networkVelocity = (Vector3)stream.ReceiveNext();
            isGrounded = (bool)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
            
            // 远程玩家的相机目标旋转
            if (cameraTarget != null)
                cameraTarget.localRotation = (Quaternion)stream.ReceiveNext();
        }
    }
    
    private void OnDestroy()
    {
        // 如果是本地玩家，解锁鼠标
        if (photonView.IsMine)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}