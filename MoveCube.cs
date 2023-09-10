using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MoveCube : MonoBehaviour
{
	public bool isDead = false;
	public bool isCheckedMat = false;
	public bool canChange = true;
	public float moveLerp = 1;

	[HideInInspector] public Player player;
	public PosCube targetPosCube;
	[HideInInspector] public float startTime = 0;
	public float updateTime = 2;
	public float destroyTime = 1;
	public Ease destroyEase;
	[HideInInspector] public int matCount = 0;
	[HideInInspector] public int openedTrail = 0;
	public List<TrailRenderer> trails;
	public Ease scaleEase;

	[HideInInspector] public Material thisMat;
	private MeshRenderer meshRenderer;
	private float halfSizeX = 0;
	private int posCubeLayer;
	private int moveCubeLayer;
	private float distanceToTarget = 0;

	private PosCube behindObj = null;
	private PosCube rightObj = null;
	private PosCube leftObj = null;
	private PosCube forwardObj = null;
	private PosCube backObj = null;

	[HideInInspector] public float destroyTimetime = 0;
	[HideInInspector] public ParticleSystem destroyEffect;

	private bool isDestroyStarted = false;

	private Vector3 targetScale;
	private Vector3 startScale;
	[HideInInspector] public float scaleTime = 50000000;

	private void Start()
	{
		thisMat = transform.GetComponent<MeshRenderer>().material;
		meshRenderer = transform.GetComponent<MeshRenderer>();
		halfSizeX = meshRenderer.bounds.size.x * 0.5f;
		posCubeLayer = LayerMask.GetMask("PosCube");
		moveCubeLayer = LayerMask.GetMask("MoveCube");

		targetScale = transform.localScale;
		startScale = targetScale * 0.01f;
		transform.localScale = startScale;

		//InvokeRepeating("UpdateByQueue", startTime, updateTime);
		Invoke("StartScale", scaleTime);

		destroyTimetime = 0;

		CloseTrails();
	}

	private void StartScale()
	{
		transform.DOScale(targetScale, 0.5f).SetEase(scaleEase);
	}

	private void Update()
	{
		if (!player.isPlaying) return;
		UpdateByQueue();
		if (targetPosCube == null) return;
		transform.position = Vector3.Lerp(transform.position, targetPosCube.transform.position, Time.deltaTime * moveLerp);

		float dis = Vector3.Distance(transform.position, targetPosCube.transform.position);

		if (dis > 0.1f) canChange = false;
		else
		{
			canChange = true;
			CloseTrails();
		}
	}

	private void UpdateByQueue()
	{
		/*if (destroyTimetime != 0 && !isDestroyStarted)
		{
			if (Time.time > destroyTimetime) Destroying();
		}*/

		if (isDead) return;

		CheckAroundRaycast();
	}

	public void CheckAroundRaycast()
	{
		//Raycast down
		RaycastHit[] rayInfos = Physics.RaycastAll(transform.position, -Vector3.up, halfSizeX * 2, posCubeLayer);
		RaycastHit[] rayInfos2 = Physics.RaycastAll(transform.position + halfSizeX * Vector3.right - 0.2f * Vector3.right, -Vector3.up, halfSizeX * 2, posCubeLayer);
		RaycastHit[] rayInfos3 = Physics.RaycastAll(transform.position - halfSizeX * Vector3.right + 0.2f * Vector3.right, -Vector3.up, halfSizeX * 2, posCubeLayer);
		RaycastHit[] rayInfos4 = Physics.RaycastAll(transform.position + halfSizeX * Vector3.forward - 0.2f * Vector3.forward, -Vector3.up, halfSizeX * 2, posCubeLayer);
		RaycastHit[] rayInfos5 = Physics.RaycastAll(transform.position - halfSizeX * Vector3.forward + 0.2f * Vector3.forward, -Vector3.up, halfSizeX * 2, posCubeLayer);

		//Check objects around
		behindObj = null;
		rightObj = null;
		leftObj = null;
		forwardObj = null;
		backObj = null;

		foreach (var ray in rayInfos)
		{
			if (ray.collider.gameObject != gameObject)
			{
				behindObj = ray.collider.GetComponent<PosCube>();
				break;
			}
			//else Debug.Log("Kendisini gördü");
		}

		foreach (var ray in rayInfos2)
		{
			if (behindObj != null)
			{
				if (ray.collider.gameObject != gameObject && ray.collider.gameObject != behindObj.gameObject)
				{
					rightObj = ray.collider.GetComponent<PosCube>();
					break;
				}
			}
			else if (ray.collider.gameObject != gameObject)
			{
				rightObj = ray.collider.GetComponent<PosCube>();
				break;
			}
		}

		foreach (var ray in rayInfos3)
		{
			if (behindObj != null)
			{
				if (ray.collider.gameObject != gameObject && ray.collider.gameObject != behindObj.gameObject)
				{
					leftObj = ray.collider.GetComponent<PosCube>();
					break;
				}
			}
			else if (ray.collider.gameObject != gameObject)
			{
				leftObj = ray.collider.GetComponent<PosCube>();
				break;
			}
		}

		foreach (var ray in rayInfos4)
		{
			if (behindObj != null)
			{
				if (ray.collider.gameObject != gameObject && ray.collider.gameObject != behindObj.gameObject)
				{
					forwardObj = ray.collider.GetComponent<PosCube>();
					break;
				}
			}
			else if (ray.collider.gameObject != gameObject)
			{
				forwardObj = ray.collider.GetComponent<PosCube>();
				break;
			}
		}

		foreach (var ray in rayInfos5)
		{
			if (behindObj != null)
			{
				if (ray.collider.gameObject != gameObject && ray.collider.gameObject != behindObj.gameObject)
				{
					backObj = ray.collider.GetComponent<PosCube>();
					break;
				}
			}
			else if (ray.collider.gameObject != gameObject)
			{
				backObj = ray.collider.GetComponent<PosCube>();
				break;
			}
		}

		//Fall Down
		Conditions();
	}

	public void Conditions()
	{
		if (behindObj != null && behindObj.havePosCube == null)
		{
			if (rightObj == null && leftObj == null && backObj == null && forwardObj == null)
			{
				ChangePos(behindObj);
				return;
			}
		}

		if (behindObj != null && rightObj != null && rightObj == behindObj)
		{
			ChangePos(rightObj);
			return;
		}

		if (behindObj != null && leftObj != null && leftObj == behindObj)
		{
			ChangePos(leftObj);
			return;
		}

		if (behindObj != null && backObj != null && backObj == behindObj)
		{
			ChangePos(backObj);
			return;
		}

		if (behindObj != null && forwardObj != null && forwardObj == behindObj)
		{
			ChangePos(forwardObj);
			return;
		}

		if (rightObj != null && rightObj.havePosCube == null)
		{
			ChangePos(rightObj);
			return;
		}

		if (leftObj != null && leftObj.havePosCube == null)
		{
			ChangePos(leftObj);
			return;
		}

		if (backObj != null && backObj.havePosCube == null)
		{
			ChangePos(backObj);
			return;
		}

		if (forwardObj != null && forwardObj.havePosCube == null)
		{
			ChangePos(forwardObj);
			return;
		}
	}

	public void ChangePos(PosCube posCube)
	{
		if (!canChange) return;
		if (posCube.havePosCube != null) return;

		if (targetPosCube != null) targetPosCube.havePosCube = null;
		targetPosCube = posCube;
		posCube.havePosCube = gameObject;
		OpenTrails();
	}

	public void CheckAroundMoveCubesSameMat(float time)
	{
		if (isCheckedMat) return;

		List<GameObject> sameMatObjs = new List<GameObject>();
		RaycastHit[] rayInfos = Physics.RaycastAll(transform.position, transform.up, halfSizeX * 2, moveCubeLayer);
		RaycastHit[] rayInfos2 = Physics.RaycastAll(transform.position, transform.up * -1, halfSizeX * 2, moveCubeLayer);
		RaycastHit[] rayInfos3 = Physics.RaycastAll(transform.position, transform.forward, halfSizeX * 2, moveCubeLayer);
		RaycastHit[] rayInfos4 = Physics.RaycastAll(transform.position, transform.forward * -1, halfSizeX * 2, moveCubeLayer);
		RaycastHit[] rayInfos5 = Physics.RaycastAll(transform.position, transform.right, halfSizeX * 2, moveCubeLayer);
		RaycastHit[] rayInfos6 = Physics.RaycastAll(transform.position, transform.right * -1, halfSizeX * 2, moveCubeLayer);

		foreach (var item in rayInfos)
		{
			if (item.collider.gameObject != gameObject && item.collider.GetComponent<MoveCube>())
			{
				if (item.collider.GetComponent<MoveCube>().matCount == matCount && 
					!item.collider.GetComponent<MoveCube>().isCheckedMat) sameMatObjs.Add(item.collider.gameObject);
			}
		}

		foreach (var item in rayInfos2)
		{
			if (item.collider.gameObject != gameObject && item.collider.GetComponent<MoveCube>())
			{
				if (item.collider.GetComponent<MoveCube>().matCount == matCount && 
					!item.collider.GetComponent<MoveCube>().isCheckedMat) sameMatObjs.Add(item.collider.gameObject);
			}
		}

		foreach (var item in rayInfos3)
		{
			if (item.collider.gameObject != gameObject && item.collider.GetComponent<MoveCube>())
			{
				if (item.collider.GetComponent<MoveCube>().matCount == matCount && 
					!item.collider.GetComponent<MoveCube>().isCheckedMat) sameMatObjs.Add(item.collider.gameObject);
			}
		}

		foreach (var item in rayInfos4)
		{
			if (item.collider.gameObject != gameObject && item.collider.GetComponent<MoveCube>())
			{
				if (item.collider.GetComponent<MoveCube>().matCount == matCount && 
					!item.collider.GetComponent<MoveCube>().isCheckedMat) sameMatObjs.Add(item.collider.gameObject);
			}
		}

		foreach (var item in rayInfos5)
		{
			if (item.collider.gameObject != gameObject && item.collider.GetComponent<MoveCube>())
			{
				if (item.collider.GetComponent<MoveCube>().matCount == matCount && 
					!item.collider.GetComponent<MoveCube>().isCheckedMat) sameMatObjs.Add(item.collider.gameObject);
			}
		}

		foreach (var item in rayInfos6)
		{
			if (item.collider.gameObject != gameObject && item.collider.GetComponent<MoveCube>())
			{
				if (item.collider.GetComponent<MoveCube>().matCount == matCount && 
					!item.collider.GetComponent<MoveCube>().isCheckedMat) sameMatObjs.Add(item.collider.gameObject);
			}
		}

		Player.DestroyCube destroyCube = new Player.DestroyCube();

		destroyCube.destroyObj = gameObject;
		destroyCube.destroyTime = time;

		player.destroyCubes.Add(destroyCube);

		isCheckedMat = true;

		for (int i = 0; i < sameMatObjs.Count; i++)
		{
			sameMatObjs[i].GetComponent<MoveCube>().CheckAroundMoveCubesSameMat(time + player.destroyCubeWaitTime);
		}
	}

	public void DestroyCube(float time)
	{
		destroyTimetime = Time.time + time;
		player.cannotTouchTime = Time.time + player.touchWaitOnDestroy;
		isDead = true;
	}

	public void Destroying()
	{
		isDestroyStarted = true;
		Tween tween = transform.DOScale(transform.localScale * 0.01f, destroyTime).SetEase(destroyEase);
		Instantiate(destroyEffect, transform.position, Quaternion.identity);
		Destroy(gameObject, destroyTime + 0.05f);
	}

	public void NewDestroy()
	{
		Instantiate(destroyEffect, transform.position, Quaternion.identity);
		Destroy(gameObject);
	}

	private void OpenTrails()
	{
		trails[openedTrail].gameObject.SetActive(true);
	}

	public void CloseTrails()
	{
		for (int i = 0; i < trails.Count; i++)
		{
			trails[i].gameObject.SetActive(false);
		}
	}

	public IEnumerator ChangeColorScaleEffect()
	{
		transform.DOScale(targetScale * 0.7f, 0.5f);

		yield return new WaitForSeconds(0.7f);

		transform.DOScale(targetScale, 0.5f);
	}

	private void OnTriggerEnter(Collider col)
	{
		if (col.CompareTag("ChangeColor"))
		{
			Debug.Log("Girdi");
			MoveCube moveCube = this;
			int matCount = (moveCube.matCount + 1) % moveCube.player.manager.mats.Count;
			Material mat = moveCube.player.manager.mats[matCount];
			moveCube.matCount = matCount;
			moveCube.GetComponent<MeshRenderer>().material = mat;
			moveCube.thisMat = moveCube.GetComponent<MeshRenderer>().material;
			moveCube.destroyEffect = moveCube.player.manager.effects[matCount];
			moveCube.openedTrail = matCount;
			moveCube.CloseTrails();
			StartCoroutine(moveCube.ChangeColorScaleEffect());
		}
	}

	/*private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;

		/*Gizmos.DrawLine(transform.position, transform.position + transform.right * -10);

		Gizmos.DrawLine(transform.position, transform.position + transform.right * 10);

		Gizmos.DrawLine(transform.position, transform.position + transform.forward * -10);

		Gizmos.DrawLine(transform.position, transform.position + transform.forward * 10);

		float distance = halfSizeX * 2;

		Vector3 startPos;

		startPos = transform.position + halfSizeX * Vector3.right - 0.2f * Vector3.right;

		Gizmos.DrawLine(startPos, startPos + Vector3.up * -distance);

		startPos = transform.position - halfSizeX * Vector3.right + 0.2f * Vector3.right;

		Gizmos.DrawLine(startPos, startPos + Vector3.up * -distance);

		startPos = transform.position - halfSizeX * Vector3.forward + 0.2f * Vector3.forward;

		Gizmos.DrawLine(startPos, startPos + Vector3.up * -distance);

		startPos = transform.position + halfSizeX * Vector3.forward - 0.2f * Vector3.forward;

		Gizmos.DrawLine(startPos, startPos + Vector3.up * -distance);

		startPos = transform.position;

		Gizmos.DrawLine(startPos, startPos + Vector3.up * -distance);
	}*/
}
