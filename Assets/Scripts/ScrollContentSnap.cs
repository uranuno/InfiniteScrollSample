using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 要素にスナップするScrollRect拡張
/// </summary>
[RequireComponent (typeof (ScrollRect))]
public class ScrollContentSnap : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	public float unit;

	public int limitedLength;

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

	private ScrollRect m_ScrollRect;

	private Vector2 m_BeginDragPos;
	private int m_BeginDragIndex;

	private IEnumerator m_Snapping;
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

		var childCount = 0;
		foreach (Transform child in m_Content)
		{
			if (child.gameObject.activeSelf)
				childCount++;
		}

		m_PrevButton.onClick.AddListener (() => {
			SnapNext (GetCurrentIndex (), -1);
		});
		m_NextButton.onClick.AddListener (() => {
			SnapNext (GetCurrentIndex (), 1);
		});

		m_ScrollRect = GetComponent<ScrollRect> ();
		m_ScrollRect.enabled = childCount > 1;

		SnapNext (GetCurrentIndex (), 0);
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

			SnapNext (m_BeginDragIndex, direction);
		}

		m_IsDragging = false;
	}

	private void SnapNext (int currentIndex, int direction)
	{
		// スナップ先のindex
		var targetIndex = currentIndex + direction;

		var limited = 0 < limitedLength;
		m_PrevButton.gameObject.SetActive (!limited || 0 < targetIndex);
		m_NextButton.gameObject.SetActive (!limited || targetIndex < limitedLength - 1);
		if (limited)
			targetIndex = Mathf.Clamp (targetIndex, 0, limitedLength - 1);

		StopSnapping ();

		m_Snapping = SnapTo (targetIndex);
		StartCoroutine (m_Snapping);
	}

	private IEnumerator SnapTo (int targetIndex)
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
		m_Snapping = null;
		yield break;
	}

	private void StopSnapping ()
	{
		if (m_Snapping != null)
		{
			StopCoroutine (m_Snapping);
			m_Snapping = null;
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
				SnapNext (GetCurrentIndex (), 1);
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
}
