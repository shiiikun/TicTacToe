using UnityEngine;

public class GameManager
{
    public readonly CheckComponent[] CheckComponents;

    public Character Player1 { get; set; }

    public Character Player2 { get; set; }

    public PlayButton PlayButton { get; set; }


    private const bool UseAi = true;
    private int _chessState;
    private Player _currentPlayer;
    private int _player1Score;
    private int _player2Score;

    public bool Paused { get; private set; }

    private static GameManager _instance = new();

    public static GameManager Instance => _instance;

    private GameManager()
    {
        CheckComponents = new CheckComponent[9];
        Paused = true;
    }

    public void OnCheckClicked(int checkIndex)
    {
        if (_currentPlayer == Player.None || Paused)
            return;

        // 更新棋盘状态
        _chessState += Strategy.Pow[checkIndex] * (int)_currentPlayer;
        CheckComponents[checkIndex].SetOccupier(_currentPlayer);
        Paused = true;
    }

    private void SwitchPlayer()
    {
        _currentPlayer = Player.Either & ~ _currentPlayer;
        Player1.SetPointer(_currentPlayer == Player.P1);
        Player2.SetPointer(_currentPlayer == Player.P2);
    }

    public void ContinueGame()
    {
        var currInfo = Strategy.GetStateInfo(_chessState);
        Debug.Log(currInfo);
        // 首先检查游戏结束条件
        switch (currInfo.GameStatus)
        {
            case GameStatus.Draw:
            {
                PlayButton.SetActive(true);
                Player1.SetPointer(false);
                Player2.SetPointer(false);
                Player1.SetWinTag(false);
                Player2.SetWinTag(false);
                return;
            }
            case GameStatus.GameOver:
            {
                PlayButton.SetActive(true);
                Player1.SetPointer(false);
                Player2.SetPointer(false);
                foreach (var index in currInfo.KeyIndices)
                {
                    CheckComponents[index].SetHalo(true);
                }

                if (currInfo.LeadingPlayer == Player.P1)
                {
                    _player1Score++;
                    Player1.SetScore(_player1Score);
                    Player1.SetWinTag(true);
                    Player2.SetWinTag(false);
                }
                else if (currInfo.LeadingPlayer == Player.P2)
                {
                    _player2Score++;
                    Player2.SetScore(_player2Score);
                    Player2.SetWinTag(true);
                    Player1.SetWinTag(false);
                }

                return;
            }
            default:
            {
                SwitchPlayer();
                Paused = false;
                break;
            }
        }

        // 本轮是否由AI托管
        if (_currentPlayer == Player.P2 && UseAi)
        {
            AiMove();
        }
    }

    // P2由机器人托管时的操作逻辑
    private void AiMove()
    {
        var currInfo = Strategy.GetStateInfo(_chessState);

        // 没到赛点,则空格子里随便选一个
        if (currInfo.GameStatus == GameStatus.Normal)
        {
            var suggestedIndices = currInfo.KeyIndices;
            var randomIndex = suggestedIndices[Random.Range(0, suggestedIndices.Length)];
            var check = CheckComponents[randomIndex];
            check.Click();
            Debug.Log("没到赛点,则空格子里随便选一个");
        }

        // 到赛点了
        else if (currInfo.GameStatus == GameStatus.GamePoint)
        {
            // 所有赛点格子中随机选一个
            var suggestedIndices = currInfo.KeyIndices;

            // 双方都是赛点,选己方的格子
            if (currInfo.LeadingPlayer == Player.Either)
            {
                // 遇到对方的赛点格就跳过
                foreach (var index in suggestedIndices)
                {
                    var nextState = _chessState + Strategy.Pow[index] * (int)Player.P2;
                    if (Strategy.GetStateInfo(nextState).LeadingPlayer != Player.P2)
                        continue;

                    var check = CheckComponents[index];
                    check.Click();
                    Debug.Log("双方都是赛点,选己方的格子");
                    return;
                }
            }
            // 只有一方是赛点,随便选一个
            else
            {
                var randomIndex = suggestedIndices[Random.Range(0, suggestedIndices.Length)];
                var check = CheckComponents[randomIndex];
                check.Click();
                Debug.Log("只有一方是赛点,随便选一个");
            }
        }
    }

    public void ResetGame()
    {
        _chessState = 0;
        _currentPlayer = Random.Range(0, 2) == 0 ? Player.P1 : Player.P2;
        SwitchPlayer();
        foreach (var check in CheckComponents)
        {
            check.SetOccupier(Player.None);
            check.SetHalo(false);
        }

        if (_player1Score == 10 || _player2Score == 10)
        {
            _player1Score = 0;
            _player2Score = 0;
            Player1.SetScore(0);
            Player2.SetScore(0);
        }

        PlayButton.SetActive(false);
        ContinueGame();
    }
}