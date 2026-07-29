using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// BUG THẬT: InventorySystem._itemDatabase là mảng gán TAY trên component trong scene -- mọi ItemData mới
// tạo qua script (5 mảnh giấy, 3 chìa khoá, sổ ghi nợ, nhật ký, hộp nhạc...) KHÔNG tự động lọt vào mảng
// này. GameData.collectedItems vẫn có đúng itemId (AddItem() không phụ thuộc _itemDatabase), nhưng
// InventoryUI luôn tra tên/icon qua GetItemData(itemId) -- không có trong _itemDatabase thì trả về null,
// UI hiện trống trơn dù đã nhặt được đồ thật. Tool này auto-scan TOÀN BỘ ItemData trong thư mục Items và
// bù vào mảng, không trùng lặp, không cần biết tên từng file.
public static class VoD_RegisterItemDatabase
{
    private const string ItemsRootFolder = "Assets/_Project/Data/Items";

    [MenuItem("VoD/Villa/Fix - Đăng Ký Toàn Bộ ItemData Vào InventorySystem")]
    public static void Register()
    {
        var inventory = Object.FindFirstObjectByType<InventorySystem>(FindObjectsInactive.Include);
        if (inventory == null)
        {
            Debug.LogError("[VoD] Không tìm thấy InventorySystem nào trong scene.");
            return;
        }

        var guids = AssetDatabase.FindAssets("t:ItemData", new[] { ItemsRootFolder });
        var allItems = guids
            .Select(g => AssetDatabase.LoadAssetAtPath<ItemData>(AssetDatabase.GUIDToAssetPath(g)))
            .Where(item => item != null)
            .ToList();

        var so = new SerializedObject(inventory);
        var dbProp = so.FindProperty("_itemDatabase");

        var existing = new List<ItemData>();
        for (int i = 0; i < dbProp.arraySize; i++)
        {
            var v = dbProp.GetArrayElementAtIndex(i).objectReferenceValue as ItemData;
            if (v != null) existing.Add(v);
        }

        int added = 0;
        foreach (var item in allItems)
        {
            if (existing.Contains(item)) continue;
            existing.Add(item);
            added++;
        }

        dbProp.arraySize = existing.Count;
        for (int i = 0; i < existing.Count; i++)
            dbProp.GetArrayElementAtIndex(i).objectReferenceValue = existing[i];

        so.ApplyModifiedProperties();

        Debug.Log($"[VoD] Đã quét '{ItemsRootFolder}' ({allItems.Count} ItemData tìm thấy) -- thêm mới {added}, " +
                  $"tổng cộng {existing.Count} item trong Item Database của '{inventory.name}'. Save Scene.");
    }
}
