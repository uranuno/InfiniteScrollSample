using UnityEngine;

public class InfiniteScrollSample : MonoBehaviour
{
	public string[] textureNames = new string[0];

	[SerializeField]
	private InfiniteScroll m_InfiniteScroll;

	[SerializeField]
	private ScrollContentSnap m_ScrollRectSnap;

	// Use this for initialization
	private void Start ()
	{
		m_InfiniteScroll.Setup ();

		var items = m_InfiniteScroll.GetComponentsInChildren<InfiniteScrollItemSample> ();
		for (var i = 0; i < items.Length; i++)
		{
			items[i].OnUpdateItem.AddListener (OnUpdateItem);
			items[i].UpdateItem ();
		}

		m_InfiniteScroll.enabled = true;
		m_ScrollRectSnap.enabled = true;
	}

	protected virtual void OnUpdateItem (InfiniteScrollItemSample item)
	{
		var index = GetLoopIndex (item.index, textureNames.Length);
		item.UpdateTexture (textureNames[index]);
	}

	private static int GetLoopIndex (int index, int max)
	{
		if (max == 0)
		{
			Debug.LogError ("ゼロで割れません！！");
			return -1;
		}

		var loopIndex = index % max;
		if (loopIndex < 0)
			loopIndex += max;
		return loopIndex;
	}
}
