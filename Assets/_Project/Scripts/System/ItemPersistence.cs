using UnityEngine;

public class ItemPersistence : MonoBehaviour
{
    private const string SaveKey      = "VoD_Items";
    private const string ChapterKey   = "VoD_Chapter";
    private const string AudioLogsKey = "VoD_AudioLogs";

    private void Awake()
    {
        Load();
    }

    public void Save()
    {
        PlayerPrefs.SetString(SaveKey,      string.Join(",", GameData.collectedItems));
        PlayerPrefs.SetInt   (ChapterKey,   GameData.currentChapter);
        PlayerPrefs.SetInt   (AudioLogsKey, GameData.audioLogsHeard);
        PlayerPrefs.Save();
    }

    public void Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey)) return;

        string raw = PlayerPrefs.GetString(SaveKey, "");
        GameData.collectedItems.Clear();
        if (!string.IsNullOrEmpty(raw))
        {
            foreach (var id in raw.Split(','))
                if (!string.IsNullOrEmpty(id)) GameData.collectedItems.Add(id);
        }

        GameData.currentChapter = PlayerPrefs.GetInt(ChapterKey,   1);
        GameData.audioLogsHeard = PlayerPrefs.GetInt(AudioLogsKey, 0);
    }

    public void DeleteSave()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.DeleteKey(ChapterKey);
        PlayerPrefs.DeleteKey(AudioLogsKey);
        PlayerPrefs.Save();
    }

    public bool HasSave() => PlayerPrefs.HasKey(SaveKey);

    public void ApplyToInventory(InventorySystem inv)
    {
        if (inv == null) return;
        foreach (var id in GameData.collectedItems)
            inv.AddItem(id);
    }
}
