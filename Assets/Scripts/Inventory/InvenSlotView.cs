using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 인벤토리 단일 슬롯 UI 표시를 담당한다.
/// </summary>
public class InvenSlotView : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image iconImage;

    private ItemData boundItemData;
    private int boundSlotIndex = -1;

    /// <summary>
    /// 슬롯 클릭 시 선택된 슬롯 인덱스와 아이템 데이터를 전달한다.
    /// </summary>
    public event Action<int, ItemData> SlotClicked;

    /// <summary>
    /// 슬롯 인덱스와 아이템을 바인딩한다.
    /// </summary>
    /// <param name="slotIndex">슬롯 인덱스.</param>
    /// <param name="itemData">표시할 아이템 데이터.</param>
    public void Bind(int slotIndex, ItemData itemData)
    {
        boundSlotIndex = slotIndex;
        boundItemData = itemData;

        if (iconImage == null)
        {
            return;
        }

        if (itemData == null || itemData.Icon == null)
        {
            Clear();
            return;
        }

        iconImage.sprite = itemData.Icon;
        iconImage.enabled = true;
        iconImage.raycastTarget = false;
    }

    /// <summary>
    /// 슬롯 표시를 비운다.
    /// </summary>
    public void Clear()
    {
        boundSlotIndex = -1;
        boundItemData = null;

        if (iconImage == null)
        {
            return;
        }

        iconImage.sprite = null;
        iconImage.enabled = false;
        iconImage.raycastTarget = false;
    }

    /// <summary>
    /// 슬롯 클릭 입력을 처리한다.
    /// </summary>
    /// <param name="eventData">포인터 이벤트 데이터.</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        //string itemName = boundItemData != null ? boundItemData.ItemName : "(Empty)";
        //string buttonName = eventData.button.ToString();
        //Debug.Log($"[InvenSlotView] OnPointerClick - Slot: {name}, Item: {itemName}, Button: {buttonName}", this);
        SlotClicked?.Invoke(boundSlotIndex, boundItemData);
    }
}
