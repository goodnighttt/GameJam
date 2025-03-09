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