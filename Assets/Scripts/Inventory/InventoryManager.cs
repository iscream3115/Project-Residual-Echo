using System.Collections.Generic;
using UnityEngine;
using ResidualEcho.Player;

/// <summary>
/// 인벤토리 시스템 상태를 관리하고 인벤토리 입력 게이트웨이 이벤트를 처리한다.
/// </summary>
public class InventoryManager : MonoBehaviour
{
    private InventoryInputGateway inputGateway;

    [SerializeField] private GameObject inventoryRoot;
    [SerializeField] private InvenGridView inventoryGridView;
    [SerializeField] private InventoryItemInfoView itemInfoView;
    [SerializeField] private Transform dropOrigin;

    private readonly List<ItemData> items = new();
    private bool isOpen;
    private int selectedItemIndex = -1;
    private ItemData selectedItemData;

    private void Awake()
    {
        SetOpenState(false);
        inputGateway = GetComponent<InventoryInputGateway>();
        RefreshGrid();
    }

    private void OnEnable()
    {
        if (inputGateway != null)
        {
            inputGateway.ToggleRequested += HandleToggleRequested;
            inputGateway.CloseRequested += HandleCloseRequested;
        }

        if (inventoryGridView != null)
        {
            inventoryGridView.SlotItemSelected += HandleSlotItemSelected;
        }
    }

    private void OnDisable()
    {
        if (inputGateway != null)
        {
            inputGateway.ToggleRequested -= HandleToggleRequested;
            inputGateway.CloseRequested -= HandleCloseRequested;
        }

        if (inventoryGridView != null)
        {
            inventoryGridView.SlotItemSelected -= HandleSlotItemSelected;
        }
    }

    /// <summary>
    /// 선택된 아이템을 사용 처리한다.
    /// 현재는 실제 능력치 반영 대신 사용 로그만 출력한다.
    /// </summary>
    public void OnClickUseSelectedItem()
    {
        if (!TryGetSelectedItem(out ItemData selectedItem))
        {
            Debug.Log("사용할 아이템이 선택되지 않았다.");
            return;
        }

        Debug.Log($"{selectedItem.ItemName} 아이템을 사용했다!");
        RemoveSelectedItemFromInventory();
    }

    /// <summary>
    /// 선택된 아이템을 플레이어 위치에 드롭하고 인벤토리에서 제거한다.
    /// </summary>
    public void OnClickDropSelectedItem()
    {
        if (!TryGetSelectedItem(out ItemData selectedItem))
        {
            Debug.Log("버릴 아이템이 선택되지 않았다.");
            return;
        }

        TryDropSelectedItemToWorld(selectedItem);
        RemoveSelectedItemFromInventory();
    }

    /// <summary>
    /// 인벤토리 창을 토글한다.
    /// </summary>
    public void ToggleInventory()
    {
        SetOpenState(!isOpen);
    }

    /// <summary>
    /// 인벤토리 창을 닫는다.
    /// </summary>
    public void CloseInventory()
    {
        SetOpenState(false);
    }

    /// <summary>
    /// 인벤토리에 아이템을 추가한다.
    /// 왼쪽 위 슬롯(인덱스 0)부터 순서대로 채운다.
    /// </summary>
    /// <param name="itemData">추가할 아이템 데이터.</param>
    /// <returns>추가 성공 여부.</returns>
    public bool TryAddItem(ItemData itemData)
    {
        if (itemData == null || inventoryGridView == null)
        {
            return false;
        }

        if (items.Count >= inventoryGridView.SlotCount)
        {
            Debug.Log("인벤토리가 가득 찼다!");
            return false;
        }

        items.Add(itemData);
        RefreshGrid();
        return true;
    }

    /// <summary>
    /// 인벤토리 전체를 비운다.
    /// </summary>
    public void ClearAllItems()
    {
        items.Clear();
        RefreshGrid();
    }

    private void HandleToggleRequested()
    {
        ToggleInventory();
    }

    private void HandleCloseRequested()
    {
        CloseInventory();
    }

    private void SetOpenState(bool shouldOpen)
    {
        isOpen = shouldOpen;

        if (inventoryRoot != null)
        {
            inventoryRoot.SetActive(isOpen);
        }

        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isOpen;

        if (!isOpen)
        {
            ClearSelection();
        }
    }

    private void RefreshGrid()
    {
        if (inventoryGridView == null)
        {
            return;
        }

        inventoryGridView.Render(items);
    }

    private void HandleSlotItemSelected(int slotIndex, ItemData itemData)
    {
        selectedItemIndex = slotIndex;
        selectedItemData = itemData;

        if (itemInfoView == null)
        {
            return;
        }
        
        itemInfoView.ShowItem(itemData);
    }

    private bool TryGetSelectedItem(out ItemData itemData)
    {
        itemData = null;

        if (selectedItemIndex < 0 || selectedItemIndex >= items.Count)
        {
            return false;
        }

        itemData = selectedItemData;
        return itemData != null;
    }

    private void RemoveSelectedItemFromInventory()
    {
        if (selectedItemIndex < 0 || selectedItemIndex >= items.Count)
        {
            return;
        }

        items.RemoveAt(selectedItemIndex);
        RefreshGrid();
        ClearSelection();
    }

    private void ClearSelection()
    {
        selectedItemIndex = -1;
        selectedItemData = null;

        if (itemInfoView != null)
        {
            itemInfoView.Clear();
        }
    }

    private void TryDropSelectedItemToWorld(ItemData itemData)
    {
        ItemBase itemBase = FindDropSource(itemData);
        if (itemBase == null)
        {
            Debug.LogWarning($"{itemData.ItemName}에 대응하는 월드 아이템이 없어 드롭하지 못했다.");
            return;
        }

        Transform origin = ResolveDropOrigin();
        itemBase.transform.position = origin.position;
        itemBase.transform.rotation = origin.rotation;
        itemBase.ResetCollectedState();
        itemBase.gameObject.SetActive(true);
    }

    private ItemBase FindDropSource(ItemData itemData)
    {
        ItemBase[] worldItems = FindObjectsByType<ItemBase>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < worldItems.Length; i++)
        {
            ItemBase worldItem = worldItems[i];

            if (worldItem == null)
            {
                continue;
            }

            if (worldItem.ItemData == itemData && !worldItem.gameObject.activeSelf)
            {
                return worldItem;
            }
        }

        for (int i = 0; i < worldItems.Length; i++)
        {
            ItemBase worldItem = worldItems[i];

            if (worldItem == null)
            {
                continue;
            }

            if (worldItem.ItemData == itemData)
            {
                return worldItem;
            }
        }

        return null;
    }

    private Transform ResolveDropOrigin()
    {
        if (dropOrigin != null)
        {
            return dropOrigin;
        }

        PlayerController playerController = FindFirstObjectByType<PlayerController>();
        if (playerController != null)
        {
            return playerController.transform;
        }

        return transform;
    }
}
