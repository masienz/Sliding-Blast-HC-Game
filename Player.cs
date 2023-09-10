using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DG.Tweening;
using MoreMountains.NiceVibrations;

public class Player : MonoBehaviour
{
	[System.Serializable]
	public struct DestroyCube
	{
		public GameObject destroyObj;
		public float destroyTime;
	}

	public Manager manager;
	public int levelCount = 1;
	public float coinCount = 1;
	public int minCoinIncrease = 3;
	public int maxCoinIncrease = 10;
	public int minCoinIncreaseOnWin = 20;
	public int maxCoinIncreaseOnWin = 70;

	private int gainCoinOnEnd = 0;

	public bool isPlaying = false;

	[Header("Move")]
	public bool canTouch = true;
	public GameObject rotatingObj;
	public Rigidbody rb;
	[SerializeField] private float rotateSpeed = 10;
	public int moveCount;
	public bool canMove = true;
	public float moveWait = 0.5f;

	[Header("Skins")]
	public int equippedSkin = 0;
	public float skinTransitionTime = 1;
	public List<SkinButton> skinButtons;
	public List<float> skinBuyCounts;

	private float skinBuyCount = 0;

	[HideInInspector] public bool isBuying = false;

	[Header("Destroy")]
	public float destroyCubeWaitTime = 0.1f;
	public float touchWaitOnDestroy = 1;
	private bool canTouchOnDestroy = true;
	[HideInInspector] public float cannotTouchTime = 0;

	[Header("Combo")]
	public Slider comboSlider;
	public TextMeshProUGUI comboText;
	public float comboDecreaseSpeed = 1;

	private int comboCount = 1;

	[Header("Settings")]
	public Settings settings;
	public bool isVibrate = true;
	public bool isSoundEffect = true;

	[Header("Sounds")]
	public AudioSource winSound;
	public AudioSource loseSound;
	public AudioSource blastSound;
	public AudioSource coinSound;

	[Header("UI")]
	public float winPanelTime = 1.5f;
	public GameObject playPanel;
	public GameObject winPanel;
	public GameObject losePanel;
	public GameObject inGamePanel;
	public GameObject skinPanel;
	public GameObject buyButton;
	public TextMeshProUGUI coinTextOnWin;
	public UIMoneyEffect moneyEffect;
	public List<ParticleSystem> winEffects;
	public TextMeshProUGUI moveCountText;
	public TextMeshProUGUI levelCountText;
	public List<TextMeshProUGUI> coinCountTexts;
	public GameObject blastHandIcon;
	public GameObject rotateHandIcon;
	public float panelAnimDuration = 1;
	public Ease panelEase;

	private bool isMoveTouch;

	public List<DestroyCube> destroyCubes;

	private int moveCubeLayer;

	private Touch lastTouch;
	private Touch nullTouch;
	private int touchStationaryCount;

	[HideInInspector] public static Player instance { get; private set; }

	private void Awake()
	{
		if (instance == null) instance = this;
		moveCubeLayer = LayerMask.GetMask("MoveCube");
		levelCount = 1;
		skinButtons[0].isActivated = true;
		skinButtons[0].state = SkinButton.SkinButtonState.equipped;
		skinButtons[0].CheckButton();
		equippedSkin = 0;
		for (int j = 1; j < skinButtons.Count; j++)
		{
			skinButtons[j].buyBGBorder.SetActive(false);
			skinButtons[j].isActivated = false;
			skinButtons[j].CheckButton();
		}
		Load();
		nullTouch = new Touch();
		nullTouch.fingerId = 1061;
		lastTouch = nullTouch;
		comboCount = 1;

		canTouch = false;
		isPlaying = false;

		manager.moveCubePrefab = manager.moveCubesSkins[equippedSkin];

		manager.ResetLevel();
		StartCoroutine(manager.SpawnCube());
		levelCountText.text = levelCount.ToString();
	}

	private void Start()
	{
		playPanel.SetActive(true);
		winPanel.SetActive(false);
		losePanel.SetActive(false);
		skinPanel.SetActive(false);
		inGamePanel.SetActive(false);

		canTouchOnDestroy = true;
		destroyCubes = new List<DestroyCube>();
		levelCountText.text = levelCount.ToString();

		if (levelCount == 1)
		{
			blastHandIcon.SetActive(true);
			rotateHandIcon.SetActive(false);
		}
		else
		{
			blastHandIcon.SetActive(false);
			rotateHandIcon.SetActive(false);
		}

		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = 200;
	}

	public IEnumerator CanTouch()
	{
		canTouch = false;
		yield return new WaitForSeconds(0.5f);
		canTouch = true;
	}

	private void FixedUpdate()
	{
		if (cannotTouchTime > Time.time)
		{
			canTouchOnDestroy = false;
		}
		else canTouchOnDestroy = true;

		Combo();

		if (destroyCubes.Count != 0) DestroyCubes();

		if (Input.touchCount != 0 && canTouch)
		{
			var touch = Input.GetTouch(0);

			if (touch.phase == TouchPhase.Began)
			{
				isMoveTouch = false;
				touchStationaryCount = 0;
				lastTouch = touch;
			}

			if (touch.phase == TouchPhase.Stationary && !isMoveTouch)
			{
				touchStationaryCount++;
				lastTouch = touch;
			}

			if (touch.phase == TouchPhase.Moved && canMove)
			{
				if (canTouchOnDestroy)
				{
					rb.AddTorque(touch.deltaPosition.y * rotateSpeed, touch.deltaPosition.x * -rotateSpeed, 0);
					rotatingObj.transform.rotation = rb.transform.rotation;
					if (levelCount == 1 && rotateHandIcon.activeInHierarchy) rotateHandIcon.SetActive(false);
				}
				isMoveTouch = true;
			}

			if ((touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) && !isMoveTouch)
			{
				RaycastHit hit;

				if (Physics.Raycast(Camera.main.ScreenPointToRay(touch.position), out hit, 5000, moveCubeLayer))
				{
					if (hit.collider.GetComponent<MoveCube>())
					{
						if (!hit.collider.GetComponent<MoveCube>().isDead)
						{
							hit.collider.GetComponent<MoveCube>().CheckAroundMoveCubesSameMat(0);
							moveCount--;
							moveCountText.transform.DOShakeScale(0.5f, 0.5f, 5, 90);
							StartCoroutine(MoveWait());
							StartCoroutine(TouchWait());
							if (levelCount == 1 && blastHandIcon.activeInHierarchy)
							{
								blastHandIcon.SetActive(false);
								rotateHandIcon.SetActive(true);
							}
							AddCombo();
							manager.OnBlastedPlayer();
						}
					}
				}
				isMoveTouch = false;
				touchStationaryCount = 0;
				lastTouch = nullTouch;
			}
		}

		else if (canTouch && Input.touchCount == 0 && !isMoveTouch && touchStationaryCount < 15 && lastTouch.fingerId != nullTouch.fingerId)
		{
			RaycastHit hit;

			if (Physics.Raycast(Camera.main.ScreenPointToRay(lastTouch.position), out hit, 5000, moveCubeLayer))
			{
				if (hit.collider.GetComponent<MoveCube>())
				{
					if (!hit.collider.GetComponent<MoveCube>().isDead)
					{
						hit.collider.GetComponent<MoveCube>().CheckAroundMoveCubesSameMat(0);
						moveCount--;
						moveCountText.transform.DOShakeScale(0.5f, 0.5f, 5, 90);
						StartCoroutine(MoveWait());
						StartCoroutine(TouchWait());
						if (levelCount == 1 && blastHandIcon.activeInHierarchy)
						{
							blastHandIcon.SetActive(false);
							rotateHandIcon.SetActive(true);
						}
						AddCombo();
						manager.OnBlastedPlayer();
					}
				}
			}
			isMoveTouch = false;
			touchStationaryCount = 0;
			lastTouch = nullTouch;
		}

		else
		{
			isMoveTouch = false;
			lastTouch = nullTouch;
		}

		for (int i = 0; i < coinCountTexts.Count; i++)
		{
			coinCountTexts[i].text = ((int)coinCount).ToString();
		}

		moveCountText.text = moveCount.ToString() + " Moves";
	}

	private void DestroyCubes()
	{
		if (destroyCubes.Count > 5)
		{
			// Give +1 move
			moveCount++;
		}
		if (destroyCubes.Count > 0)
		{
			GenerateVibration();
			PlaySound(blastSound);
		}
		for (int i = destroyCubes.Count - 1; i > -1; i--)
		{
			destroyCubes[i].destroyObj.GetComponent<MoveCube>().NewDestroy();
			/* Mobil sistemde bozuk
			 * if(!destroyCubes[i].destroyObj.GetComponent<MoveCube>().targetPosCube.GetComponent<PosCube>().isColorChanging) 
				StartCoroutine(destroyCubes[i].destroyObj.GetComponent<MoveCube>().targetPosCube.GetComponent<PosCube>().ChangeColor());*/
			manager.moveCubes.Remove(destroyCubes[i].destroyObj);
			destroyCubes.RemoveAt(i);
		}
	}

	public void LevelReset()
	{
		Save();
		SceneManager.LoadScene("MainGame");
	}

	private IEnumerator MoveWait()
	{
		canMove = false;

		yield return new WaitForSeconds(moveWait);

		canMove = true;
	}

	public IEnumerator TouchWait()
	{
		canTouch = false;
		yield return new WaitForSeconds(0.5f);
		canTouch = true;
	}

	public void Save()
	{
		PlayerPrefs.SetInt("LevelCount", levelCount);
		PlayerPrefs.SetFloat("CoinCount", coinCount);
		PlayerPrefs.SetInt("EquSkinCount", equippedSkin);

		for (int i = 0; i < skinButtons.Count; i++)
		{
			PlayerPrefs.SetInt("SkinButton" + i.ToString(), skinButtons[i].isActivated ? 1 : 0);
		}

		if (isSoundEffect) PlayerPrefs.SetInt("Sound", 1);
		else PlayerPrefs.SetInt("Sound", 0);

		if (isVibrate) PlayerPrefs.SetInt("Vibrate", 1);
		else PlayerPrefs.SetInt("Vibrate", 0);
	}

	public void Load()
	{
		if(PlayerPrefs.HasKey("LevelCount")) levelCount = PlayerPrefs.GetInt("LevelCount");
		if (PlayerPrefs.HasKey("CoinCount")) coinCount = PlayerPrefs.GetFloat("CoinCount");
		if (PlayerPrefs.HasKey("EquSkinCount")) equippedSkin = PlayerPrefs.GetInt("EquSkinCount");

		if (PlayerPrefs.HasKey("SkinButton0"))
		{
			for (int i = 0; i < skinButtons.Count; i++)
			{
				skinButtons[i].isActivated = PlayerPrefs.GetInt("SkinButton" + i.ToString()) == 1 ? true : false;
				if (skinButtons[i].isActivated)
				{
					if(i == equippedSkin) skinButtons[i].state = SkinButton.SkinButtonState.equipped;
					else skinButtons[i].state = SkinButton.SkinButtonState.notEquipped;
				}
				else skinButtons[i].state = SkinButton.SkinButtonState.disabled;
			}
		}

		for (int j = 0; j < skinButtons.Count; j++)
		{
			skinButtons[j].buyBGBorder.SetActive(false);
			skinButtons[j].CheckButton();
		}

		CheckSkins();

		if (PlayerPrefs.HasKey("Vibrate"))
		{
			if (PlayerPrefs.GetInt("Vibrate") == 1)
			{
				isVibrate = true;
				settings.vibrate.GetComponent<Image>().sprite = settings.vibrateOn;
				settings.isOpenVibrate = true;
			}
			else
			{
				isVibrate = false;
				settings.vibrate.GetComponent<Image>().sprite = settings.vibrateOff;
				settings.isOpenVibrate = false;
			}
		}
		else
		{
			isVibrate = true;
			settings.vibrate.GetComponent<Image>().sprite = settings.vibrateOn;
			settings.isOpenVibrate = true;
		}

		if (PlayerPrefs.HasKey("Sound"))
		{
			if (PlayerPrefs.GetInt("Sound") == 1)
			{
				isSoundEffect = true;
				settings.sound.GetComponent<Image>().sprite = settings.soundOn;
				settings.isOpenSound = true;
			}
			else
			{
				isSoundEffect = false;
				settings.sound.GetComponent<Image>().sprite = settings.soundOff;
				settings.isOpenSound = false;
			}
		}
		else
		{
			isSoundEffect = true;
			settings.sound.GetComponent<Image>().sprite = settings.soundOn;
			settings.isOpenSound = true;
		}
	}

	[ContextMenu("ResetSave")]
	public void ResetSave()
	{
		PlayerPrefs.DeleteAll();
	}

	public IEnumerator Win()
	{
		yield return new WaitForSeconds(winPanelTime);

		levelCount++;

		gainCoinOnEnd = Random.Range(minCoinIncreaseOnWin, maxCoinIncreaseOnWin);

		for (int i = 0; i < moveCount; i++)
		{
			int coin = Random.Range(minCoinIncrease, maxCoinIncrease);
			gainCoinOnEnd += coin;
		}

		coinCount += gainCoinOnEnd;

		coinTextOnWin.text = "+" + gainCoinOnEnd.ToString();

		winPanel.transform.localScale = Vector3.one * 0.01f;

		winPanel.transform.DOScale(Vector3.one, panelAnimDuration).SetEase(panelEase);

		winPanel.SetActive(true);

		PlaySound(winSound);

		yield return new WaitForSeconds(panelAnimDuration);

		moneyEffect.Play(moneyEffect.moneys.Count - 1);

		PlaySound(coinSound);
	}

	public void EquipSkin(int skinNumber)
	{
		equippedSkin = skinNumber;

		for (int i = 0; i < skinButtons.Count; i++)
		{
			if (skinNumber == i) skinButtons[i].state = SkinButton.SkinButtonState.equipped;
			else if(skinButtons[i].isActivated) skinButtons[i].state = SkinButton.SkinButtonState.notEquipped;
			else if (!skinButtons[i].isActivated) skinButtons[i].state = SkinButton.SkinButtonState.disabled;

			skinButtons[i].CheckButton();
		}

		manager.moveCubePrefab = manager.moveCubesSkins[equippedSkin];
		manager.ChangeSpawnedCubesSkin();
	}
	
	public void PlayLevel()
	{
		Save();
		StartCoroutine(CanTouch());

		playPanel.SetActive(false);
		winPanel.SetActive(false);
		losePanel.SetActive(false);

		manager.moveCubePrefab = manager.moveCubesSkins[equippedSkin];

		manager.ResetLevel();
		StartCoroutine(manager.SpawnCube());
		levelCountText.text = levelCount.ToString();
	}

	public void MainMenuPlay()
	{
		canTouch = true;
		playPanel.SetActive(false);
		winPanel.SetActive(false);
		losePanel.SetActive(false);
		inGamePanel.SetActive(true);
		isPlaying = true;
	}

	public void OpenOrCloseSkinPanel()
	{
		if (skinPanel.activeInHierarchy) skinPanel.SetActive(false);
		else
		{
			skinPanel.SetActive(true);
			CheckSkins();
		}
	}

	public void BuySkinFunc()
	{
		if (skinBuyCount > coinCount || isBuying) return;
		StartCoroutine(BuySkin());
	}

	private IEnumerator BuySkin()
	{
		isBuying = true;
		List<SkinButton> closedButtons = new List<SkinButton>();
		SkinButton selected = null;

		for (int i = 0; i < skinButtons.Count; i++)
		{
			if (skinButtons[i].state == SkinButton.SkinButtonState.disabled) closedButtons.Add(skinButtons[i]);
		}

		for (int i = 0; i < 6; i++)
		{
			yield return new WaitForSeconds(skinTransitionTime);

			for (int j = 0; j < closedButtons.Count; j++)
			{
				closedButtons[j].buyBGBorder.SetActive(false);
			}

			int a = Random.Range(0, closedButtons.Count);

			closedButtons[a].buyBGBorder.SetActive(true);
			if (i == 5) selected = closedButtons[a];
		}

		selected.state = SkinButton.SkinButtonState.equipped;
		selected.isActivated = true;

		selected.CheckButton();
		coinCount -= skinBuyCount;
		selected.buyBGBorder.SetActive(false);
		CheckSkins();

		EquipSkin(selected.equipCount);

		Save();
		isBuying = false;
	}

	public void CheckSkins()
	{
		List<SkinButton> closedButtons = new List<SkinButton>();

		for (int i = 0; i < skinButtons.Count; i++)
		{
			if (skinButtons[i].state == SkinButton.SkinButtonState.disabled) closedButtons.Add(skinButtons[i]);
		}

		if (closedButtons.Count != 0)
		{
			skinBuyCount = skinBuyCounts[skinBuyCounts.Count - closedButtons.Count];
			buyButton.GetComponentInChildren<TextMeshProUGUI>().text = skinBuyCount.ToString();
		}
		else
		{
			buyButton.GetComponentInChildren<TextMeshProUGUI>().text = "Purchased All";
		}
	}

	private void Combo()
	{
		comboText.text = $"{comboCount}x";
		if (comboCount > 1)
		{
			comboSlider.value -= Time.deltaTime * comboDecreaseSpeed * comboCount;
			if (comboSlider.value <= 0)
			{
				comboSlider.value = 1;
				comboCount = 1;
			}
		}
	}

	public void AddCombo()
	{
		comboSlider.value = 1;
		comboCount++;
	}

	public void ResetCombo()
	{
		comboSlider.value = 1;
		comboCount = 1;
	}

	public void GenerateVibration()
	{
		if (!isVibrate) return;

		MMVibrationManager.StopAllHaptics();

		MMVibrationManager.TransientHaptic(0.85f, 0.05f, true, this);
	}

	public void PlaySound(AudioSource audioSource)
	{
		if (!isSoundEffect) return;

		audioSource.Stop();
		audioSource.Play();
	}

	private void OnApplicationQuit()
	{
		Save();
	}
}
