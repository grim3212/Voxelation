using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemStack {
	public string id;
	public int amount;

	public ItemStack (string _id, int _amount) {
		id = _id;
		amount = _amount;
	}
}
