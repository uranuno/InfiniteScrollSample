using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InfiniteScrollItemSample : MonoBehaviour, IInfiniteScrollItem
{
	[SerializeField]
	private RawImage[] m_Images = new RawImage[0];

	public int index { get; set; }

	[System.Serializable]
	public class UpdateItemEvent : UnityEvent<InfiniteScrollItemSample> { }

	public UpdateItemEvent OnUpdate = new UpdateItemEvent ();

	private string m_TextureName;

	private IEnumerator m_TextureUpdating;

	public void OnUpdateItem ()
	{
		OnUpdate.Invoke (this);
	}

	public void UpdateTexture (string textureName)
	{
		m_TextureName = textureName;

		// 前回の更新作業を止める
		if (m_TextureUpdating != null)
			StopCoroutine (m_TextureUpdating);

		m_TextureUpdating = UpdateTextureInternal ();
		StartCoroutine (m_TextureUpdating);
	}

	private IEnumerator UpdateTextureInternal ()
	{
		// テクスチャ空
		for (var i = 0; i < m_Images.Length; i++)
			m_Images[i].texture = null;

		// ロードが終わるのを待つ
		yield return TextureManager.LoadTexture (m_TextureName);

		// テクスチャ更新
		var texture = TextureManager.GetTexture (m_TextureName);
		for (var i = 0; i < m_Images.Length; i++)
			m_Images[i].texture = texture;
	}
}
