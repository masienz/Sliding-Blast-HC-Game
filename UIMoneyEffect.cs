using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIMoneyEffect : MonoBehaviour
{
	[System.Serializable]
    public struct Money
	{
		public RectTransform money;
		public Tween tween;
	}

	public Player player;
	public List<Money> moneys;
	public RectTransform targetMoney;
	private Vector2 startPos;
	public float duration = 0.5f;

	private void Awake()
	{
		startPos = moneys[0].money.anchoredPosition;
	}

	public void Play(int moneyCount)
	{
		int count = 0;

		if (moneyCount > moneys.Count) count = moneys.Count;
		else count = moneyCount;

		for (int i = 0; i < count; i++)
		{
			Money money = new Money();
			money.money = moneys[i].money;
			money.money.gameObject.SetActive(true);
			money.tween = moneys[i].money.DOAnchorPos(targetMoney.anchoredPosition, duration);
			Sequence seq = DOTween.Sequence();
			seq.Append(money.tween);
			seq.PrependInterval(0.1f * i);
			//if (character.isSoundEffect) money.money.GetComponent<AudioSource>().PlayDelayed(0.1f * i);
			StartCoroutine(isEndMoney(money, duration + 0.1f * i));
			moneys[i] = money;
			if(player.moveCount > 0) player.moveCount--;
		}
	}

	private IEnumerator isEndMoney(Money money, float duration)
	{
		yield return new WaitForSeconds(duration);

		money.money.gameObject.SetActive(false);
		money.money.anchoredPosition = startPos;
	}
}
