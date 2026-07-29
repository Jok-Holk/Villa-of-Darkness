using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueLine
{
    public string speakerName;
    [TextArea(2, 6)]
    public string text;

    [Tooltip("Có giọng lồng tiếng (VO) → hiện dạng phụ đề tinh giản. Không tick → nội tâm/ghi chú, hiện dạng popup khung, dừng lại đọc.")]
    public bool hasVoice = true;

    [Tooltip("File audio giọng lồng tiếng thật -- chỉ có ý nghĩa khi hasVoice = true. Để trống thì dòng thoại vẫn hiện chữ bình thường, chỉ không phát tiếng.")]
    public AudioClip voiceClip;
}

[CreateAssetMenu(menuName = "Dialogue/Dialogue Asset")]
public class DialogueAsset : ScriptableObject
{
    public List<DialogueLine> lines = new List<DialogueLine>();
}
