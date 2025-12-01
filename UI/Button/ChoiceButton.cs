using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceButton : MonoBehaviour
{
    [SerializeField] private TMP_Text choiceText;
    private Button _button;

    private Choice _choice;       // 이 버튼이 담고 있는 선택지 데이터
    private IDialogueController _controller = null;

    private void Awake()    
    {
        _button = GetComponent<Button>(); 
        _button.onClick.AddListener(OnClick);
    }

    // Called by UIManager
    public void SetData(Choice choice, IDialogueController controller)
    {
        _choice = choice;
        _controller = controller;
        choiceText.text = choice.choiceText;
        gameObject.SetActive(true);
    }

    public void ResetData()
    {
        _choice = null;
        _controller = null;
        choiceText.text = "";
        gameObject.SetActive(false); // 버튼을 비활성화
    }

    private void OnClick()
    {
        // 자신을 보여달라고 요청했던 NPC에게 "이 선택지가 눌렸어!" 라고 다시 알려줌
        if (_controller != null)
        {
            _controller?.SelectChoice(_choice);
        }
    }
}
