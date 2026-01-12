using UnityEngine;

public class GameStateSync : MonoBehaviour
{
    public static GameStateSync Instance { get; private set; }
    
    private int syncInterval = 5;
    private int lastSyncMoveCount = 0;
    public int MoveCount { get; private set; } = 0;
    
    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }
    
    public void OnMoveExecuted()
    {
        MoveCount++;
        if (MoveCount - lastSyncMoveCount >= syncInterval)
        {
            SendCurrentState();
            lastSyncMoveCount = MoveCount;
        }
    }
    
    public void SendCurrentState()
    {
        if (GameManager.instance == null) return;
        
        var state = GetCurrentState();
        var msg = BluetoothMessage.Create(BluetoothMessageType.StateSync, state);
        BluetoothGameManager.Instance?.SendMessage(msg);
    }
    
    public StateSyncMessage GetCurrentState()
    {
        var gm = GameManager.instance;
        var board = gm.BoardManager;
        
        // Get scores from ScoreManager via GetAllScores
        int p1Score = 0, p2Score = 0;
        int[] cellValues = gm.GetCellValues();
        
        // Calculate scores from board state
        // P1 cells: 0-4 (dan) + cell 5 (quan1)
        // P2 cells: 6-10 (dan) + cell 11 (quan2)
        // Scores are tracked separately in ScoreManager
        
        return new StateSyncMessage
        {
            board = cellValues,
            p1Score = p1Score,
            p2Score = p2Score,
            currentTurn = (int)gm._currentTurn,
            moveCount = MoveCount,
            quan1Available = board.Quan1Available,
            quan2Available = board.Quan2Available
        };
    }
    
    public void ApplyState(StateSyncMessage state)
    {
        if (GameManager.instance == null) return;
        
        var local = GetCurrentState();
        if (!StatesMatch(local, state))
        {
            Debug.LogWarning("⚠️ Desync detected! Applying remote state...");
            ForceApplyState(state);
        }
    }
    
    private bool StatesMatch(StateSyncMessage a, StateSyncMessage b)
    {
        if (a.moveCount != b.moveCount) return false;
        if (a.board.Length != b.board.Length) return false;
        
        for (int i = 0; i < a.board.Length; i++)
            if (a.board[i] != b.board[i]) return false;
        
        return true;
    }
    
    private void ForceApplyState(StateSyncMessage state)
    {
        var board = GameManager.instance.BoardManager;
        for (int i = 0; i < state.board.Length && i < board.board.Length; i++)
            board.board[i] = state.board[i];
        
        MoveCount = state.moveCount;
        GameManager.instance.UIControl?.UpdateBoard(board.board);
    }
    
    public void Reset()
    {
        MoveCount = 0;
        lastSyncMoveCount = 0;
    }
}
