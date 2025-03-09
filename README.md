# 3D第三人称视角双人联机同屏游戏

## 需求

1. 实现3D第三人称视角的双人联机同屏游戏
2. 使用Photon Unity Networking (PUN)进行网络同步
3. 分屏显示两个玩家的视角
4. 防止角色穿过地面

## 实现

### 网络连接与玩家生成
- 使用`NetworkManager`脚本连接到Photon服务器
- 自动创建或加入房间
- 根据玩家ID在不同的生成点生成玩家

### 分屏系统
- 使用`NetworkSplitScreenManager`创建两个相机
- 根据玩家ID决定左右分屏位置
- 每个相机跟随一个玩家

### 相机跟随
- 使用`NetworkCameraFollow`实现第三人称相机跟随
- 相机保持一定距离和高度
- 平滑跟随玩家移动

### 玩家控制与物理
- 使用`NetworkPlayer`脚本处理玩家输入和移动
- 使用Rigidbody和CapsuleCollider进行物理模拟
- 实现地面检测防止角色穿过地面
- 添加跳跃功能和重力调整

### 网络同步
- 使用Photon的RPC和IPunObservable接口同步玩家状态
- 同步位置、速度和地面状态
- 远程玩家使用平滑插值移动

## 防止角色穿过地面的解决方案

1. **添加适当的碰撞体**：
   - 确保玩家预制体有CapsuleCollider组件
   - 调整碰撞体大小以匹配角色模型

2. **设置Rigidbody属性**：
   - 使用连续碰撞检测(Continuous Collision Detection)
   - 冻结旋转以防止角色倒下

3. **实现地面检测**：
   - 使用射线检测判断角色是否在地面上
   - 根据地面状态调整移动和跳跃行为

4. **增强重力**：
   - 添加额外重力使角色更快落地
   - 防止角色在斜坡或台阶上悬浮

5. **调整物理材质**：
   - 为角色和地面设置适当的物理材质
   - 调整摩擦力和弹性

## 使用说明

1. 确保场景中有地面对象，并将其Layer设置为"Ground"
2. 在Inspector中设置NetworkPlayer脚本的groundLayer为"Ground"
3. 调整groundCheckDistance、jumpForce和gravityMultiplier参数以获得最佳效果
4. 确保玩家预制体有Rigidbody和CapsuleCollider组件

## Mixamo使用指南

### 1. 获取角色模型
- 访问 mixamo.com
- 使用Adobe账号登录
- 选择 "Characters" 标签
- 选择喜欢的角色模型（推荐Y Bot或X Bot作为开始）

### 2. 获取动画
基础动画推荐清单：
- Idle（待机）
- Walking（行走）
- Running（跑步）
- Jump（跳跃）
- Jump Landing（着陆）

### 3. 导出步骤
1. 选择动画后点击"Download"
2. 选择以下设置：
   - Format: FBX for Unity
   - Skin: With Skin
   - Frames per Second: 30
   - Keyframe Reduction: None（保证动画质量）

### 4. Unity导入设置
1. 将下载的FBX文件拖入Unity项目
2. 在Inspector中设置：
   - Rig类型选择"Humanoid"
   - 勾选"Apply Root Motion"（如果需要）
   - 确保动画循环选项正确设置

### 5. 动画设置建议
```csharp
// 创建Animator Controller
// 设置基本参数
- isWalking (bool)
- isRunning (bool)
- isJumping (bool)
- verticalSpeed (float)

// 动画状态机示例结构
Idle -> Walking -> Running
  |
  -> Jumping -> Landing -> Idle
```

### 6. 基础动画代码示例
```csharp
public class CharacterAnimator : MonoBehaviour
{
    private Animator animator;
    private CharacterController controller;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
    }
    
    void Update()
    {
        // 获取移动输入
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        // 计算移动量
        bool isMoving = Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f;
        
        // 更新动画参数
        animator.SetBool("isWalking", isMoving);
        animator.SetBool("isRunning", isMoving && Input.GetKey(KeyCode.LeftShift));
        animator.SetBool("isJumping", !controller.isGrounded);
    }
}
```

### 7. 注意事项
1. 动画文件命名要规范
2. 正确设置动画循环
3. 调整动画过渡时间
4. 设置适当的动画层权重 

## 第三人称相机与角色控制系统

### 鼠标控制视角功能
1. 水平旋转（左右）
   - 控制整个角色的旋转
   - 影响移动方向
   - 平滑过渡

2. 垂直旋转（上下）
   - 仅控制相机视角
   - 有最大/最小角度限制
   - 不影响角色移动

3. 相机跟随系统
   - 跟随角色位置和旋转
   - 平滑过渡
   - 保持固定距离和高度

### 实现细节
1. 角色控制
   ```csharp
   // 水平旋转（左右）
   rotationY += mouseX * rotationSpeed;
   transform.rotation = Quaternion.Euler(0, rotationY, 0);
   
   // 垂直旋转（上下）
   rotationX -= mouseY * rotationSpeed;
   rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);
   cameraTarget.localRotation = Quaternion.Euler(rotationX, 0, 0);
   ```

2. 相机跟随
   ```csharp
   // 计算相机位置
   targetPosition = target.position - target.forward * distance + Vector3.up * height;
   
   // 平滑移动
   transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, 1f / smoothSpeed);
   ```

3. 移动控制
   ```csharp
   // 相对于角色朝向的移动
   Vector3 direction = new Vector3(horizontal, 0, vertical);
   Vector3 moveDirection = transform.TransformDirection(direction);
   controller.Move(moveDirection * moveSpeed * Time.deltaTime);
   ```

### 设置步骤
1. 创建CameraTarget子对象
2. 配置NetworkPlayer组件
3. 配置NetworkCameraFollow组件
4. 调整旋转和平滑参数 