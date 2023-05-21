using System;
using System.Collections.Generic;
using UnityEngine;

public enum GameStatus
{
    Invalid,
    Normal, // 一般状态
    GamePoint, // 赛点
    GameOver, // 分出胜负
    Draw, // 平局
}

[Flags]
public enum Player
{
    None = 0,
    P1 = 1,
    P2 = 2,
    Either = P1 | P2
}

public struct ChessStateInfo
{
    /// <summary>
    /// 当前对局状态
    /// </summary>
    public GameStatus GameStatus;

    /// <summary>
    /// 当前胜者或领先者
    /// </summary>
    public Player LeadingPlayer;

    /// <summary>
    /// 决定当前对局状态的格子编号:
    /// Normal-未被占领的格子;
    /// GamePoint-距离达成胜利条件仅剩的格子;
    /// GameOver-胜利者占领的连成一条线的3个格子;
    /// </summary>
    public int[] KeyIndices;

#if UNITY_EDITOR
    public override string ToString()
    {
        var indices = "[";
        if (KeyIndices != null)
        {
            foreach (var index in KeyIndices)
            {
                indices += index + ",";
            }
        }

        indices += "]";
        return $"当前棋盘状态{GameStatus},领先玩家{LeadingPlayer},关键格子{indices}";
    }
#endif
}

public class Strategy
{
    /// <summary>
    /// 井字棋的所有棋盘状态
    /// </summary>
    private const int NumOfStates = 19683;

    /// <summary>
    /// 棋盘的格子数
    /// </summary>
    public const int NumOfChecks = 9;

    public static readonly int[] Pow = { 1, 3, 9, 27, 81, 243, 729, 2187, 6561 };

    /// <summary>
    /// 所有的胜利情况对应的需要占领的3个格子
    /// </summary>
    private static readonly int[][] WinConditions =
    {
        new[] { 0, 1, 2 },
        new[] { 3, 4, 5 },
        new[] { 6, 7, 8 },
        new[] { 0, 3, 6 },
        new[] { 1, 4, 7 },
        new[] { 2, 5, 8 },
        new[] { 0, 4, 8 },
        new[] { 2, 4, 6 },
    };

    private static ChessStateInfo[] _chessStateInfos;

    // O(1)获取任意棋谱的信息
    public static ChessStateInfo GetStateInfo(int state)
    {
        return _chessStateInfos[state];
    }

    private static Strategy _instance = new();

    public static Strategy Instance => _instance;

    // 实例化时生成全部棋谱信息,只需计算一次所以就不优化算法了
    private Strategy()
    {
        Debug.Log("哔哔哔————开始生成策略");
        _chessStateInfos = new ChessStateInfo[NumOfStates];

        // 分析每种棋盘状态的对应信息,不可能存在的状态不作专门判断,任由其归类
        for (var state = 0; state < NumOfStates; ++state)
        {
            var currInfo = new ChessStateInfo
            {
                GameStatus = GameStatus.Invalid,
                LeadingPlayer = Player.None,
                KeyIndices = null,
            };

            // 首先判断是否分出胜负:任意一方达成胜利条件
            foreach (var condition in WinConditions)
            {
                var occupier0 = GetOccupier(state, condition[0]);
                var occupier1 = GetOccupier(state, condition[1]);
                var occupier2 = GetOccupier(state, condition[2]);
                if (occupier0 == Player.P1 && occupier1 == Player.P1 && occupier2 == Player.P1)
                {
                    currInfo.GameStatus = GameStatus.GameOver;
                    currInfo.LeadingPlayer = Player.P1;
                    currInfo.KeyIndices = condition;
                    break;
                }

                if (occupier0 == Player.P2 && occupier1 == Player.P2 && occupier2 == Player.P2)
                {
                    currInfo.GameStatus = GameStatus.GameOver;
                    currInfo.LeadingPlayer = Player.P2;
                    currInfo.KeyIndices = condition;
                    break;
                }
            }

            // 是否平局:所有格子都被占领了
            if (currInfo.GameStatus == GameStatus.Invalid)
            {
                for (var i = 0; i < NumOfChecks; i++)
                {
                    if (GetOccupier(state, i) == Player.None)
                        break;
                    if (i == NumOfChecks - 1)
                        currInfo.GameStatus = GameStatus.Draw;
                }
            }

            // 是否赛点:任意一方距离胜利条件还差1个空白格子
            if (currInfo.GameStatus == GameStatus.Invalid)
            {
                var lastIndices = new HashSet<int>();
                foreach (var condition in WinConditions)
                {
                    var occupier0 = GetOccupier(state, condition[0]);
                    var occupier1 = GetOccupier(state, condition[1]);
                    var occupier2 = GetOccupier(state, condition[2]);
                    var occupiers = (occupier0, occupier1, occupier2);
                    if (occupiers == (Player.None, Player.P1, Player.P1))
                    {
                        currInfo.LeadingPlayer |= Player.P1;
                        currInfo.GameStatus = GameStatus.GamePoint;
                        lastIndices.Add(condition[0]);
                    }
                    else if (occupiers == (Player.P1, Player.None, Player.P1))
                    {
                        currInfo.LeadingPlayer |= Player.P1;
                        currInfo.GameStatus = GameStatus.GamePoint;
                        lastIndices.Add(condition[1]);
                    }
                    else if (occupiers == (Player.P1, Player.P1, Player.None))
                    {
                        currInfo.LeadingPlayer |= Player.P1;
                        currInfo.GameStatus = GameStatus.GamePoint;
                        lastIndices.Add(condition[2]);
                    }
                    else if (occupiers == (Player.None, Player.P2, Player.P2))
                    {
                        currInfo.LeadingPlayer |= Player.P2;
                        currInfo.GameStatus = GameStatus.GamePoint;
                        lastIndices.Add(condition[0]);
                    }
                    else if (occupiers == (Player.P2, Player.None, Player.P2))
                    {
                        currInfo.LeadingPlayer |= Player.P2;
                        currInfo.GameStatus = GameStatus.GamePoint;
                        lastIndices.Add(condition[1]);
                    }
                    else if (occupiers == (Player.P2, Player.P2, Player.None))
                    {
                        currInfo.LeadingPlayer |= Player.P2;
                        currInfo.GameStatus = GameStatus.GamePoint;
                        lastIndices.Add(condition[2]);
                    }
                }

                if (currInfo.GameStatus == GameStatus.GamePoint)
                {
                    var keyIndices = new int[lastIndices.Count];
                    var i = 0;
                    foreach (var index in lastIndices)
                    {
                        keyIndices[i] = index;
                        i++;
                    }

                    currInfo.KeyIndices = keyIndices;
                }
            }

            // 以上都不符合,定义为普通状态,并统计空格子
            if (currInfo.GameStatus == GameStatus.Invalid)
            {
                currInfo.GameStatus = GameStatus.Normal;
                var indices = new List<int>();
                for (var i = 0; i < NumOfChecks; i++)
                {
                    if (GetOccupier(state, i) == Player.None)
                    {
                        indices.Add(i);
                    }
                }

                currInfo.KeyIndices = indices.ToArray();
            }

            _chessStateInfos[state] = currInfo;
        }
    }

    private static Player GetOccupier(int chessState, int boxIndex)
    {
        if (boxIndex < 0 || boxIndex > 8 || chessState < 0 || chessState >= NumOfStates)
        {
            throw new ArgumentException();
        }

        var player = chessState / Pow[boxIndex] % 3;
        return player switch
        {
            0 => Player.None,
            1 => Player.P1,
            2 => Player.P2,
            _ => throw new Exception("不可能出现的情况")
        };
    }
}