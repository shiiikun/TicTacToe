using UnityEngine;

public class GameLauncher : MonoBehaviour
{
    
    public GameObject check;

    public GameObject player1;

    public GameObject player2;

    public GameObject playButton;

    private const float PositionOffset = 6f;

    private void Start()
    {
        var gameManager = GameManager.Instance;

        for (var i = 0; i < 9; i++)
        {
            var newBox = Instantiate(check);
            newBox.transform.position = new Vector3((i % 3 - 1) * PositionOffset, - (i / 3 - 1) * PositionOffset + 2f);
            var comp = newBox.AddComponent<CheckComponent>();
            comp.myIndex = i;
            gameManager.CheckComponents[i] = comp;
        }

        var p1 = Instantiate(player1);
        p1.transform.position = new Vector3(-15f, -5f);
        gameManager.Player1 = p1.AddComponent<Character>();
        var p2 = Instantiate(player2);
        p2.transform.position = new Vector3(15f, -5f);
        gameManager.Player2 = p2.AddComponent<Character>();

        var button = Instantiate(playButton);
        button.transform.position = new Vector3(0, -10f);
        gameManager.PlayButton = button.AddComponent<PlayButton>();
    }
}