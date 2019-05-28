using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureManager : MonoBehaviour
{
	static private readonly Dictionary<string, Texture2D> m_LoadedTextures = new Dictionary<string, Texture2D> ();

	static private readonly HashSet<string> m_LoadingTextureNames = new HashSet<string> ();

	static private TextureManager m_Instance;

	private void Awake ()
	{
		m_Instance = this;
	}

	static public IEnumerator LoadTexture (string textureName)
	{
		// テクスチャロード済み
		var texture = GetTexture (textureName);
		if (texture != null)
			yield break;

		// テクスチャロード開始していなければ開始
		if (!m_LoadingTextureNames.Contains (textureName))
			m_Instance.StartCoroutine (LoadTextureInternal (textureName));

		// テクスチャのロードが終わるのを待つ
		while (GetTexture (textureName) == null)
			yield return null;
	}

	// 新しくテクスチャをロードする
	static private IEnumerator LoadTextureInternal (string textureName)
	{
		m_LoadingTextureNames.Add (textureName);

		var req = Resources.LoadAsync<Texture2D> (textureName);
		yield return req;

		m_LoadedTextures[textureName] = req.asset as Texture2D;
		m_LoadingTextureNames.Remove (textureName);
	}

	// テクスチャを取得する
	static public Texture2D GetTexture (string textureName)
	{
		Texture2D texture;
		m_LoadedTextures.TryGetValue (textureName, out texture);
		return texture;
	}
}
