using UnityEngine;
using System.Collections;
using DG.Tweening;

public class PosCube : MonoBehaviour
{
    public GameObject havePosCube;
	public float colorChangeSpeed = 10f;
	public bool isColorChanging = false;

	public IEnumerator ChangeColor()
	{
		isColorChanging = true;

		float alpha = 0;

		while (true)
		{
			yield return new WaitForFixedUpdate();

			alpha += colorChangeSpeed * Time.deltaTime;

			transform.GetChild(0).GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0, alpha);

			if (alpha > 0.39f)
			{
				alpha = 0.4f;
				transform.GetChild(0).GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0, alpha);
				break;
			}
		}

		while (true)
		{
			yield return new WaitForFixedUpdate();

			alpha -= colorChangeSpeed * Time.deltaTime;

			transform.GetChild(0).GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0, alpha);

			if (alpha < 0.01f)
			{
				alpha = 0f;
				transform.GetChild(0).GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0, alpha);
				break;
			}
		}

		isColorChanging = false;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.black;
		Gizmos.DrawCube(transform.position, 0.5f * Vector3.one);
	}
}
