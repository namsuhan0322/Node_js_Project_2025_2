using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NativeWebSocket;
using Newtonsoft.Json;
using UnityEngine.UI;
using System;

[Serializable]
public class NetworkMessage
{
    public string type;
    public string playerId;
    public string message;
    public Vector3Data position;
    public Vector3Data rotation;            // 회전 추가
}

[Serializable]
public class Vector3Data
{
    public float x, y, z;

    public Vector3Data(Vector3 v)
    {
        this.x = v.x;
        this.y = v.y;
        this.z = v.z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }
}

public class NetworkManager : MonoBehaviour
{
    private WebSocket _webSocket;
    [SerializeField] private string _serverUrl = "ws://localhost:3000";

    [Header("UI Elements")]
    [SerializeField] private InputField _messageInput;
    [SerializeField] private Button _sendButton;
    [SerializeField] private Button _connectButton;
    [SerializeField] private Text _chatLog;
    [SerializeField] private Text _statusText;

    [Header("PlayerSetting")]
    [SerializeField] private Transform _localPlayer;                    // 내 플레이어
    [SerializeField] private GameObject _remotePlayerPrefabs;           // 다른 플레이어 Prefabs
    [SerializeField] private float _positionSendRate = 0.1f;            // 위치 전송 간격

    private string _myPlayerId;
    private Dictionary<string, GameObject> _remotePlayers = new Dictionary<string, GameObject>();
    private float _lastPositionSendTime;

    void Start()
    {
        _sendButton.onClick.AddListener(SendChatMessage);
        _connectButton.onClick.AddListener(ConnectToServer);

        // enter 키로 메세지 전송
        if (_messageInput != null)
        {
            _messageInput.onEndEdit.AddListener((text) =>
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    SendChatMessage();
                }
            });
        }
    }


    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        if (_webSocket != null)
        {
            _webSocket.DispatchMessageQueue();
        }
#endif

        // 일정 간격으로 내 위치, 회전 값 전송
        if (_webSocket != null && _webSocket.State == WebSocketState.Open && _localPlayer != null)
        {
            if (Time.time - _lastPositionSendTime >= _positionSendRate)
            {
                SendPositionUpdate();
                _lastPositionSendTime = Time.time;
            }
        }
    }

    private async void ConnectToServer()
    {
        if (_webSocket != null && _webSocket.State == WebSocketState.Open)
        {
            AddToChatLog($"[시스템] 이미 연결되어 있습니다.");
            return;
        }

        UpdateStatusText("연결 중...", Color.yellow);

        _webSocket = new WebSocket( _serverUrl );

        _webSocket.OnOpen += () =>
        {
            UpdateStatusText("연결됨", Color.green);
            AddToChatLog("[시스템] 서버에 연결 되었습니다.");
        };

        _webSocket.OnError += (e) =>
        {
            UpdateStatusText("에러 발생", Color.cyan);
            AddToChatLog($"[시스템] 에러 : {e}");
        };

        _webSocket.OnClose += (e) =>
        {
            UpdateStatusText("연결 끊김", Color.red);
            AddToChatLog("[시스템] 서버와의 연결이 끊어졌습니다.");
        };

        _webSocket.OnClose += (e) =>
        {
            UpdateStatusText("연결 끊김", Color.red);
            AddToChatLog("[시스템] 서버와의 연결이 끊어졌습니다.");

            // 연결 끊김 시 모든 원격 플레이어 제거
            foreach (var player in _remotePlayers.Values)
            {
                if (player != null) Destroy(player);
            }
            _remotePlayers.Clear();
        };

        _webSocket.OnMessage += (bytes) =>
        {
            var message = System.Text.Encoding.UTF8.GetString(bytes);
            HandleMessage(message);
        };

        await _webSocket.Connect();
    }

    private void HandleMessage(string json)
    {
        try
        {
            NetworkMessage message = JsonConvert.DeserializeObject<NetworkMessage>(json);

            switch (message.type)
            {
                case " connection":
                    _myPlayerId = message.playerId;
                    AddToChatLog($"[시스템] {message.message} (당신의 ID : {_myPlayerId})");
                    break;
                case "chat": 
                    string displayName = message.playerId == _myPlayerId ? "나" : message.playerId;
                    AddToChatLog($"[{displayName}] {message.message}");
                    break;
                case "playerJoin":
                    if (message.playerId != _myPlayerId)
                    {
                        AddToChatLog($"[시스템] {message.playerId} 님이 입장했습니다.");
                        CreateRemotePlayer(message.playerId, message.position, message.rotation);
                    }
                    break;
                case "playerDisconnect":
                    AddToChatLog($"[시스템] {message.playerId} 님이 퇴장 했습니다");
                    RemoveRemotePlayer(message.playerId);
                    break;
                case "positionUpdate":
                    if (message.playerId != _myPlayerId)
                    {
                        UpdateRemotePlayer(message.playerId, message.position, message.rotation);
                    }
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"메세지 처리중인 에러 : {e.Message}");
        }
    }

    private async void SendChatMessage()
    {
        if (string.IsNullOrEmpty(_messageInput.text)) return;

        if (_webSocket == null || _webSocket.State != WebSocketState.Open)
        {
            AddToChatLog("[시스템] 서버에 연결되지 않았습니다.");
            return;
        }

        NetworkMessage message = new NetworkMessage
        {
            type = "chat",
            message = _messageInput.text
        };

        await _webSocket.SendText(JsonConvert.SerializeObject(message));
        _messageInput.text = "";
        _messageInput.ActivateInputField();             // 입력창 다시 활성화
    }

    private async void SendPositionUpdate()
    {
        if (_localPlayer == null) return;

        NetworkMessage message = new NetworkMessage
        {
            type = "positionUpdate",
            position = new Vector3Data(_localPlayer.position),
            rotation = new Vector3Data(_localPlayer.eulerAngles)
        };

        await _webSocket.SendText(JsonConvert.SerializeObject(message));
     }

    private void AddToChatLog(string message)
    {
        if (_chatLog != null)
        {
            _chatLog.text += $"\n{message}";
        }
    }

    private void UpdateStatusText(string status, Color color)
    {
        if (_statusText != null)
        {
            _statusText.text = status;
            _statusText.color = color;
        }
    }

    private async void OnApplicationQuit()
    {
        if (_webSocket != null && _webSocket.State == WebSocketState.Open)
        {
            await _webSocket.Close();
        }
    }

    private void CreateRemotePlayer(string playerId, Vector3Data position, Vector3Data rotation)
    {
        if (_remotePlayers.ContainsKey(playerId)) return;
        if (_remotePlayerPrefabs == null)
        {
            Debug.LogError("RemotePlayerPrefab이 설정 되지 않았습니다.");
            return;
        }

        Vector3 pos = position != null ? position.ToVector3() : Vector3.zero;
        Vector3 rot = rotation != null ? rotation.ToVector3() : Vector3.zero;

        GameObject player = Instantiate(_remotePlayerPrefabs, pos, Quaternion.Euler(rot));
        player.name = "RemotePlayer_" + playerId;
        _remotePlayers.Add(playerId, player);

        Debug.Log($"원격 플레이어 생성 : {playerId} at {pos}, rotation_{rot}");
    }

    private void RemoveRemotePlayer(string playerId)
    {
        if (_remotePlayers.ContainsKey(playerId))
        {
            Destroy(_remotePlayers[playerId]);
            _remotePlayers.Remove(playerId);
            Debug.Log($"원격 플레이어 제거 : {playerId}");
        }
    }

    private void UpdateRemotePlayer(string playerId, Vector3Data position, Vector3Data rotation)
    {
        // 플레이어가 없으면 생성
        if (!_remotePlayers.ContainsKey(playerId))
        {
            CreateRemotePlayer(playerId, position, rotation);
            return;
        }

        GameObject player = _remotePlayers[playerId];
        if (player == null) return;

        // 부드러운 이동
        if (position != null)
        {
            player.transform.position = Vector3.Lerp(player.transform.position, position.ToVector3(), Time.deltaTime * 10f);
        }
        // 부드러운 회전
        if (rotation != null)
        {
            Quaternion targetRotation = Quaternion.Euler(rotation.ToVector3());
            player.transform.rotation = Quaternion.Lerp(player.transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    private void OnDestroy()
    {
        if (_sendButton != null)
        {
            _sendButton.onClick.RemoveListener(SendChatMessage);
        }
        if (_connectButton != null)
        {
            _connectButton.onClick.RemoveListener(ConnectToServer);
        }
    }
}
