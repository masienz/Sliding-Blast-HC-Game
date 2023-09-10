using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cinemachine;
using System.Collections;

public class Manager : MonoBehaviour
{
    public enum Mechanics
    {
        inGameSpawnCube,
        stoneCube,
        randomChangeColorOnBlast,
    }

	public Player player;
    public CinemachineTargetGroup targetGroup;

    [Header("Spawn Settings")]
    [SerializeField] private GameObject moveCubesParentInScene;
    [SerializeField] private GameObject posCubesParentInScene;
    [SerializeField] private GameObject posCubePrefab;
    public GameObject moveCubePrefab;
	public GameObject changeColorObj;
    public List<GameObject> moveCubesSkins;
    [SerializeField] private int minXCount = 3;
    [SerializeField] private int maxXCount = 3;
    [SerializeField] private int minYCount = 3;
    [SerializeField] private int maxYCount = 3;
    [SerializeField] private int minZCount = 3;
    [SerializeField] private int maxZCount = 3;
    [SerializeField] private float cubeDistance = 0.5f;
    [SerializeField] private float moveCubesUpdateTimeInterval = 0.05f;
    public List<Material> mats;
	public Material stoneCubeMat;
	public List<ParticleSystem> effects;

    private List<GameObject> posCubes = new List<GameObject>();
    public List<GameObject> moveCubes = new List<GameObject>();
	private List<GameObject> stoneCubes = new List<GameObject>();

	private bool isEnd = false;
    public bool isStarting = false;

    private Mechanics currentMechanic;
    private int mechBlastCount = 0;

	private void Awake()
    {
        //List<GameObject> outCubes = new List<GameObject>();
        isStarting = false;
        isEnd = false;
    }

	private void Update()
	{
        if (isEnd) return;
        if (!isStarting) return;

        CheckCubes();
    }

	public IEnumerator SpawnCube()
	{
        if (player.levelCount > 5)
        {
            currentMechanic = (Mechanics)Random.Range(0, 2);
		}

        posCubes = new List<GameObject>();
        moveCubes = new List<GameObject>();
        int xCount = Random.Range(minXCount, maxXCount);
        int yCount = Random.Range(minYCount, maxYCount);
        int zCount = Random.Range(minZCount, maxZCount);

        if (player.levelCount == 1)
		{
            xCount = 2;
            yCount = 2;
            zCount = 2;
        }

        Vector3 pos = new Vector3((xCount / 2) * cubeDistance, (yCount / 2) * cubeDistance, (zCount / 2) * cubeDistance);

        #region Pos Cubes

        List<GameObject> posObjs = new List<GameObject>();

        //Pos Cubes Spawn
        for (int i = 0; i < xCount; i++)
        {
            for (int j = 0; j < yCount; j++)
            {
                for (int k = 0; k < zCount; k++)
                {
                    GameObject spawned = Instantiate(posCubePrefab, new Vector3(i * cubeDistance, j * cubeDistance, k * cubeDistance) - pos, Quaternion.identity);
                    //if (i == 0 || j == 0 || k == 0 || i == xCount - 1 || j == yCount - 1 || k == zCount - 1) outCubes.Add(spawned);

                    posCubes.Add(spawned);
                    posObjs.Add(spawned);
                }
            }
        }
        #endregion

        #region Move Cubes
        int interval = 1;
        List<GameObject> moveObjs = new List<GameObject>();

        List<int> stoneCubesCount = new List<int>();

        if (currentMechanic == Mechanics.stoneCube)
        {
            for (int i = 0; i < 5; i++)
            {
				stoneCubesCount.Add(Random.Range(0, (xCount * yCount * zCount) - 1));
            }
        }

		//Move Cubes Spawn
		for (int i = 0; i < xCount; i++)
        {
            for (int j = 0; j < yCount; j++)
            {
                for (int k = 0; k < zCount; k++)
                {
                    GameObject spawned = Instantiate(moveCubePrefab, new Vector3(i * cubeDistance, j * cubeDistance, k * cubeDistance) - pos, Quaternion.identity);
					spawned.GetComponent<MoveCube>().startTime = moveCubesUpdateTimeInterval * interval;
					interval++;

                    //if (i == 0 || j == 0 || k == 0 || i == xCount - 1 || j == yCount - 1 || k == zCount - 1) outCubes.Add(spawned);

                    moveCubes.Add(spawned);
                    moveObjs.Add(spawned);
                }
            }
        }

        //Position calculate
        Vector3 origin = Vector3.zero;
        Vector3 all = Vector3.zero;

		for (int i = 0; i < moveObjs.Count; i++)
		{
            all += moveCubes[i].transform.position;
        }

        origin = all / moveCubes.Count;

        moveCubesParentInScene.transform.position = origin;
        posCubesParentInScene.transform.position = origin;
        player.rotatingObj.transform.position = origin;

		for (int i = 0; i < moveObjs.Count; i++)
		{
			moveObjs[i].transform.parent = moveCubesParentInScene.transform;
        }

        for (int i = 0; i < posObjs.Count; i++)
        {
            posObjs[i].transform.parent = posCubesParentInScene.transform;
        }

        player.rb.transform.position = player.rotatingObj.transform.position;

        //2 obj paint same mat per mat
        for (int i = 0; i < mats.Count; i++)
		{
			for (int j = 0; j < 2; j++)
			{
                int obj = Random.Range(0, moveObjs.Count);
                moveObjs[obj].GetComponent<MeshRenderer>().material = mats[i];
                moveObjs[obj].GetComponent<MoveCube>().matCount = i;
                moveObjs[obj].GetComponent<MoveCube>().destroyEffect = effects[i];
                moveObjs[obj].GetComponent<MoveCube>().openedTrail = i;
            }
		}

        //Remaining obj paint
        for (int i = 0; i < moveObjs.Count; i++)
		{
            MoveCube moveCube = moveObjs[i].GetComponent<MoveCube>();

            int mat = Random.Range(0, mats.Count);
            moveObjs[i].GetComponent<MeshRenderer>().material = mats[mat];
            moveCube.matCount = mat;
            moveCube.destroyEffect = effects[mat];

            moveCube.openedTrail = mat;
        }
		#endregion

		for (int i = 0; i < posCubes.Count; i++)
		{
            posCubes[i].GetComponent<PosCube>().havePosCube = moveCubes[i];
        }

		for (int i = 0; i < posCubes.Count; i++)
		{
            moveCubes[i].GetComponent<MoveCube>().targetPosCube = posCubes[i].GetComponent<PosCube>();
            moveCubes[i].GetComponent<MoveCube>().player = player;
        }

        //Move cube scale animation
        for (int i = 0; i < moveCubes.Count; i++)
        {
            moveCubes[i].GetComponent<MoveCube>().scaleTime = i * 0.01f;
        }

        //Set player move count
        List<int> sortByList = new List<int>();

        sortByList.Add(xCount);
        sortByList.Add(yCount);
        sortByList.Add(zCount);

        sortByList.Sort();

        player.moveCount = sortByList[2] + sortByList[0];
        
        //Add move to player first 3 level
        if (player.levelCount < 4) player.moveCount += 4 - player.levelCount;

        //Stone cubes
        if (currentMechanic == Mechanics.stoneCube)
        {
			for (int i = 0; i < stoneCubesCount.Count; i++)
			{
                moveCubes[stoneCubesCount[i]].GetComponent<MoveCube>().isDead = true;
                moveCubes[stoneCubesCount[i]].GetComponent<MoveCube>().isCheckedMat = true;
				moveCubes[stoneCubesCount[i]].GetComponent<MoveCube>().CloseTrails();
				moveCubes[stoneCubesCount[i]].GetComponent<MoveCube>().enabled = false;
                moveCubes[stoneCubesCount[i]].GetComponent<MeshRenderer>().material = stoneCubeMat;
				stoneCubes.Add(moveCubes[stoneCubesCount[i]]);
				moveCubes.Remove(moveCubes[stoneCubesCount[i]]);
			}
		}
        
        yield return new WaitForSeconds(moveCubes.Count * 0.01f);
        
        isStarting = true;
    }

    public void ChangeSpawnedCubesSkin()
    {
        for (int i = moveCubes.Count - 1; i > -1; i--)
        {
			GameObject spawned = Instantiate(moveCubePrefab, moveCubes[i].transform.position, moveCubes[i].transform.rotation);
            MoveCube beforeCube = moveCubes[i].GetComponent<MoveCube>();
            MoveCube currentCube = spawned.GetComponent<MoveCube>();
            spawned.GetComponent<MoveCube>().startTime = beforeCube.startTime;
            spawned.transform.parent = moveCubesParentInScene.transform;

			currentCube.GetComponent<MeshRenderer>().material = mats[beforeCube.matCount];
			currentCube.matCount = beforeCube.matCount;
			currentCube.destroyEffect = beforeCube.destroyEffect;
			currentCube.openedTrail = beforeCube.openedTrail;
			currentCube.targetPosCube = beforeCube.targetPosCube;
            currentCube.targetPosCube.havePosCube = currentCube.gameObject;
			currentCube.player = player;
			currentCube.scaleTime = beforeCube.scaleTime;

			moveCubes.Add(spawned);
            moveCubes.Remove(beforeCube.gameObject);
            Destroy(beforeCube.gameObject);
		}
	}

    public void ResetLevel()
    {
        for (int i = 0; i < posCubes.Count; i++)
        {
            Destroy(posCubes[i]);
		}

		for (int i = 0; i < moveCubes.Count; i++)
		{
			Destroy(moveCubes[i]);
		}

        posCubes = new List<GameObject>();
		moveCubes = new List<GameObject>();
        isEnd = false;
        isStarting = true;
	}

    private void CheckCubes()
	{
        if (moveCubes.Count == 0)
        {
            StartCoroutine(player.Win());
            player.canTouch = false;

			for (int i = 0; i < posCubes.Count; i++)
			{
                posCubes[i].SetActive(false);
			}

			if (stoneCubes.Count > 0)
            {
				for (int i = 0; i < stoneCubes.Count; i++)
				{
					stoneCubes[i].SetActive(false);
				}
			}
            
			for (int i = 0; i < player.winEffects.Count; i++)
			{
                player.winEffects[i].Play();
			}

            isEnd = true;
        }
        if (player.moveCount == 0 && moveCubes.Count > 0)
		{
            player.canTouch = false;
            StartCoroutine(CheckLastCubes());
        }
    }

    private IEnumerator CheckLastCubes()
    {
        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < moveCubes.Count; i++)
        {
            if (!moveCubes[i].GetComponent<MoveCube>().isDead)
            {
				player.losePanel.transform.localScale = Vector3.one * 0.01f;
				player.losePanel.transform.DOScale(Vector3.one, player.panelAnimDuration).SetEase(player.panelEase);
				player.losePanel.SetActive(true);
				player.canTouch = false;
                player.PlaySound(player.loseSound);
				isEnd = true;
				break;
            }
        }
    }

    public void OnBlastedPlayer()
    {
        if (currentMechanic == Mechanics.inGameSpawnCube && mechBlastCount % 5 == 2 && moveCubes.Count > 6)
        {
            GameObject spawnPos = null;
            List<GameObject> emptyPosCubes = new List<GameObject>();

            for (int i = 0; i < posCubes.Count; i++)
            {
				if (posCubes[i].GetComponent<PosCube>().havePosCube == null)
				{
                    emptyPosCubes.Add(posCubes[i]);
				}
			}

            while (spawnPos == null)
            {
                int spawnCount = Random.Range(0, emptyPosCubes.Count);

                if (emptyPosCubes[spawnCount].GetComponent<PosCube>().havePosCube == null)
                {
					spawnPos = emptyPosCubes[spawnCount];
				}
            }

			GameObject spawned = Instantiate(moveCubePrefab, spawnPos.transform.position, spawnPos.transform.rotation);

            MoveCube moveCube = spawned.GetComponent<MoveCube>();

            int mat = Random.Range(0, mats.Count);
			spawned.GetComponent<MeshRenderer>().material = mats[mat];
			moveCube.matCount = mat;
			moveCube.destroyEffect = effects[mat];
			moveCube.openedTrail = mat;
			moveCube.targetPosCube = spawnPos.GetComponent<PosCube>();
			moveCube.player = player;
			spawnPos.GetComponent<PosCube>().havePosCube = spawned;
			spawned.transform.parent = moveCubesParentInScene.transform;
            moveCube.scaleTime = 0;

            moveCubes.Add(spawned);
		}
        else if (currentMechanic == Mechanics.randomChangeColorOnBlast && mechBlastCount % 5 == 2 && moveCubes.Count > 6)
        {
            Instantiate(changeColorObj, moveCubesParentInScene.transform.position, Quaternion.identity);
		}
		mechBlastCount++;
	}
}
