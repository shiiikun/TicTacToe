using UnityEngine;

/// <summary>
/// 棋盘格子组件,用来响应鼠标事件或运行AI
/// </summary>
public class CheckComponent : MonoBehaviour
{
    private GameObject _halo;

    private GameObject _icon1;
    private GameObject _icon2;
    private bool IsOccupied { get; set; }

    public int myIndex;

    /// <summary>
    /// 玩家操作间隔
    /// </summary>
    private const float SwitchPlayerInterval = 1f;

    private void Start()
    {
        _halo = transform.Find("Halo").gameObject;
        _halo.SetActive(false);
        _icon1 = transform.Find("Icon1").gameObject;
        _icon2 = transform.Find("Icon2").gameObject;
        SetOccupier(Player.None);
    }

    public void SetOccupier(Player player)
    {
        IsOccupied = player != Player.None;
        _icon1.SetActive(player == Player.P1);
        _icon2.SetActive(player == Player.P2);
    }

    public void SetHalo(bool active)
    {
        _halo.SetActive(active);
    }

    private void OnMouseEnter()
    {
        if (IsOccupied || GameManager.Instance.Paused)
            return;

        _halo.SetActive(true);
    }

    private void OnMouseExit()
    {
        if (IsOccupied || GameManager.Instance.Paused)
            return;

        _halo.SetActive(false);
    }

    private void OnMouseUpAsButton()
    {
        if (IsOccupied || GameManager.Instance.Paused)
            return;
        
        Click();
    }

    /// <summary>
    /// 点击
    /// </summary>
    public void Click()
    {
        _halo.SetActive(false);
        GameManager.Instance.OnCheckClicked(myIndex);
        Invoke(nameof(ContinueGame), SwitchPlayerInterval);
    }

    private void ContinueGame()
    {
        GameManager.Instance.ContinueGame();
    }
}