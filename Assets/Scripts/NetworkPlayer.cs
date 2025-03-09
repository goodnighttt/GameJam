using UnityEngine;
using Photon.Pun;

public class NetworkPlayer : MonoBehaviourPun, IPunObservable
{
    [Header("移动设置")]
    public float moveSpeed = 5f;
    public float jumpForce = 8f;
    public float gravity = -9.81f;
    
    [Header("组件引用")]
    public CharacterController controller;
    
    // 移动相关变量
    private Vector3 velocity;
    private bool isGrounded;
    
    // 网络同步变量
    private Vector3 networkPosition;
    private Vector3 networkVelocity;
    
    private void Awake()
    {
        // 获取组件引用
        if (controller == null)
            controller = GetComponent<CharacterController>();
            
        // 初始化网络同步变量
        networkPosition = transform.position;
        networkVelocity = Vector3.zero;
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
            
            // 获取输入
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            
            // 计算移动方向（世界坐标系）
            Vector3 movement = new Vector3(horizontal, 0, vertical);
            
            // 限制对角线移动速度
            if (movement.magnitude > 1f)
                movement.Normalize();
                
            // 应用移动（不改变朝向）
            controller.Move(movement * moveSpeed * Time.deltaTime);
            
            // 处理跳跃
            if (isGrounded && Input.GetButtonDown("Jump"))
            {
                velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            }
            
            // 应用重力
            velocity.y += gravity * Time.deltaTime;
            
            // 应用垂直移动
            controller.Move(velocity * Time.deltaTime);
            
            // 移除了更新朝向的代码，保持角色原有朝向
        }
        // 远程玩家平滑移动
        else
        {
            // 使用CharacterController.Move进行平滑移动
            Vector3 targetPosition = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 10);
            controller.Move(targetPosition - transform.position);
        }
    }
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(velocity);
            stream.SendNext(isGrounded);
        }
        else
        {
            networkPosition = (Vector3)stream.ReceiveNext();
            networkVelocity = (Vector3)stream.ReceiveNext();
            isGrounded = (bool)stream.ReceiveNext();
        }
    }
}