using TMPro;
using UnityEngine;

public class CardSlotUIView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _idText;
    [SerializeField] TextMeshProUGUI _nameText;
    [SerializeField] TextMeshProUGUI _descriptionText;

    public void SetInfo(string id, string name, string description)
    {
        _idText.text = id;
        _nameText.text = name;
        _descriptionText.text = description;
    }
}
