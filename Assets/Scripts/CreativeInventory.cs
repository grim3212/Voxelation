using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreativeInventory : MonoBehaviour {
	public GameObject slotPrefab;
	World world;
	List<ItemSlot> slots = new List<ItemSlot> ();

	// Start is called before the first frame update
	void Start () {
		world = GameObject.Find ("World").GetComponent<World> ();


		foreach (KeyValuePair<string, Block> entry in BlockRegistry.Blocks) {
			GameObject newSlot = Instantiate (slotPrefab, transform);


			ItemStack stack = new ItemStack (entry.Value.blockId, 64);
			ItemSlot slot = new ItemSlot (newSlot.GetComponent<UIItemSlot> (), stack);
			slot.isCreative = true;
		}
	}
}
