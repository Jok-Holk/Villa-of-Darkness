using UnityEngine;
using TMPro;

// Dùng chung cho mọi UI tự dựng bằng code (ExamineStageUI, HudMetersUI...) -- lấy ĐÚNG font asset
// "NotoSans SDF" đang chạy ổn định sẵn trong project (DialoguePanel dùng làm chính) thay vì tự import/gán
// font mới. Project từng gặp bug crash lúc thử đổi sang font KHÁC (ghi trong memory), nhưng font NÀY đã
// dùng an toàn ở nơi khác nên không phải rủi ro tương tự.
public static class VoDFontUtil
{
    private static TMP_FontAsset _cachedNotoSansFont;

    public static TMP_FontAsset FindNotoSansFont()
    {
        if (_cachedNotoSansFont != null) return _cachedNotoSansFont;

        // SỬA (Jok phát hiện -- DiaryReaderUI không ra đúng font Noto): tìm qua TMP_Text ĐANG DÙNG font này
        // (VD DialoguePanel) phụ thuộc component đó đã thật sự tồn tại/load vào bộ nhớ TẠI ĐÚNG THỜI ĐIỂM gọi
        // hàm này -- nếu hệ UI nào chạy OnEnable() SỚM hơn (VD UIBootstrap ép chạy rất sớm) thì có thể tìm
        // hụt, trả về null. Tìm THẲNG font asset qua TMP_FontAsset trước (không phụ thuộc component nào đang
        // dùng nó hay chưa) -- đáng tin hơn hẳn, chỉ fallback về cách cũ nếu không thấy.
        var allFonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
        foreach (var f in allFonts)
        {
            if (f != null && f.name.Contains("NotoSans"))
            {
                _cachedNotoSansFont = f;
                return _cachedNotoSansFont;
            }
        }

        var allTexts = Resources.FindObjectsOfTypeAll<TMP_Text>();
        foreach (var t in allTexts)
        {
            if (t.font != null && t.font.name.Contains("NotoSans"))
            {
                _cachedNotoSansFont = t.font;
                return _cachedNotoSansFont;
            }
        }
        Debug.LogWarning("[VoDFontUtil] Không tìm thấy font 'NotoSans SDF' đang dùng ở đâu trong scene -- giữ nguyên font mặc định.");
        return null;
    }
}
