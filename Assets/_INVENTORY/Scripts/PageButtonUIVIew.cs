using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PageButtonUIVIew : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _labelText;
    [SerializeField] private Image _image;
    [SerializeField] private Color _selectedColor;
    [SerializeField] private Button _button;
    private Color _unselectedColor = Color.white;


    public Button Button => _button;

    public void SetLabel(string text)
    {
        _labelText.text = text;
    }

    public void MarkAsSelected()
    {
        _image.color = _selectedColor;
    }

    public void MarkAsUnselected()
    {
        _image.color = _unselectedColor;
    }
}
