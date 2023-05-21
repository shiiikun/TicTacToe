using UnityEngine;

public class PlayButton : MonoBehaviour
{
    private void OnMouseUpAsButton()
    {
        GameManager.Instance.ResetGame();
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
}