using UnityEngine;

public class InfiniteScrollSample : MonoBehaviour
{
	public string[] textureNames = new string[0];

	[SerializeField]
	private InfiniteScroll m_InfiniteScroll;

	[SerializeField]
	private ScrollPageControl m_ScrollPageControl;

	// Use this for initialization
	private void Start ()
	{
		m_InfiniteScroll.Setup ();

		var items = m_InfiniteScroll.GetComponentsInChildren<InfiniteScrollItemSample> ();
		for (var i = 0; i < items.Length; i++)
		{
			items[i].OnUpdate.AddListener (OnUpdateItem);
			items[i].OnUpdateItem ();
		}

		m_InfiniteScroll.enabled = true;
		m_ScrollPageControl.enabled = true;
	}

	protected virtual void OnUpdateItem (InfiniteScrollItemSample item)
	{
		var index = ScrollPageControl.GetLoopIndex (item.index, textureNames.Length);
		item.UpdateTexture (textureNames[index]);
	}
}
