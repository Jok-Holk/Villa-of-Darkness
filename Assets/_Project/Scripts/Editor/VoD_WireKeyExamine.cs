using UnityEditor;
using UnityEngine;

// Đăng ký Examine cho 3 chìa khoá vào InventoryUI._examineRegistry -- ExamineItem đã có sẵn ngay trong
// prefab dùng chung (Prop_Key01_Skeleton) từ trước, chỉ chưa được trỏ vào registry nên bấm [V] trong
// Inventory không có tác dụng gì (dù hint vẫn sáng vì isExaminable=1 đã bật đúng trên ItemData).
public static class VoD_WireKeyExamine
{
    private static readonly string[] KeyItemIds = { "key_salon", "key_sansau", "key_tiepkhach" };

    [MenuItem("VoD/Villa/Fix - Gắn Examine Cho 3 Chìa Khoá (xem lại từ Inventory)")]
    public static void Wire()
    {
        var inventoryUI = Object.FindFirstObjectByType<InventoryUI>(FindObjectsInactive.Include);
        if (inventoryUI == null) { Debug.LogError("[VoD][KeyExamine] Không tìm thấy InventoryUI."); return; }

        var pickupItems = Object.FindObjectsByType<PickupItem>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        var so = new SerializedObject(inventoryUI);
        var registryProp = so.FindProperty("_examineRegistry");

        int wired = 0;
        foreach (string keyId in KeyItemIds)
        {
            PickupItem match = null;
            foreach (var pickup in pickupItems)
            {
                var pickupSO = new SerializedObject(pickup);
                var itemDataProp = pickupSO.FindProperty("_itemData");
                var data = itemDataProp?.objectReferenceValue as ItemData;
                if (data != null && data.itemId == keyId) { match = pickup; break; }
            }

            if (match == null)
            {
                Debug.LogWarning($"[VoD][KeyExamine] Không tìm thấy PickupItem nào có itemId '{keyId}' trong scene -- bỏ qua.");
                continue;
            }

            var examineItem = match.GetComponent<ExamineItem>();
            if (examineItem == null)
            {
                Debug.LogWarning($"[VoD][KeyExamine] '{match.gameObject.name}' (itemId '{keyId}') không có ExamineItem -- bỏ qua.");
                continue;
            }

            bool alreadyRegistered = false;
            for (int i = 0; i < registryProp.arraySize; i++)
            {
                if (registryProp.GetArrayElementAtIndex(i).FindPropertyRelative("itemId").stringValue == keyId)
                {
                    // Cập nhật lại tham chiếu (phòng khi trỏ sai/rỗng từ trước) thay vì bỏ qua im lặng.
                    registryProp.GetArrayElementAtIndex(i).FindPropertyRelative("examineItem").objectReferenceValue = examineItem;
                    alreadyRegistered = true;
                    break;
                }
            }

            if (!alreadyRegistered)
            {
                int newIndex = registryProp.arraySize;
                registryProp.InsertArrayElementAtIndex(newIndex);
                var entry = registryProp.GetArrayElementAtIndex(newIndex);
                entry.FindPropertyRelative("itemId").stringValue = keyId;
                entry.FindPropertyRelative("examineItem").objectReferenceValue = examineItem;
            }

            wired++;
            Debug.Log($"[VoD][KeyExamine] Đã gắn '{keyId}' -> '{match.gameObject.name}'.");
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(inventoryUI);
        Debug.Log($"[VoD][KeyExamine] XONG -- {wired}/{KeyItemIds.Length} chìa khoá đã đăng ký Examine. Giờ bấm [V] trong Inventory sẽ mở được 360 độ cho chìa khoá.");
    }
}
