using System.Linq;
using UnityEditor;
using UnityEngine;

// Sau bug fix "tiêu đề Examine trống lúc soi trực tiếp ngoài world" (ExamineItem._itemName) -- liệt kê
// TOÀN BỘ ExamineItem trong scene đang mở, đánh dấu cái nào ĐÃ đăng ký qua InventoryUI._examineRegistry
// (soi từ Inventory, ItemData tự lo tên, ĐỂ TRỐNG _itemName là đúng) và cái nào CHƯA đăng ký + đang để
// trống tên (soi trực tiếp ngoài world -- CẦN điền _itemName mới hiện tiêu đề/prompt).
public static class VoD_ListExamineItemNames
{
    [MenuItem("VoD/Villa/Scan - Liệt Kê ExamineItem Cần Điền Tên")]
    public static void Scan()
    {
        var allExamine = Object.FindObjectsByType<ExamineItem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        var inventoryUI = Object.FindFirstObjectByType<InventoryUI>(FindObjectsInactive.Include);

        var registeredSet = new System.Collections.Generic.HashSet<ExamineItem>();
        if (inventoryUI != null)
        {
            var so = new SerializedObject(inventoryUI);
            var registryProp = so.FindProperty("_examineRegistry");
            if (registryProp != null)
            {
                for (int i = 0; i < registryProp.arraySize; i++)
                {
                    var entry = registryProp.GetArrayElementAtIndex(i);
                    var examineRef = entry.FindPropertyRelative("examineItem").objectReferenceValue as ExamineItem;
                    if (examineRef != null) registeredSet.Add(examineRef);
                }
            }
        }

        Debug.Log($"[VoD][ExamineNames] Tổng cộng {allExamine.Length} ExamineItem trong scene. Registered qua Inventory = {registeredSet.Count}.");

        int needsName = 0;
        foreach (var item in allExamine.OrderBy(i => i.gameObject.name))
        {
            var so = new SerializedObject(item);
            string itemName = so.FindProperty("_itemName")?.stringValue ?? "";
            bool fromInventory = registeredSet.Contains(item);
            string path = GetHierarchyPath(item.transform);

            if (fromInventory)
            {
                Debug.Log($"  [Inventory-OK] '{path}' -- soi từ Inventory, không cần điền _itemName (đang: '{itemName}')");
            }
            else if (string.IsNullOrEmpty(itemName))
            {
                needsName++;
                Debug.LogWarning($"  [CẦN ĐIỀN] '{path}' -- soi trực tiếp ngoài world, _itemName đang TRỐNG -- tiêu đề Examine + prompt [E] sẽ không hiện tên.");
            }
            else
            {
                Debug.Log($"  [Đã có tên] '{path}' -- _itemName = '{itemName}'");
            }
        }

        Debug.Log($"[VoD][ExamineNames] XONG -- {needsName} object cần điền tên (xem cảnh báo vàng phía trên).");
    }

    private static string GetHierarchyPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
}
