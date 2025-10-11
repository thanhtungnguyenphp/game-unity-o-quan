using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum PlayerTurn
{
    P1,
    P2
}
public enum States
{
    SelectingCell,
    SelectingDirection,
    Animating,
    GameOver
}


public class BoardManager : MonoBehaviour
{
    public int[] board = new int[12];

    public GameObject _prefabDanA;
    public GameObject _prefabDanB;
    public GameObject _prefabQuan;

    bool _quan1;
    bool _quan2;
    public bool Quan1Available => _quan1;
    public bool Quan2Available => _quan2;
    public void Initialize()
    {
        //Transform Quan = transform.Find("Quan");
        // Transform Dan1 = transform.Find("Dan1");
        // Transform Dan2 = transform.Find("Dan2");
        //int useDaA = 0;

        // foreach (Transform child in Quan)
        // {
        //     Transform da = child.transform.Find("item").Find("da");
        //     Transform quan = child.transform.Find("item").Find("quan");
        //     Instantiate(_prefabQuan, quan);
        //     for (int i = 0; i < 5; i++)
        //         Instantiate(useDaA == 0 ? _prefabDanA : _prefabDanB);
        //     useDaA++;
        // }

        // foreach (Transform child in Dan1)
        // {
        //     for (int i = 0; i < 5; i++)
        //         Instantiate(_prefabDanA, child.transform.Find("item"));
        // }
        // foreach (Transform child in Dan2)
        // {
        //     for (int i = 0; i < 5; i++)
        //         Instantiate(_prefabDanB, child.transform.Find("item"));
        // }

        for (int i = 0; i < 12; i++)
            board[i] = 0;
        for (int i = 0; i < 5; i++)
            board[i] = 5;
        for (int i = 6; i <= 10; i++)
            board[i] = 5;
        board[5] = board[11] = 1;
        _quan1 = _quan2 = true;

    }

    public bool IsPlayerCell(int index, PlayerTurn turn)
    {
        return (turn == PlayerTurn.P1 && index >= 0 && index <= 4) ||
               (turn == PlayerTurn.P2 && index >= 6 && index <= 10);
    }

    public bool IsQuan(int idx) => idx == 5 || idx == 11;

    public int GetPointValue(int idx, int count)
        => IsQuan(idx) ?
        count * API._masterData.piece.quan.score :
        count * API._masterData.piece.dan.score;

    /// <summary>
    /// Đưa board về trạng thái ban đầu và render lại UI.
    /// </summary>
    public void ResetBoard()
    {
        Initialize();
    }

    public void EatStone(int idx)
    {
        if (idx == 5 && _quan1)
            _quan1 = false;
        else if (idx == 11 && _quan2)
            _quan2 = false;
        board[idx] = 0;
    }

    public bool IsQuanAvailable() => _quan1 || _quan2;
    public int GetPoint(int idx)
    {
        if (idx == 5 && _quan1 || idx == 11 && _quan2)
            return API._masterData.piece.quan.score + ((board[idx] - 1) * API._masterData.piece.dan.score);
        else
            return board[idx] * API._masterData.piece.dan.score;    
    }

}
