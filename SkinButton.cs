using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkinButton : MonoBehaviour
{
    public enum SkinButtonState
	{
		disabled,
		equipped,
		notEquipped
	}

	public bool isActivated = false;
	public int equipCount = 0; 
	public SkinButtonState state;
	public GameObject disabledObj;
	public GameObject equippedObj;
	public GameObject notEquippedObj;
	public GameObject buyBGBorder;

	private void Start()
	{
		CheckButton();
	}

	public void CheckButton()
	{
		switch (state)
		{
			case SkinButtonState.disabled:
				disabledObj.SetActive(true);
				equippedObj.SetActive(false);
				notEquippedObj.SetActive(false);
				GetComponent<Button>().interactable = false;
				break;
			case SkinButtonState.equipped:
				disabledObj.SetActive(false);
				equippedObj.SetActive(true);
				notEquippedObj.SetActive(false);
				GetComponent<Button>().interactable = true;
				break;
			case SkinButtonState.notEquipped:
				disabledObj.SetActive(false);
				equippedObj.SetActive(false);
				notEquippedObj.SetActive(true);
				GetComponent<Button>().interactable = true;
				break;
			default:
				break;
		}
	}
}
