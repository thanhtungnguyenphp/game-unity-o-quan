using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BluetoothMessageQueue : MonoBehaviour
{
    public static BluetoothMessageQueue Instance { get; private set; }
    
    private Queue<BluetoothMessage> messageQueue = new Queue<BluetoothMessage>();
    private bool isProcessing = false;
    
    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }
    
    public void Enqueue(BluetoothMessage message)
    {
        messageQueue.Enqueue(message);
        if (!isProcessing)
            StartCoroutine(ProcessQueue());
    }
    
    private IEnumerator ProcessQueue()
    {
        isProcessing = true;
        
        while (messageQueue.Count > 0)
        {
            var msg = messageQueue.Dequeue();
            yield return ProcessMessage(msg);
            yield return new WaitForSeconds(0.1f);
        }
        
        isProcessing = false;
    }
    
    private IEnumerator ProcessMessage(BluetoothMessage msg)
    {
        var msgType = (BluetoothMessageType)msg.type;
        
        switch (msgType)
        {
            case BluetoothMessageType.Move:
                yield return ProcessMove(msg);
                break;
            case BluetoothMessageType.StateSync:
                ProcessStateSync(msg);
                break;
            case BluetoothMessageType.Heartbeat:
                HeartbeatManager.Instance?.OnHeartbeatReceived();
                break;
            case BluetoothMessageType.Ack:
                // ACK received, can remove from pending
                break;
            case BluetoothMessageType.RequestSync:
                GameStateSync.Instance?.SendCurrentState();
                break;
        }
    }
    
    private IEnumerator ProcessMove(BluetoothMessage msg)
    {
        var move = JsonUtility.FromJson<MoveMessage>(msg.payload);
        
        if (!IsValidMove(move))
        {
            Debug.LogError($"❌ Invalid move: {move.cellIndex}");
            yield break;
        }
        
        GameManager.instance.OnSelectCell(move.cellIndex);
        GameManager.instance.OnSelectDirection(move.direction);
        
        yield return new WaitForSeconds(0.5f);
        SendAck(msg.messageId, true);
    }
    
    private bool IsValidMove(MoveMessage move)
    {
        if (GameManager.instance == null) return false;
        
        var myTurn = BluetoothGameManager.Instance?.myTurn ?? PlayerTurn.P1;
        if (GameManager.instance._currentTurn == myTurn) return false;
        if (move.cellIndex < 0 || move.cellIndex >= GameConstants.BOARD_SIZE) return false;
        if (move.direction != -1 && move.direction != 1) return false;
        
        var board = GameManager.instance.GetCellValues();
        if (board[move.cellIndex] <= 0) return false;
        
        return true;
    }
    
    private void ProcessStateSync(BluetoothMessage msg)
    {
        var state = JsonUtility.FromJson<StateSyncMessage>(msg.payload);
        GameStateSync.Instance?.ApplyState(state);
    }
    
    private void SendAck(int messageId, bool success)
    {
        var ack = new AckMessage { messageId = messageId, success = success };
        var msg = BluetoothMessage.Create(BluetoothMessageType.Ack, ack);
        BluetoothGameManager.Instance?.SendMessage(msg);
    }
}
