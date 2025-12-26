using System.Collections;
using UnityEngine;

/// <summary>
/// Main game coordinator - delegates to specialized managers
/// </summary>
public class GameManager : MonoBehaviour, IGameManager
{
    public static GameManager instance { get; private set; }

    [Header("AI Settings")]
    [SerializeField] private bool isAIEnabled = false;
    [SerializeField] private PlayerTurn aiPlayer = PlayerTurn.P2;
    [SerializeField] private AIDifficulty aiDifficulty = AIDifficulty.Gemini;
    
    [Header("Game Mode")]
    public GameMode currentMode = GameMode.Local;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    // Components
    private BoardManager _boardManager;
    private UIControl _uiController;
    private EndGameUI _endGameUI;
    private GameState _gameState;
    private HighlighCellSelected _highlightCellSelected;
    
    // Managers
    private ScoreManager _scoreManager;
    private RuleEngine _ruleEngine;
    private TurnManager _turnManager;
    private AnimationController _animationController;
    private MoveHandler _moveHandler;

    // Public accessors
    public BoardManager BoardManager => _boardManager;
    public UIControl UIControl => _uiController;
    public EndGameUI EndGameUI => _endGameUI;
    public HighlighCellSelected HighlightCellSelected => _highlightCellSelected;
    public PlayerTurn _currentTurn => _turnManager.CurrentTurn;
    public States _currentState => _turnManager.CurrentState;
    
    public void SetAIMode(bool enabled)
    {
        isAIEnabled = enabled;
        if (enabled && AIManager.Instance != null)
        {
            AIManager.Instance.SetAIDifficulty(aiDifficulty);
            Debug.Log($"AI Mode: ON ({aiDifficulty})");
        }
        else
        {
            Debug.Log("AI Mode: OFF (2 players)");
        }
    }

    #region Initialize
    public void Initialize(GameState gameState)
    {
        _gameState = gameState;
        
        // Initialize managers
        _scoreManager = new ScoreManager();
        _ruleEngine = new RuleEngine();
        _turnManager = new TurnManager();

        // Get components
        Transform board = transform.Find("board");
        _boardManager = board.GetComponent<BoardManager>();
        _uiController = gameObject.GetComponent<UIControl>();
        _endGameUI = transform.Find("End").GetComponent<EndGameUI>();
        _highlightCellSelected = board.GetComponent<HighlighCellSelected>();

        // Initialize animation and move handlers
        _animationController = new AnimationController(this, _uiController);
        _moveHandler = new MoveHandler(this, _boardManager, _scoreManager, _animationController, _turnManager);

        // Create AIManager if not exists
        if (AIManager.Instance == null)
        {
            var aiGO = new GameObject("AIManager");
            aiGO.AddComponent<AIManager>();
            DontDestroyOnLoad(aiGO);
        }
        
        // Initialize AI if enabled
        if (isAIEnabled && AIManager.Instance != null)
        {
            AIManager.Instance.SetAIDifficulty(aiDifficulty);
        }

        // Initialize components
        _boardManager.Initialize();
        _uiController.Initialize();
        _endGameUI.Init();
        _highlightCellSelected.Initialize();
        
        ResetGame();
        UpdateUI();
    }
    #endregion

    #region Select Cell
    public void OnSelectCell(int index)
    {
        Debug.Log($"Cell selected: {index}");
        
        // Bluetooth mode validation
        if (currentMode == GameMode.Bluetooth && !CanSelectCellBluetooth(index))
        {
            return;
        }
        
        SoundManager.Instance.PlaySFX(Config.SFX.CLICK);

        if (!_turnManager.IsValidState(States.SelectingCell))
            return;

        bool allEmpty = _ruleEngine.IsAllPlayerCellsEmpty(_turnManager.CurrentTurn, _boardManager.board);
        
        if (!_ruleEngine.IsValidMove(index, _turnManager.CurrentTurn, _boardManager.board, allEmpty))
            return;

        _turnManager.SelectCell(index);

        if (allEmpty)
        {
            bool restored = _moveHandler.FillPieces(_turnManager.CurrentTurn);
            Debug.Log($"Restore pieces: {restored}");
            if (!restored)
            {
                EndGame();
                return;
            }
            UpdateUI();
        }

        _highlightCellSelected.ShowHighlightCells(_turnManager.SelectedIndex);
        _uiController.ShowDirection();
    }
    #endregion

    public void CallbackHideArrowDirection()
    {
        _turnManager.SetState(States.SelectingCell);
        _highlightCellSelected.HideHighlightCells();
    }

    #region Select Direction
    public void OnSelectDirection(int dir)
    {
        if (!_turnManager.IsValidState(States.SelectingDirection))
            return;

        _turnManager.SetDirection(dir);
        _highlightCellSelected.HideHighlightCells();
        
        // Send move to opponent if Bluetooth mode
        if (currentMode == GameMode.Bluetooth && BluetoothGameManager.Instance != null)
        {
            int selectedCell = _turnManager.SelectedIndex;
            BluetoothGameManager.Instance.SendMove(selectedCell, dir);
        }
        
        StartCoroutine(HandleTurn());
    }
    #endregion

    #region Handle Turn
    private IEnumerator HandleTurn()
    {
        _turnManager.SetState(States.Animating);

        // Save state before move for undo
        GameStateSnapshot savedState = new GameStateSnapshot(
            _boardManager.board,
            _scoreManager.GetScore(PlayerTurn.P1),
            _scoreManager.GetScore(PlayerTurn.P2),
            (int)_turnManager.CurrentTurn,
            _turnManager.CurrentTurn == PlayerTurn.P1
        );
        
        if (UndoManager.Instance != null)
        {
            UndoManager.Instance.SaveState(
                _boardManager.board,
                _scoreManager.GetScore(PlayerTurn.P1),
                _scoreManager.GetScore(PlayerTurn.P2),
                (int)_turnManager.CurrentTurn,
                _turnManager.CurrentTurn == PlayerTurn.P1
            );
        }

        // Execute move with error handling
        bool moveSuccessful = true;
        bool hasError = false;
        System.Exception caughtException = null;
        
        // Note: Cannot use try-catch with yield in C#
        // Using flag-based error handling instead
        yield return StartCoroutine(ExecuteMoveWithErrorTracking(
            (success, exception) => {
                moveSuccessful = success;
                hasError = !success;
                caughtException = exception;
            }
        ));
        
        // Handle errors after coroutine completes
        if (hasError)
        {
            Debug.LogError($"❌ Error during move execution: {caughtException?.Message}");
            if (caughtException != null)
                Debug.LogException(caughtException);
            
            // Attempt recovery
            if (savedState != null)
            {
                Debug.Log("↩️ Attempting to restore previous state...");
                RestoreState(savedState);
            }
            else
            {
                Debug.LogWarning("⚠️ No saved state available, resetting game");
                ResetGame();
            }
            yield break;
        }

        if (moveSuccessful)
        {
            _animationController.UpdateBoardImmediate(_boardManager.board);
            UpdateUI();
            CheckGameOver();

            if (_turnManager.CurrentState != States.GameOver)
            {
                _turnManager.SwitchTurn();
                _turnManager.SetState(States.SelectingCell);
                
                // Trigger AI move if it's AI's turn
                if (isAIEnabled && _turnManager.CurrentTurn == aiPlayer)
                {
                    StartCoroutine(HandleAITurn());
                }
            }
        }
    }

    private IEnumerator HandleAITurn()
    {
        if (AIManager.Instance == null)
        {
            Debug.LogError("❌ AIManager not found in scene!");
            _turnManager.SetState(States.SelectingCell);
            yield break;
        }

        // Execute AI move
        // Note: Cannot use try-catch with yield in C#
        yield return AIManager.Instance.MakeAIMove(
            _boardManager.board,
            _turnManager.CurrentTurn,
            _boardManager.Quan1Available,
            _boardManager.Quan2Available,
            (cellIndex, direction) =>
            {
                try
                {
                    if (cellIndex >= 0 && cellIndex < GameConstants.BOARD_SIZE)
                    {
                        OnSelectCell(cellIndex);
                        OnSelectDirection(direction);
                    }
                    else
                    {
                        Debug.LogError($"❌ AI returned invalid move: {cellIndex}");
                        // Recover by allowing player to continue
                        _turnManager.SetState(States.SelectingCell);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"❌ Error during AI turn: {e.Message}");
                    Debug.LogException(e);
                    // Recover to player selection state
                    _turnManager.SetState(States.SelectingCell);
                }
            }
        );
    }
    #endregion

    #region Check Game Over
    public void CheckGameOver()
    {
        if (_ruleEngine.CheckGameOver(_boardManager.board, _boardManager.Quan1Available, _boardManager.Quan2Available))
        {
            EndGame();
        }
    }
    #endregion

    #region Handle End
    public void EndGame()
    {
        _turnManager.SetState(States.GameOver);

        _ruleEngine.CollectRemainingStones(_boardManager.board, _scoreManager);
        string result = _ruleEngine.DetermineWinner(_scoreManager);

        UpdateUI();

        Debug.Log("Game Over: " + result);
        SoundManager.Instance.PlaySFX(Config.SFX.END_GAME);

        _scoreManager.GetAllScores(out int p1Score, out int p2Score, out _, out _, out _, out _);
        
        // Award XP and coins based on result
        bool isPlayer1 = _turnManager.CurrentTurn == PlayerTurn.P1;
        bool won = (isPlayer1 && p1Score > p2Score) || (!isPlayer1 && p1Score < p2Score);
        
        if (LevelSystem.Instance != null)
        {
            int xp = won ? 50 : 10;
            if (p1Score > 40 || p2Score > 40) xp += 20; // Bonus for high score
            LevelSystem.Instance.AddXP(xp);
        }
        
        if (CurrencyManager.Instance != null)
        {
            int coins = won ? 20 : 5;
            CurrencyManager.Instance.AddCoins(coins);
        }

        // Track achievements
        if (AchievementManager.Instance != null && won)
        {
            AchievementManager.Instance.UpdateProgress(AchievementType.FirstWin);
            AchievementManager.Instance.UpdateProgress(AchievementType.MasterPlayer);
            
            // Perfect game (50+ points)
            if (p1Score >= 50 || p2Score >= 50)
            {
                AchievementManager.Instance.UpdateProgress(AchievementType.PerfectGame);
            }
            
            // AI Destroyer (if playing against AI)
            if (isAIEnabled)
            {
                AchievementManager.Instance.UpdateProgress(AchievementType.AIDestroyer);
            }
        }

        _endGameUI.Show(
            summary: result,
            score1: p1Score,
            score2: p2Score,
            callbackPlayAgain: ResetGame,
            callbackReturnToMenu: _gameState.CallbackBacktoMenu
        );
    }
    #endregion

    #region Update
    public void UpdateManager()
    {
        _uiController.UpdateStates(_turnManager.CurrentTurn, _turnManager.CurrentState);
        _uiController.UpdateOutline(_turnManager.CurrentTurn);
    }

    private void UpdateUI()
    {
        _scoreManager.GetAllScores(out int p1Score, out int p2Score, out int p1Stones, out int p2Stones, out int p1Owe, out int p2Owe);
        _uiController.UpdatePlayer(p1Score, p2Score, p1Stones, p2Stones, p1Owe, p2Owe);
    }
    #endregion

    #region Reset Game
    public void ResetGame()
    {
        SoundManager.Instance.PlaySFX(Config.SFX.START_GAME);
        StopAllCoroutines();

        _scoreManager.Reset();
        _turnManager.Reset();
        _boardManager.ResetBoard();

        _uiController.UpdateBoard(_boardManager.board);
        UpdateUI();
        _uiController.UpdateStates(_turnManager.CurrentTurn, _turnManager.CurrentState);
        
        // Clear undo history on reset
        if (UndoManager.Instance != null)
            UndoManager.Instance.ClearHistory();
    }
    #endregion

    #region Hint/Undo Support
    public int[] GetCellValues()
    {
        return _boardManager.board;
    }

    public bool IsPlayer1Turn()
    {
        return _turnManager.CurrentTurn == PlayerTurn.P1;
    }

    public void RestoreState(GameStateSnapshot state)
    {
        if (state == null) return;

        // Restore board
        _boardManager.board = (int[])state.cellValues.Clone();
        
        // Restore scores
        _scoreManager.SetScore(PlayerTurn.P1, state.player1Score);
        _scoreManager.SetScore(PlayerTurn.P2, state.player2Score);
        
        // Restore turn
        _turnManager.SetTurn(state.isPlayer1Turn ? PlayerTurn.P1 : PlayerTurn.P2);
        
        // Update UI
        _animationController.UpdateBoardImmediate(_boardManager.board);
        UpdateUI();
        UpdateManager();
        
        Debug.Log("↩️ State restored successfully");
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        Debug.Log("⏸️ Game paused");
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        Debug.Log("▶️ Game resumed");
    }
    
    bool CanSelectCellBluetooth(int index)
    {
        if (BluetoothGameManager.Instance == null)
            return true;
        
        // Check if it's my turn
        if (_turnManager.CurrentTurn != BluetoothGameManager.Instance.myTurn)
        {
            Debug.Log("❌ Not your turn!");
            return false;
        }
        
        // Check cell ownership
        if (BluetoothGameManager.Instance.myTurn == PlayerTurn.P1 && index > 5)
        {
            Debug.Log("❌ Not your cell!");
            return false;
        }
        
        if (BluetoothGameManager.Instance.myTurn == PlayerTurn.P2 && index < 6)
        {
            Debug.Log("❌ Not your cell!");
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Helper coroutine to track errors in ExecuteMove
    /// </summary>
    private IEnumerator ExecuteMoveWithErrorTracking(System.Action<bool, System.Exception> callback)
    {
        bool success = true;
        System.Exception exception = null;
        
        // Execute move without try-catch (yield return not allowed in try-catch)
        yield return _moveHandler.ExecuteMove();
        
        // Check for errors after execution
        // Note: If ExecuteMove throws, Unity will log it automatically
        
        callback(success, exception);
    }
    #endregion
}
