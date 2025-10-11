using System.Collections;
using UnityEngine;

public class GameMagager : MonoBehaviour
{
    public static GameMagager instance { get; private set; }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }
    BoardManager _boardManager;
    UIControl _uiController;
    EndGameUI _endGameUI;
    GameState _gameState;
    HighlighCellSelected _highlightCellSelected;
    public BoardManager BoardManager => _boardManager;
    public UIControl UIControl => _uiController;
    public EndGameUI EndGameUI => _endGameUI;
    public HighlighCellSelected HighlightCellSelected => _highlightCellSelected;

    public PlayerTurn _currentTurn = PlayerTurn.P1;
    public States _currentState = States.SelectingCell;

    int _p1Score = 0;
    int _p2Score = 0;
    int _p1StoneCount = 0;
    int _p2StoneCount = 0;
    int _p1Owed = 0;
    int _p2Owed = 0;

    public int P1Score => _p1Score;
    public int P2Score => _p2Score;
    public int P1StoneCount => _p1StoneCount;
    public int P2StoneCount => _p2StoneCount;
    public int P1Owe => _p1Owed;
    public int P2Owe => _p2Owed;

    int _selectedIndex = -1;
    int _direction = 0; // -1 trái, 1 phải

    // Sow timing
    float sowDelay = 0.35f;

    #region Initialize
    public void Initialize(GameState gameState)
    {
        _gameState = gameState;
        Transform board = transform.Find("board");
        _boardManager = board.GetComponent<BoardManager>();
        _uiController = gameObject.GetComponent<UIControl>();
        _endGameUI = transform.Find("End").GetComponent<EndGameUI>();
        _highlightCellSelected = board.GetComponent<HighlighCellSelected>();

        _boardManager.Initialize();
        _uiController.Initialize();
        _endGameUI.Init();
        _highlightCellSelected.Initialize();
        ResetGame();
        _uiController.UpdateBoard(_boardManager.board);
        _uiController.UpdatePlayer(_p1Score, _p2Score, _p1StoneCount, _p2StoneCount, _p1Owed, _p2Owed);
    }
    #endregion

    // ====== SỰ KIỆN CHỌN Ô ======
    #region Select Cell
    public void OnSelectCell(int index)
    {
        print($"click {index}");
        SoundManager.Instance.PlaySFX(Config.SFX.CLICK);
        if (_currentState != States.SelectingCell)
            return;
        if (!_boardManager.IsPlayerCell(index, _currentTurn))
            return;
        if (_boardManager.board[index] == 0 && !IsAllPlayerCellsEmpty(_currentTurn))
            return;

        _selectedIndex = index;

        // === [Mục 6.1] Kiểm tra mất dân và xử lý ===
        if (IsAllPlayerCellsEmpty(_currentTurn))
        {
            bool restored = FillPieces(_currentTurn);
            print($"tra diem: {restored} ");
            if (!restored)
            {

            }
            _uiController.UpdateBoard(_boardManager.board);
            _uiController.UpdatePlayer(_p1Score, _p2Score, _p1StoneCount, _p2StoneCount, _p1Owed, _p2Owed);
            // return;
        }

        _currentState = States.SelectingDirection;
        _highlightCellSelected.ShowHighlightCells(_selectedIndex);
        _uiController.ShowDirection();

    }
    #endregion

    public void CallbackHideArrowDirection()
    {
        _currentState = States.SelectingCell;
        _highlightCellSelected.HideHighlightCells();
    }

    #region Select direction
    // ====== SỰ KIỆN CHỌN HƯỚNG (TRÁI/PHẢI) ======
    public void OnSelectDirection(int dir)
    {
        if (_currentState != States.SelectingDirection)
            return;
        _direction = _currentTurn == PlayerTurn.P1 ? dir : -dir;
        _highlightCellSelected.HideHighlightCells();
        StartCoroutine(HandleTurn());
    }
    #endregion

    #region Handle main turn  
    // ====== XỬ LÝ LƯỢT CHƠI CHÍNH ======
    IEnumerator HandleTurn()
    {
        _currentState = States.Animating;

        int pos = _selectedIndex;
        int hand = _boardManager.board[pos];
        _boardManager.board[pos] = 0;
        _selectedIndex = -1;

        // === RẢI QUÂN ===
        while (hand > 0)
        {
            pos = (pos + _direction + 12) % 12;
            if (pos == _selectedIndex) continue; // [Mục 6.2] Bỏ qua ô xuất phát
            _boardManager.board[pos]++;
            hand--;
            SoundManager.Instance.PlaySFX(Config.SFX.MOVE);
            _uiController.UpdateBoard(_boardManager.board);
            yield return new WaitForSeconds(sowDelay);
        }

        // === XỬ LÝ SAU KHI RẢI ===
        yield return StartCoroutine(HandlePostMove(pos));

        _uiController.UpdateBoard(_boardManager.board);
        //_uiController.UpdatePlayer(_p1Score, _p2Score, _p1StoneCount, _p2StoneCount, _p1Owed, _p2Owed);

        // === KIỂM TRA KẾT THÚC GAME ===
        CheckGameOver();

        if (_currentState != States.GameOver)
        {
            _currentTurn = _currentTurn == PlayerTurn.P1 ? PlayerTurn.P2 : PlayerTurn.P1;
            _currentState = States.SelectingCell;
        }
    }
    #endregion

    #region Move continue or eat
    // ====== XỬ LÝ SAU RẢI QUÂN (đi tiếp hoặc ăn) ======
    IEnumerator HandlePostMove(int pos)
    {
        int next = (pos + _direction + 12) % 12;

        if (_boardManager.IsQuan(next)) yield break;

        if (_boardManager.board[next] > 0)
        {
            // [Mục 4.3 - Trường hợp 2] Đi tiếp nếu ô có quân
            int hand = _boardManager.board[next];
            _boardManager.board[next] = 0;
            pos = next;

            while (hand > 0)
            {
                pos = (pos + _direction + 12) % 12;
                _boardManager.board[pos]++;
                SoundManager.Instance.PlaySFX(Config.SFX.MOVE);
                hand--;
                _uiController.UpdateBoard(_boardManager.board);
                yield return new WaitForSeconds(sowDelay);
            }

            yield return StartCoroutine(HandlePostMove(pos));
        }
        else
        {
            // [Mục 4.3 - Trường hợp 3] Gặp ô trống -> ăn quân
            yield return StartCoroutine(CaptureChain(next));
        }
    }
    #endregion

    #region Continue eat
    // ====== XỬ LÝ ĂN DÂY ======
    IEnumerator CaptureChain(int emptyPos)
    {
        int next = (emptyPos + _direction + 12) % 12; // ô sau ô trống

        // Nếu gặp ô trống thì dừng lại
        if (/* _boardManager.IsQuan(next) || */ _boardManager.board[next] == 0)
            yield break;

        // Ăn quân ở ô tiếp theo
        
        int eaten = _boardManager.board[next];
        int point = _boardManager.GetPoint(next);
        _boardManager.EatStone(next);

        UpdatePlayerScore(_currentTurn, add: point);
        UpdatePlayerStoneCount(_currentTurn, add: eaten);

        TryRepayDebt();

        _uiController.UpdateBoard(_boardManager.board);
        print($"Ăn {eaten} quân ở ô {next} ({_currentTurn}) diem: {point}");
        _uiController.UpdatePlayer(_p1Score, _p2Score, _p1StoneCount, _p2StoneCount, _p1Owed, _p2Owed);
        yield return new WaitForSeconds(0.3f);

        // Kiểm tra tiếp: nếu ô sau ô vừa ăn là ô trống, tiếp tục ăn dây
        int afterNext = (next + _direction + 12) % 12;
        if (_boardManager.board[afterNext] == 0 && !_boardManager.IsQuan(afterNext))
        {
            yield return StartCoroutine(CaptureChain(afterNext));
        }
    }
    #endregion

    // ====== CHECK MẤT DÂN ======
    bool IsAllPlayerCellsEmpty(PlayerTurn player)
    {
        int start = player == PlayerTurn.P1 ? 0 : 6;
        for (int i = 0; i < 5; i++)
        {
            if (_boardManager.board[start + i] > 0)
                return false;
        }
        return true;
    }

    #region FillPieces
    private bool FillPieces(PlayerTurn player)
    {
        int needed = 5;
        // Lấy từ điểm của chính mình
        if (player == PlayerTurn.P1)
        {
            int used = Mathf.Min(_p1Score, needed);
            UpdatePlayerScore(PlayerTurn.P1, sub: used);
            UpdatePlayerStoneCount(PlayerTurn.P1, sub: used);
            needed -= used;
            _boardManager.board[0] = 0; 
        }
        else
        {
            int used = Mathf.Min(_p2Score, needed);
            UpdatePlayerScore(PlayerTurn.P2, sub: used);
            UpdatePlayerStoneCount(PlayerTurn.P2, sub: used);
            needed -= used;
        }

        // Nếu thiếu, mượn từ đối thủ
        if (needed > 0)
        {
            if (player == PlayerTurn.P1 && _p2Score >= needed)
            {
                UpdatePlayerScore(PlayerTurn.P2, sub: needed);
                UpdatePlayerStoneCount(PlayerTurn.P2, sub: needed);
                _p1Owed = needed;
            }
            else if (player == PlayerTurn.P2 && _p1Score >= needed)
            {
                UpdatePlayerScore(PlayerTurn.P1, sub: needed);
                UpdatePlayerStoneCount(PlayerTurn.P1, sub: needed);
                _p2Owed = needed;
            }
            else
            {
                // Không đủ cả hai
                EndGame();
                return false;
            }
        }

        int start = (player == PlayerTurn.P1) ? 0 : 6;
        for (int i = 0; i < 5; i++)
            _boardManager.board[start + i] = 1;
        return true;
    }
    #endregion

    // ====== Xử lí trả nợ ======
    void TryRepayDebt()
    {
        // Player1 trả nợ
        if (_p1Owed > 0)
        {
            int payment = Mathf.Min(_p1Score, _p1Owed);
            if (payment > 0)
            {
                UpdatePlayerScore(PlayerTurn.P1, sub: payment);
                UpdatePlayerScore(PlayerTurn.P2, add: payment);
                UpdatePlayerStoneCount(PlayerTurn.P1, sub: payment);
                UpdatePlayerStoneCount(PlayerTurn.P2, add: payment);
                _p1Owed -= payment;
                Debug.Log($"Player1 trả {payment} hạt nợ cho Player2, còn nợ {_p1Owed}");
            }
        }

        // Player2 trả nợ
        if (_p2Owed > 0)
        {
            int payment = Mathf.Min(_p2Score, _p2Owed);
            if (payment > 0)
            {
                UpdatePlayerScore(PlayerTurn.P2, sub: payment);
                UpdatePlayerScore(PlayerTurn.P1, add: payment);
                UpdatePlayerStoneCount(PlayerTurn.P2, sub: payment);
                UpdatePlayerStoneCount(PlayerTurn.P1, add: payment);
                _p2Owed -= payment;
                Debug.Log($"Player2 trả {payment} hạt nợ cho Player1, còn nợ {_p2Owed}");
            }
        }
    }

    #region Check Game Over
    // ====== KIỂM TRA KẾT THÚC GAME ======
    public void CheckGameOver()
    {
        bool quanHet = !_boardManager.IsQuanAvailable();
        bool danHet = true;

        for (int i = 0; i < 12; i++)
        {
            if (i == 5 || i == 11) continue;
            if (_boardManager.board[i] > 0)
            {
                danHet = false;
                break;
            }
        }

        if (quanHet || danHet)
        {
            EndGame();
        }
    }
    #endregion

    #region Handle End
    // ====== TÍNH ĐIỂM & KẾT THÚC GAME ======
    public void EndGame()
    {
        _currentState = States.GameOver;

        // [Mục 7] Vét bàn
        for (int i = 0; i < 5; i++)
        {
            UpdatePlayerScore(PlayerTurn.P1, add: _boardManager.board[i]);
            UpdatePlayerStoneCount(PlayerTurn.P1, add: _boardManager.board[i]);
        }
        for (int i = 6; i <= 10; i++)
        {
            UpdatePlayerScore(PlayerTurn.P2, add: _boardManager.board[i]);
            UpdatePlayerStoneCount(PlayerTurn.P2, add: _boardManager.board[i]);
        }
        // tinh no
        int effectiveP1 = _p1Score - _p1Owed;
        int effectiveP2 = _p2Score - _p2Owed;

        _uiController.UpdatePlayer(_p1Score, _p2Score, _p1StoneCount, _p2StoneCount, _p1Owed, _p2Owed);

        string result;
        if (effectiveP1 > effectiveP2)
            result = "Người chơi 1 thắng!";
        else if (effectiveP2 > effectiveP1)
            result = "Người chơi 2 thắng!";
        else
            result = "Hòa!";

        Debug.Log("Kết thúc: " + result);

        SoundManager.Instance.PlaySFX(Config.SFX.END_GAME);
        // xu ly UI ending
        _endGameUI.Show(
            summary: result,
            score1: _p1Score,
            score2: P2Score,
            callbackPlayAgain: ResetGame,
            callbackReturnToMenu: _gameState.CallbackBacktoMenu
        );
    }
    #endregion

    #region Update 
    public void UpdateManager()
    {
        // tinh giay 
        _uiController.UpdateStates(_currentTurn, _currentState);
        _uiController.UpdateOutline(_currentTurn);
    }

    public void UpdatePlayerStoneCount(PlayerTurn turn, int add = 0, int sub = 0)
    {
        if (turn == PlayerTurn.P1)
            _p1StoneCount = add != 0 ? _p1StoneCount + add : _p1StoneCount - sub;
        else
            _p2StoneCount = add != 0 ? _p2StoneCount + add : _p2StoneCount - sub;
    }

    public void UpdatePlayerScore(PlayerTurn turn, int add = 0, int sub = 0)
    {
        if (turn == PlayerTurn.P1)
            _p1Score = add != 0 ? _p1Score + add : _p1Score - sub;
        else
            _p2Score = add != 0 ? _p2Score + add : _p2Score - sub;
    }
    #endregion

    #region Reset Game
    /// <summary>
    /// Đưa mọi thứ về trạng thái bắt đầu: board, UI, score, turn, state…
    /// </summary>
    public void ResetGame()
    {
        SoundManager.Instance.PlaySFX(Config.SFX.START_GAME);
        StopAllCoroutines();

        _p1Score = _p2Score = _p1Owed = _p2Owed = _p1StoneCount = _p2StoneCount = 0;
        _currentTurn = PlayerTurn.P1;
        _currentState = States.SelectingCell;
        _boardManager.ResetBoard();

        _uiController.UpdateBoard(_boardManager.board);
        _uiController.UpdatePlayer(_p1Score, _p2Score, _p1StoneCount, _p2StoneCount, _p1Owed, _p2Owed);
        _uiController.UpdateStates(_currentTurn, _currentState);
    }
    #endregion

}
