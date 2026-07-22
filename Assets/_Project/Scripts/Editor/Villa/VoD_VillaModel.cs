using System;
using System.Collections.Generic;
using UnityEngine;

namespace VoDVilla
{
    /// Data schema thuần cho toàn bộ villa — không MonoBehaviour, không phụ thuộc
    /// scene. Đây là "ngôn ngữ chung" mọi tool VoD/Villa/* dùng để trao đổi, thay
    /// vì mỗi tool tự định nghĩa struct RoomSpec riêng (như VoD_ScanFixRoomScale,
    /// VoD_ScanTrueModuleScale cũ đã làm, gây lặp code + dễ lệch số liệu giữa
    /// các file). Serialize được qua JsonUtility (built-in Unity, không cần
    /// package thêm) — dùng để ghi/đọc file JSON làm "notes" bền giữa các phiên.

    [Serializable]
    public class RoomModel
    {
        public string name;
        public float cornerX, cornerZ;   // world X,Z của góc Tây-Bắc (min X, min Z)
        public float w, d, h;            // world extent theo X (W), Z (D), Y (H)
        public float rotationY;          // euler Y hiện tại của room root
        public string primaryMaterial;   // tên material đại diện (tường chính), để tham khảo khi build tầng trên

        public float MaxX => cornerX + w;
        public float MaxZ => cornerZ + d;
    }

    [Serializable]
    public class OpeningModel
    {
        public string room;
        public string side;        // "West" | "East" | "North" | "South"
        public float posAlong;     // world Z (nếu side West/East) hoặc world X (nếu side North/South) -- tâm lỗ hở
        public float width;
        public float sillY;        // độ cao đáy lỗ hở tính từ sàn phòng (0 = cửa đi chạm sàn)
        public float openHeight;   // chiều cao lỗ hở
        public string connectsToRoom; // rỗng nếu chưa xác định phòng bên kia
        public bool isWindow;      // true = cửa sổ (không phải cửa đi)
    }

    [Serializable]
    public class Vector2Data
    {
        public float x, z;
        public Vector2Data() { }
        public Vector2Data(float x, float z) { this.x = x; this.z = z; }
        public Vector2 ToVector2() => new Vector2(x, z);
    }

    [Serializable]
    public class FloorModel
    {
        public int index;          // 0 = tầng trệt, 1 = tầng 1, 2 = tầng 2
        public float elevationY;   // sàn tầng này ở world Y bao nhiêu
        public float height;       // chiều cao tầng
        public List<Vector2Data> outline = new List<Vector2Data>(); // đa giác bao ngoài, world space, thứ tự CW hoặc CCW nhất quán
        public bool isPlaceholder; // true = chỉ xây tường bao + mái, không chia phòng con
    }

    [Serializable]
    public class StaircaseModel
    {
        public float cornerX, cornerZ, w, d; // footprint (world), lấy theo CauThang hiện tại
        public int floorsSpanned;            // số tầng cầu thang xuyên qua (3 = trệt+1+2)
        public float floorHeight;            // chiều cao mỗi tầng (dùng để tính tổng độ cao leo)
    }

    [Serializable]
    public class VillaModel
    {
        public string generatedAtUtc;
        public string note;
        public List<RoomModel> rooms = new List<RoomModel>();
        public List<OpeningModel> openings = new List<OpeningModel>();
        public List<FloorModel> floors = new List<FloorModel>();
        public StaircaseModel staircase = new StaircaseModel();

        public string ToJson() => JsonUtility.ToJson(this, true);

        public static VillaModel FromJson(string json) => JsonUtility.FromJson<VillaModel>(json);

        public RoomModel FindRoom(string name) => rooms.Find(r => r.name == name);
    }
}
