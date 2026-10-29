using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckSlot : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameLabel;
    [SerializeField] private Button _button;

    public Button Button => _button;

    public void SetLabelText(string text)
    {
        _nameLabel.text = text;
    }

}
