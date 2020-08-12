using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Toolbar : MonoBehaviour {

	public UIItemSlot[] slots;
	public RectTransform highlight;
	public int slotIndex = 0;

	// Start is called before the first frame update
	void Start () {

		byte index = 1;
		foreach (UIItemSlot s in slots) {
			ItemStack stack = new ItemStack ("glass", 64);
			ItemSlot slot = new ItemSlot (slots[index - 1], stack);
			index++;
		}

	}

	// Update is called once per frame
	void Update () {
		float scroll = Input.GetAxis ("Mouse ScrollWheel");

		if (scroll != 0) {

			if (scroll > 0)
				slotIndex--;
			else
				slotIndex++;

			if (slotIndex > slots.Length - 1)
				slotIndex = 0;
			if (slotIndex < 0)
				slotIndex = slots.Length - 1;

			highlight.position = slots[slotIndex].slotIcon.transform.position;

		}
	}
}
