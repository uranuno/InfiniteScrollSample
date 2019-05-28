
public class InfiniteScrollSampleLimited : InfiniteScrollSample
{
	protected override void OnUpdateItem (InfiniteScrollItemSample item)
	{
		var index = item.index;

		item.gameObject.SetActive (0 <= index && index < textureNames.Length);

		if (item.gameObject.activeSelf)
			item.UpdateTexture (textureNames[index]);
	}
}
