using UnityEngine;
using UnityEngine.Pool;

public class CardSlotsUIViewCreator
{
    private readonly CardSlotUIView _prefab;
    private readonly ObjectPool<CardSlotUIView> _pool;
    private readonly RectTransform _root;

    public CardSlotsUIViewCreator(CardSlotUIView prefab, RectTransform root)
    {
        _prefab = prefab;
        _root = root;

        _pool = new ObjectPool<CardSlotUIView>(
            createFunc: CreateSlot,
            actionOnGet: OnGet,
            actionOnRelease: OnRelease,
            actionOnDestroy: OnDestroySlot,
            true
            );
    }

    public CardSlotUIView GetSlotView()
    {
        return _pool.Get();
    }

    public void ReleaseSlotView(CardSlotUIView view)
    {
        _pool.Release(view);
    }

    private CardSlotUIView CreateSlot()
    {
        return MonoBehaviour.Instantiate(_prefab, _root);
    }

    private void OnGet(CardSlotUIView view)
    {
        view.transform.SetAsLastSibling();
        view.gameObject.SetActive(true);
    }

    private void OnRelease(CardSlotUIView view)
    {
        view.gameObject.SetActive(false);
    }

    private void OnDestroySlot(CardSlotUIView view)
    {
        MonoBehaviour.Destroy(view.gameObject);
    }
}