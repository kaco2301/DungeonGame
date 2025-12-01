using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_HUD : UI_Base
{
    [SerializeField] Slider _hpSlider;
    [SerializeField] Slider _mpSlider;

    [SerializeField] private HealthEventChannelSO _healthChannel;
    [SerializeField] private ManaEventChannelSO _manaChannel;

    protected enum Texts
    {
        HPText,
        ManaText,
    }

    private TMP_Text HPText => Get<TMP_Text>((int)Texts.HPText);
    private TMP_Text ManaText => Get<TMP_Text>((int)Texts.ManaText);

    public override void Init()
    {
        Bind<TMP_Text>(typeof(Texts));
    }

    private void OnEnable()
    {
        _healthChannel.Register(UpdateHealthUI);
        _manaChannel.Register(UpdateManaUI);
    }

    private void OnDisable()
    {
        _healthChannel.Unregister(UpdateHealthUI);
        _manaChannel.Unregister(UpdateManaUI);
    }

    private void UpdateHealthUI(HealthData data)
    {
        if (_hpSlider != null)
        {
            float value = (data.Max > 0) ? data.Current / data.Max : 0;
            _hpSlider.value = value;

            HPText.text = $"{data.Current:0}/{data.Max:0}";
        }
    }

    private void UpdateManaUI(ManaData data)
    {
        if (_hpSlider != null)
        {
            float value = (data.Max > 0) ? data.Current / data.Max : 0;
            _mpSlider.value = value;

            ManaText.text = $"{data.Current:0}/{data.Max:0}";
        }
    }


}
