using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;

public class NetworkSplitScreenManager : MonoBehaviour
{
    [Header("本地玩家")]
    public GameObject localPlayerObj;
    
    [Header("相机设置")]
    public float cameraDistance = 5.0f;
    public float cameraHeight = 2.0f;
    public float smoothSpeed = 10.0f;
    
    [Header("分屏设置")]
    public bool isLocalPlayerOnLeft = true; // 本地玩家是否在左侧屏幕
    
    private Camera localCamera;
    private Camera remoteCamera;
    private GameObject remotePlayerObj;
    
    private void Start()
    {
        // 创建本地玩家相机
        GameObject localCameraObj = new GameObject("LocalPlayerCamera");
        localCamera = localCameraObj.AddComponent<Camera>();
        
        // 创建远程玩家相机
        GameObject remoteCameraObj = new GameObject("RemotePlayerCamera");
        remoteCamera = remoteCameraObj.AddComponent<Camera>();
        
        // 根据设置决定哪个相机在左侧，哪个在右侧
        if (isLocalPlayerOnLeft)
        {
            localCamera.rect = new Rect(0, 0, 0.5f, 1);
            remoteCamera.rect = new Rect(0.5f, 0, 0.5f, 1);
        }
        else
        {
            localCamera.rect = new Rect(0.5f, 0, 0.5f, 1);
            remoteCamera.rect = new Rect(0, 0, 0.5f, 1);
        }
        
        // 移除远程相机的AudioListener
        AudioListener remoteListener = remoteCameraObj.GetComponent<AudioListener>();
        if (remoteListener != null)
            Destroy(remoteListener);
            
        // 添加相机跟随脚本
        NetworkCameraFollow localCameraFollow = localCameraObj.AddComponent<NetworkCameraFollow>();
        localCameraFollow.target = localPlayerObj.transform;
        localCameraFollow.distance = cameraDistance;
        localCameraFollow.height = cameraHeight;
        localCameraFollow.smoothSpeed = smoothSpeed;
        
        NetworkCameraFollow remoteCameraFollow = remoteCameraObj.AddComponent<NetworkCameraFollow>();
        remoteCameraFollow.distance = cameraDistance;
        remoteCameraFollow.height = cameraHeight;
        remoteCameraFollow.smoothSpeed = smoothSpeed;
        
        // 开始寻找远程玩家
        InvokeRepeating("FindRemotePlayer", 0.5f, 0.5f);
    }
    
    private void FindRemotePlayer()
    {
        // 如果已经找到远程玩家，停止寻找
        if (remotePlayerObj != null)
        {
            CancelInvoke("FindRemotePlayer");
            return;
        }
        
        // 查找所有NetworkPlayer对象
        NetworkPlayer[] players = FindObjectsOfType<NetworkPlayer>();
        
        // 找到不是本地玩家的第一个玩家
        foreach (NetworkPlayer player in players)
        {
            if (!player.photonView.IsMine)
            {
                remotePlayerObj = player.gameObject;
                
                // 设置远程相机的目标
                NetworkCameraFollow remoteCameraFollow = remoteCamera.GetComponent<NetworkCameraFollow>();
                if (remoteCameraFollow != null)
                {
                    remoteCameraFollow.target = remotePlayerObj.transform;
                }
                
                // 停止寻找
                CancelInvoke("FindRemotePlayer");
                break;
            }
        }
    }
} 