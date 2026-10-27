using UnityEngine;

public class Card
{
    private int _id;
    private string _name;
    private string _description;
    private Sprite _sprite;

    public Card(int id, string name, string description, Sprite sprite)
    {
        _id = id;
        _name = name;
        _description = description;
        _sprite = sprite;
    }

    public int Id => _id;
    public string Name => _name;
    public string Description => _description;
    public Sprite Sprite => _sprite;

    public void SetData(int id, string name, string description, Sprite sprite)
    {
        _id = id;
        _name = name;
        _description = description;
        _sprite = sprite;
    }

}
