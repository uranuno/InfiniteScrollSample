﻿using System.Collections;
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

	public UpdateItemEvent OnUpdateItem = new UpdateItemEvent ();

	private string m_TextureName;

	private IEnumerator m_Updating;

	public void UpdateItem ()
	{
		OnUpdateItem.Invoke (this);
	}

	public void UpdateTexture (string textureName)
	{
		// 更新なし
		if (m_TextureName == textureName)
			return;

		// 前回の更新作業を止める
		if (m_Updating != null)
			StopCoroutine (m_Updating);

		// 更新開始
		m_TextureName = textureName;

		m_Updating = UpdateTextureInternal ();
		StartCoroutine (m_Updating);
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
