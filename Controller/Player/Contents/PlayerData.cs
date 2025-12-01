using TMPro;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance { get; private set; }

    public string Nickname { get; set; } = "Player"; // 기본값

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 이동해도 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void GetPlayerName(TMP_Text text)
    {
        text.text = Nickname;
    }

    public void SetPlayerName(string name)
    {
        Nickname = name;
    }
}
