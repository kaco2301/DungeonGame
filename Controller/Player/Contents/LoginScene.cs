using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginScene : MonoBehaviour
{
    public TMP_InputField nicknameInput;

    public void OnConfirmNickname()
    {
        // 입력값 저장
        string inputName = nicknameInput.text;
        if (string.IsNullOrEmpty(inputName))
            inputName = "플레이어"; // 기본값

        PlayerData.Instance.SetPlayerName(inputName);


        // 다음 씬으로 이동
        SceneManager.LoadScene("GameScene");
    }
}
