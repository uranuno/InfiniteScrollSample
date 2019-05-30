using System.Collections.Generic;
using UnityEngine;

public interface IInfiniteScrollItem
{
	int index { get; set; }
	void UpdateItem ();
}

public class InfiniteScroll : MonoBehaviour
{
	public int instantateItemCount;

	public float offset;

	[SerializeField]
	private RectTransform m_OriginalItem;

	[SerializeField]
	private RectTransform m_Content;

	// 生成済みの要素
	private LinkedList<RectTransform> m_Items = new LinkedList<RectTransform> ();

	// 生成済みの要素についているコンポーネントのキャッシュ
	private Dictionary<RectTransform, IInfiniteScrollItem> m_ItemComponents = new Dictionary<RectTransform, IInfiniteScrollItem> ();

	private float m_FilledSize;

	private int m_CurrentItemIndex;

	private bool m_IsSetupped = false;

	// Use this for initialization
	void Start ()
	{
		Setup ();
	}

	public void Setup ()
	{
		if (m_IsSetupped)
			return;

		m_IsSetupped = true;

		var itemSize = m_OriginalItem.sizeDelta.x;
		var parent = m_OriginalItem.parent;
		// 指定の数だけ生成
		for (int i = 0; i < instantateItemCount; i++)
		{
			var item = Instantiate<RectTransform> (m_OriginalItem);
			item.SetParent (parent, false);
			item.name = "item " + i;
			item.gameObject.SetActive (true);

			item.anchoredPosition = new Vector2 (itemSize * i, 0);

			m_Items.AddLast (item);

			var itemComponent = item.GetComponent<IInfiniteScrollItem> ();
			if (itemComponent != null)
			{
				m_ItemComponents[item] = itemComponent;
				UpdateItem (i, item);
			}
		}
	}

	void Update ()
	{
		RectTransform itemUpdated;
		float itemPos;

		var itemSize = m_OriginalItem.sizeDelta.x;
		var anchoredPosition = m_Content.anchoredPosition.x;

		// 正の方向にスクロールした時、スクロール量が1アイテムを超えた分だけ後ろに補填する
		while (anchoredPosition + m_FilledSize < -itemSize - offset)
		{
			m_FilledSize += itemSize;

			itemUpdated = m_Items.First.Value;
			m_Items.RemoveFirst ();
			m_Items.AddLast (itemUpdated);

			itemPos = (m_CurrentItemIndex + instantateItemCount) * itemSize;
			itemUpdated.anchoredPosition = new Vector2 (itemPos, 0);

			UpdateItem (m_CurrentItemIndex + instantateItemCount, itemUpdated);

			m_CurrentItemIndex++;
		}

		// 負の方向にスクロールした時、補填分が余るようになったら補填分を先頭に戻す
		while (anchoredPosition + m_FilledSize > -offset)
		{
			m_FilledSize -= itemSize;

			itemUpdated = m_Items.Last.Value;
			m_Items.RemoveLast ();
			m_Items.AddFirst (itemUpdated);

			m_CurrentItemIndex--;

			itemPos = m_CurrentItemIndex * itemSize;
			itemUpdated.anchoredPosition = new Vector2 (itemPos, 0);

			UpdateItem (m_CurrentItemIndex, itemUpdated);
		}
	}

	private void UpdateItem (int index, RectTransform item)
	{
		IInfiniteScrollItem itemComponent;
		if (m_ItemComponents.TryGetValue (item, out itemComponent))
		{
			itemComponent.index = index;
			itemComponent.UpdateItem ();
		}
	}
}
