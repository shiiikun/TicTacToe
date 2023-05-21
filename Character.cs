using UnityEngine;

public class Character : MonoBehaviour
{
    private GameObject _winTag;
    private GameObject _pointer;
    private GameObject[] _digitIcons;
    private GameObject _currDigitIcon;
    
    private void Start()
    {
        _winTag = transform.Find("Win").gameObject;
        _pointer = transform.Find("Point").gameObject;
        _digitIcons = new GameObject[11];
        _digitIcons[0] = transform.Find("0").gameObject;
        _digitIcons[1] = transform.Find("1").gameObject;
        _digitIcons[2] = transform.Find("2").gameObject;
        _digitIcons[3] = transform.Find("3").gameObject;
        _digitIcons[4] = transform.Find("4").gameObject;
        _digitIcons[5] = transform.Find("5").gameObject;
        _digitIcons[6] = transform.Find("6").gameObject;
        _digitIcons[7] = transform.Find("7").gameObject;
        _digitIcons[8] = transform.Find("8").gameObject;
        _digitIcons[9] = transform.Find("9").gameObject;
        _digitIcons[10] = transform.Find("10").gameObject;
        _currDigitIcon = _digitIcons[0];
        _currDigitIcon.SetActive(true);
    }

    public void SetPointer(bool active)
    {
        _pointer.SetActive(active);
    }

    public void SetWinTag(bool win)
    {
        _winTag.SetActive(win);
    }

    public void SetScore(int digit)
    {
        _currDigitIcon.SetActive(false);
        _currDigitIcon = _digitIcons[digit];
        _currDigitIcon.SetActive(true);
    }
}
