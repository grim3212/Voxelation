using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIItemSlot : MonoBehaviour {
	public bool isLinked = false;
	public ItemSlot itemSlot;
	public Image slotImage;
	public Image slotIcon;
	public Text slotAmount;

	World world;

	private void Awake () {
		world = GameObject.Find ("World").GetComponent<World> ();
	}

	public bool HasItem {
		get {
			if (itemSlot == null) {
				return false;
			}
			else {
				return itemSlot.HasItem;
			}
		}
	}

	public void Link (ItemSlot slot) {
		itemSlot = slot;
		isLinked = true;
		itemSlot.LinkUISlot (this);
		UpdateSlot ();
	}

	public void UnLink () {
		itemSlot.UnLinkUISlot ();
		itemSlot = null;
		UpdateSlot ();
	}

	public void UpdateSlot () {
		if (itemSlot != null && itemSlot.HasItem) {
			slotIcon.sprite = world.blockTypes[itemSlot.stack.id].icon;
			slotAmount.text = itemSlot.stack.amount.ToString ();
			slotIcon.enabled = true;
			slotAmount.enabled = true;
		}
		else {
			Clear ();
		}
	}

	public void Clear () {
		slotIcon.sprite = null;
		slotAmount.text = "";
		slotIcon.enabled = false;
		slotAmount.enabled = false;
	}

	private void OnDestroy () {
		if (isLinked) {
			itemSlot.UnLinkUISlot ();
		}
	}
}

public class ItemSlot {
	public ItemStack stack = null;
	private UIItemSlot slot = null;

	public bool isCreative;

	public ItemSlot (UIItemSlot _slot) {
		stack = null;
		slot = _slot;
		slot.Link (this);
	}

	public ItemSlot (UIItemSlot _slot, ItemStack _stack) {
		stack = _stack;
		slot = _slot;
		slot.Link (this);
	}

	public void LinkUISlot (UIItemSlot _slot) {
		slot = _slot;
	}

	public void UnLinkUISlot () {
		slot = null;
	}

	public void EmptySlot () {
		stack = null;
		if (slot != null)
			slot.UpdateSlot ();
	}

	public int Take (int amt) {
		if (amt > stack.amount) {
			int _amt = stack.amount;
			EmptySlot ();
			return _amt;
		}
		else if (amt < stack.amount) {
			stack.amount -= amt;
			slot.UpdateSlot ();
			return amt;
		}
		else {
			EmptySlot ();
			return amt;
		}
	}

	public ItemStack TakeAll () {
		ItemStack handOver = new ItemStack (stack.id, stack.amount);
		EmptySlot ();
		return handOver;
	}

	public void InsertStack (ItemStack _stack) {
		stack = _stack;
		slot.UpdateSlot();
	}

	public bool HasItem {
		get {
			if (stack != null) {
				return true;
			}
			else {
				return false;
			}
		}
	}
}
