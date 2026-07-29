using System.Collections.Generic;
using UnityEngine;

// Dùng chung cho mọi pill/badge bo tròn dựng bằng code (TutorialHintUI, InteractPromptUI, DiaryReaderUI...) --
// tách ra từ TutorialHintUI để không phải copy-paste lại y hệt logic vẽ sprite mỗi lần thêm 1 hệ UI mới cùng kiểu.
public static class VoDUISpriteUtil
{
    // BUG THẬT (Jok phát hiện -- "[ExecuteAlways] có thể dẫn tới treo Unity Editor"): trước đây mỗi lần gọi
    // CreateRoundedSprite() đều tạo MỚI 1 Texture2D + Sprite, KHÔNG BAO GIỜ tái sử dụng -- các hệ UI dùng
    // [ExecuteAlways] gọi lại hàm này mỗi lần OnEnable() (domain reload, bật/tắt object qua Preview Window...)
    // nên rò rỉ bộ nhớ tích luỹ dần, càng dùng lâu Editor càng nặng/có thể treo. Cache lại theo đúng bộ tham
    // số (w,h,radius) -- gọi lại với cùng kích thước chỉ trả về sprite CŨ đã tạo, không tạo thêm bao giờ.
    private static readonly Dictionary<(int w, int h, int radius), Sprite> _cache = new();

    public static Sprite CreateRoundedSprite(int w, int h, int radius)
    {
        var key = (w, h, radius);
        if (_cache.TryGetValue(key, out var cached) && cached != null) return cached;

        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false, true);
        var pixels = new Color32[w * h];
        Color32 white = new Color32(255, 255, 255, 255);
        Color32 clear = new Color32(255, 255, 255, 0);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                bool inside = true;
                if (x < radius && y < radius) inside = Vector2.Distance(new Vector2(x, y), new Vector2(radius, radius)) <= radius;
                else if (x >= w - radius && y < radius) inside = Vector2.Distance(new Vector2(x, y), new Vector2(w - radius, radius)) <= radius;
                else if (x < radius && y >= h - radius) inside = Vector2.Distance(new Vector2(x, y), new Vector2(radius, h - radius)) <= radius;
                else if (x >= w - radius && y >= h - radius) inside = Vector2.Distance(new Vector2(x, y), new Vector2(w - radius, h - radius)) <= radius;

                pixels[y * w + x] = inside ? white : clear;
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply();
        var sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect,
            new Vector4(radius, radius, radius, radius));

        _cache[key] = sprite;
        return sprite;
    }
}
