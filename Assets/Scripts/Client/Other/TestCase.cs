using System.Collections;
using UnityEngine;

public class TestCase : MonoBehaviour
{
    public enum TestCaseType
    {
        Case1_P2RestoreByOwnPoint,
        Case2_P2BorrowFromP1,
        Case3_EndGameDueToNoPoints,
        Case4_Custom
    }

    public TestCaseType testCaseToRun;
    public bool triggerTest;
    public GameMagager gameManager;

    void Awake()
    {
        gameManager = GameObject.Find("Game").GetComponent<GameMagager>();
    }

    void Update()
    {
        if (triggerTest)
        {
            triggerTest = false;

            switch (testCaseToRun)
            {
                case TestCaseType.Case1_P2RestoreByOwnPoint:
                    Setup_Case1_P2Restore();
                    break;
                case TestCaseType.Case2_P2BorrowFromP1:
                    Setup_Case2_P2Borrow();
                    break;
                case TestCaseType.Case3_EndGameDueToNoPoints:
                    Setup_Case3_EndGame();
                    break;
                case TestCaseType.Case4_Custom:
                    Debug.Log("Bạn có thể thêm logic Case 4 ở đây.");
                    break;
            }
        }
    }

    void SetupCommon()
    {
        // Reset bàn cờ: P2 không còn quân ở ô 6–10
        for (int i = 6; i <= 10; i++)
        {
            gameManager.BoardManager.board[i] = 0;
        }

        // Reset stone count
        gameManager.UpdatePlayerStoneCount(PlayerTurn.P2, sub: gameManager.P2StoneCount);
        gameManager.UpdatePlayerStoneCount(PlayerTurn.P1, sub: gameManager.P1StoneCount);

        gameManager._currentTurn = PlayerTurn.P2;
        gameManager._currentState = States.SelectingCell;
    }

    void Setup_Case1_P2Restore()
    {
        SetupCommon();

        // P2 còn đủ điểm để hồi quân
        gameManager.UpdatePlayerScore(PlayerTurn.P2, add: 10);
        gameManager.UpdatePlayerStoneCount(PlayerTurn.P2, add: 10);

        // P1 dư điểm
        gameManager.UpdatePlayerScore(PlayerTurn.P1, add: 20);

        Debug.Log("🧪 Case 1: P2 sẽ dùng điểm để hồi quân.");
        UpdateUI();
    }

    void Setup_Case2_P2Borrow()
    {
        SetupCommon();

        // P2 không có điểm
        gameManager.UpdatePlayerScore(PlayerTurn.P2, sub: gameManager.P2Score);

        // P1 còn đủ điểm để cho mượn
        gameManager.UpdatePlayerScore(PlayerTurn.P1, add: 10);

        Debug.Log("🧪 Case 2: P2 sẽ mượn điểm của P1.");
        UpdateUI();
    }

    void Setup_Case3_EndGame()
    {
        SetupCommon();

        // Cả hai đều không có điểm
        gameManager.UpdatePlayerScore(PlayerTurn.P2, sub: gameManager.P2Score);
        gameManager.UpdatePlayerScore(PlayerTurn.P1, sub: gameManager.P1Score);

        Debug.Log("🧪 Case 3: Cả hai hết điểm. Kỳ vọng: Game Over.");
        UpdateUI();
    }

    void UpdateUI()
    {
        gameManager.UIControl.UpdateBoard(gameManager.BoardManager.board);
        gameManager.UIControl.UpdatePlayer(
            gameManager.P1Score,
            gameManager.P2Score,
            gameManager.P1StoneCount,
            gameManager.P2StoneCount,
            gameManager.P1Owe,
            gameManager.P1Owe
        );
    }
}
