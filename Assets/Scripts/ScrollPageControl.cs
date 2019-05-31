using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public interface IScrollPageItem
{
	int targetPageIndex { get; set; }
	void OnTargetPageChanged ();
}

/// <summary>
/// 要素にスナップするScrollRect拡張
/// </summary>
[RequireComponent (typeof (ScrollRect))]
public class ScrollPageControl : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	public int targetIndex { get; private set; }

	public float unit;

	public int length;

	public bool limited;

	public float dragThreshold;

	public float durationPerUnit;

	public AnimationCurve curve;

	public float autoTime;

	[SerializeField]
	private RectTransform m_Content;

	public RectTransform Content { get { return m_Content; } }

	[SerializeField]
	private Button m_PrevButton;
	[SerializeField]
	private Button m_NextButton;

	[SerializeField]
	private Toggle m_OriginalToggle;

	private Toggle[] m_Toggles = new Toggle[0];

	private IScrollPageItem[] m_PageItems;

	private ScrollRect m_ScrollRect;

	private Vector2 m_BeginDragPos;
	private int m_BeginDragIndex;

	private IEnumerator m_Scrolling;
	private bool m_IsDragging;

	private float m_Accum;

	private bool m_IsSetupped;

	void Start ()
	{
		Setup ();
	}

	public void Setup ()
	{
		if (m_IsSetupped)
			return;

		m_IsSetupped = true;

		if (m_OriginalToggle != null)
		{
			m_Toggles = new Toggle[length];
			for (var i = 0; i < length; i++)
			{
				var toggle = Instantiate<Toggle> (m_OriginalToggle, m_OriginalToggle.transform.parent);
				toggle.gameObject.SetActive (true);
				toggle.interactable = false;// ぽっちは押せません
				m_Toggles[i] = toggle;
			}
		}

		if (m_PrevButton != null)
		{
			m_PrevButton.onClick.AddListener (() => {
				ScrollToTargetIndex (GetCurrentIndex () - 1);
			});
		}
		if (m_NextButton != null)
		{
			m_NextButton.onClick.AddListener (() => {
				ScrollToTargetIndex (GetCurrentIndex () + 1);
			});
		}

		var childCount = 0;
		foreach (Transform child in m_Content)
		{
			if (child.gameObject.activeSelf)
				childCount++;
		}
		m_PageItems = m_Content.GetComponentsInChildren<IScrollPageItem> (true);

		m_ScrollRect = GetComponent<ScrollRect> ();
		m_ScrollRect.enabled = childCount > 1;

		SetTargetIndex (0);
	}

	public void OnBeginDrag (PointerEventData eventData)
	{
		m_IsDragging = true;
		m_BeginDragPos = eventData.position;
		m_BeginDragIndex = GetCurrentIndex ();
	}

	public void OnDrag (PointerEventData eventData) { }

	public void OnEndDrag (PointerEventData eventData)
	{
		// スクロールが有効のときのみドラッグ操作を受け付ける
		if (m_ScrollRect.enabled)
		{
			var diff = eventData.position - m_BeginDragPos;

			// ドラッグの方向を判定
			var direction = 0;
			if (Mathf.Abs (diff.x) > dragThreshold)// 閾値以上の場合のみ
				direction = (int)Mathf.Sign (-diff.x);

			ScrollToTargetIndex (m_BeginDragIndex + direction);
		}

		m_IsDragging = false;
	}

	public void ScrollToTargetIndex (int index)
	{
		targetIndex = limited ? Mathf.Clamp (index, 0, length - 1) : index;

		StopScrolling ();

		m_Scrolling = ScrollInternal ();
		StartCoroutine (m_Scrolling);

		OnTargetIndexChanged ();
	}

	private IEnumerator ScrollInternal ()
	{
		m_ScrollRect.enabled = false;

		var from = m_Content.anchoredPosition.x;
		var to = targetIndex * unit * -1;

		var duration = Mathf.Abs (to - from) / unit * durationPerUnit;
		var accum = 0f;
		while (accum < duration)
		{
			accum += Time.deltaTime;
			var t = accum / duration;
			var pos = new Vector2 (
				Mathf.LerpUnclamped (from, to, curve.Evaluate (t)),
				m_Content.anchoredPosition.y
			);
			m_Content.anchoredPosition = pos;
			yield return null;
		}

		m_Content.anchoredPosition = new Vector2 (
			to,
			m_Content.anchoredPosition.y
		);

		m_ScrollRect.enabled = true;
		m_Scrolling = null;
		yield break;
	}

	private void StopScrolling ()
	{
		if (m_Scrolling != null)
		{
			StopCoroutine (m_Scrolling);
			m_Scrolling = null;
		}
	}

	public void SetTargetIndex (int index)
	{
		targetIndex = limited ? Mathf.Clamp (index, 0, length - 1) : index;

		StopScrolling ();

		var to = targetIndex * unit * -1;
		m_Content.anchoredPosition = new Vector2 (
			to,
			m_Content.anchoredPosition.y
		);

		OnTargetIndexChanged ();
	}

	private void OnTargetIndexChanged ()
	{
		if (m_PrevButton != null)
			m_PrevButton.gameObject.SetActive (!limited || 0 < targetIndex);
		if (m_NextButton != null)
			m_NextButton.gameObject.SetActive (!limited || targetIndex < length - 1);

		var loopTargetIndex = GetLoopIndex (targetIndex, m_Toggles.Length);
		for (var i = 0; i < m_Toggles.Length; i++)
			m_Toggles[i].isOn = i == loopTargetIndex;

		for (var i = 0; i < m_PageItems.Length; i++)
		{
			m_PageItems[i].targetPageIndex = targetIndex;
			m_PageItems[i].OnTargetPageChanged ();
		}
	}

	// 現在のスクロール位置から何番目の要素が表示されているか計算
	private int GetCurrentIndex ()
	{
		var anchoredPosition = -m_Content.anchoredPosition.x;
		var index = Mathf.CeilToInt ((anchoredPosition - unit / 2f) / unit);
		return index;
	}

	private void Update ()
	{
		// 自動スライドが有効なとき、経過時間をカウントし指定時間を超えたら自動でスライドする。
		// 有効でなくなったら経過時間は強制的にリセットする。
		if (IsAutoEnalbed ())
		{
			m_Accum += Time.deltaTime;
			if (m_Accum > autoTime)
			{
				ScrollToTargetIndex (GetCurrentIndex () + 1);
				m_Accum = 0f;
			}
		}
		else
		{
			m_Accum = 0f;
		}
	}

	// 非アクティブ時もタイマーリセット
	private void OnDisable ()
	{
		m_Accum = 0f;
	}

	private bool IsAutoEnalbed ()
	{
		return
			autoTime > 0 //自動切り替え時間が設定されている
			&& m_ScrollRect != null && m_ScrollRect.enabled //スクロールが有効
			&& !m_IsDragging; //ドラッグ中でない
	}

	static public int GetLoopIndex (int index, int max)
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
