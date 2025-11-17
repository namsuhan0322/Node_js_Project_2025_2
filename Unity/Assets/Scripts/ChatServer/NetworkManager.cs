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

    private string _myPlayerId;

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
            UpdateStatusText("에러 발생", Color.red);
            AddToChatLog($"[시스템] 에러 : {e}");
        };

        _webSocket.OnClose += (e) =>
        {
            UpdateStatusText("연결 끊김", Color.cyan);
            AddToChatLog("[시스템] 서버와의 연결이 끊어졌습니다.");
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
                case "playerDisconnect":
                    AddToChatLog($"[시스템] {message.playerId} 님이 퇴장 했습니다");
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
            type = "Chat",
            message = _messageInput.text
        };

        await _webSocket.SendText(JsonConvert.SerializeObject(message));
        _messageInput.text = "";
        _messageInput.ActivateInputField();             // 입력창 다시 활성화
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
