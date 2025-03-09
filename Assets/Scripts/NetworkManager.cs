using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("房间设置")]
    public string roomName = "SplitScreenRoom";
    public byte maxPlayers = 2;
    
    [Header("玩家预制体")]
    public GameObject playerPrefab;
    
    [Header("玩家生成位置")]
    public Transform[] spawnPoints;
    
    [Header("UI引用")]
    public GameObject connectingPanel;
    public GameObject gamePanel;
    
    private void Start()
    {
        // 显示连接面板
        if (connectingPanel != null)
            connectingPanel.SetActive(true);
        if (gamePanel != null)
            gamePanel.SetActive(false);
            
        // 连接到Photon服务器
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("正在连接到Photon服务器...");
    }
    
    public override void OnConnectedToMaster()
    {
        Debug.Log("已连接到Photon服务器");
        
        // 设置同步场景
        PhotonNetwork.AutomaticallySyncScene = true;
        
        // 加入或创建房间
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = maxPlayers,
            IsVisible = true,
            IsOpen = true
        };
        
        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
    }
    
    public override void OnJoinedRoom()
    {
        Debug.Log("已加入房间: " + PhotonNetwork.CurrentRoom.Name);
        
        // 隐藏连接面板，显示游戏面板
        if (connectingPanel != null)
            connectingPanel.SetActive(false);
        if (gamePanel != null)
            gamePanel.SetActive(true);
            
        // 生成玩家
        SpawnPlayer();
    }
    
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("玩家加入: " + newPlayer.NickName);
    }
    
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("玩家离开: " + otherPlayer.NickName);
    }
    
    private void SpawnPlayer()
    {
        // 确定生成点
        int spawnIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        if (spawnIndex >= spawnPoints.Length)
            spawnIndex = 0;
            
        // 生成网络玩家
        Vector3 spawnPosition = spawnPoints[spawnIndex].position;
        Quaternion spawnRotation = spawnPoints[spawnIndex].rotation;
        
        GameObject playerObj = PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition, spawnRotation);
        
        // 设置玩家ID
        NetworkPlayer networkPlayer = playerObj.GetComponent<NetworkPlayer>();
        if (networkPlayer != null)
        {
            networkPlayer.photonView.Owner.NickName = "Player" + PhotonNetwork.LocalPlayer.ActorNumber;
        }
        
        // 创建分屏管理器
        GameObject splitScreenObj = new GameObject("SplitScreenManager");
        NetworkSplitScreenManager splitScreenManager = splitScreenObj.AddComponent<NetworkSplitScreenManager>();
        splitScreenManager.localPlayerObj = playerObj;
        
        // 根据ActorNumber决定玩家位置（1为左侧，2为右侧）
        bool isPlayer1 = (PhotonNetwork.LocalPlayer.ActorNumber == 1);
        splitScreenManager.isLocalPlayerOnLeft = isPlayer1;
    }
} 