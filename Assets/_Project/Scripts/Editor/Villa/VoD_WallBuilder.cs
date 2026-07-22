using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VoDVilla
{
    /// Utility DÙNG CHUNG để xây 4 mặt tường của 1 khối chữ nhật (phòng hoặc cả
    /// tầng) bằng box đơn giản, tự tách 2-3 mảnh quanh mỗi lỗ hở (cửa/cửa sổ).
    /// Tổng quát hoá từ logic ban đầu viết riêng trong VoD_RebuildRoomWalls.cs
    /// (dùng cho 4 phòng tầng trệt) — giờ VoD_BuildVilla.cs (tường bao tầng 1/2)
    /// cũng gọi lại đúng hàm này, không viết lại lần 2.
    public static class VoD_WallBuilder
    {
        public const float DefaultThickness = 0.25f;

        /// Xây 4 mặt tường (West/East/North/South) của 1 khối chữ nhật world-space
        /// [cornerX..cornerX+w] x [cornerZ..cornerZ+d], cao h, làm con của `holder`.
        /// `openings` chỉ cần chứa các opening có room-side khớp regex do caller lọc
        /// sẵn (hàm này không tự lọc theo room name).
        /// `skipSides` (có thể null): các mặt KHÔNG xây gì cả -- dùng khi ranh giới
        /// đó đã có phòng liền kề xây tường rồi (tránh 2 phòng cùng xây trùng 1
        /// bức tường tại đúng 1 toạ độ, gây chồng lấn/z-fighting).
        /// `exteriorSides` (có thể null): các mặt là tường NGOÀI THẬT của villa
        /// (không giáp phòng nào) -- dùng `extMat` thay vì `intMat` cho các mặt đó.
        public static int BuildRoomWalls(Transform holder, float cornerX, float cornerZ, float w, float d, float h,
            List<OpeningModel> openings, Material intMat, Material extMat, float thickness = DefaultThickness,
            HashSet<string> skipSides = null, HashSet<string> exteriorSides = null)
        {
            int count = 0;
            foreach (var side in new[] { "West", "East", "North", "South" })
            {
                if (skipSides != null && skipSides.Contains(side)) continue;
                bool isExterior = exteriorSides != null && exteriorSides.Contains(side);
                count += BuildSide(holder, side, cornerX, cornerZ, w, d, h, openings, intMat, extMat, isExterior, thickness);
            }
            return count;
        }

        // Mỗi Cube chỉ có 1 material cho cả 6 mặt -- tường NGOÀI THẬT (isExterior)
        // nhìn từ trong phòng cũng bị lộ màu ochre (dành riêng cho mặt ngoài) nếu
        // dùng 1 khối duy nhất. Sửa: tách khối tường ngoài thành 2 LỚP MỎNG xếp
        // chồng theo bề dày -- lớp phía ngoài (ochre) + lớp phía trong (cream) --
        // mỗi lớp chỉ hiện đúng 1 màu ở đúng mặt người chơi nhìn thấy. Tường nội bộ
        // (giữa 2 phòng) không cần tách, dùng 1 khối cream như cũ.
        private static int BuildSide(Transform holder, string side, float cornerX, float cornerZ, float w, float d, float h,
            List<OpeningModel> openings, Material intMat, Material extMat, bool isExterior, float thickness)
        {
            OpeningModel op = null;
            foreach (var o in openings) if (o.side == side) { op = o; break; }

            bool runIsZ = side == "West" || side == "East";
            float runStart = runIsZ ? cornerZ : cornerX;
            float runEnd = runIsZ ? cornerZ + d : cornerX + w;
            float fixedCoord = side == "West" ? cornerX : side == "East" ? cornerX + w
                              : side == "North" ? cornerZ : cornerZ + d;
            // Hướng "ra ngoài" theo trục bề dày: West/North lùi về toạ độ nhỏ hơn,
            // East/South tiến về toạ độ lớn hơn (nội thất phòng luôn nằm giữa 2
            // trục cornerX/cornerZ và cornerX+w/cornerZ+d).
            float outwardSign = (side == "West" || side == "North") ? -1f : 1f;

            void Build(string name, float runMin, float runMax, float yMin, float yMax)
            {
                if (isExterior)
                    CreateDualLayerBox(holder, name, runIsZ, fixedCoord, outwardSign, runMin, runMax, yMin, yMax, extMat, intMat, thickness);
                else
                    CreateBox(holder, name, runIsZ, fixedCoord, runMin, runMax, yMin, yMax, intMat, thickness);
            }

            int count = 0;
            if (op == null)
            {
                Build($"Wall_{side}", runStart, runEnd, 0f, h);
                count = 1;
            }
            else
            {
                var o = op;
                float openMin = o.posAlong - o.width * 0.5f;
                float openMax = o.posAlong + o.width * 0.5f;

                if (openMin > runStart) { Build($"Wall_{side}_Left", runStart, openMin, 0f, h); count++; }
                if (openMax < runEnd) { Build($"Wall_{side}_Right", openMax, runEnd, 0f, h); count++; }
                if (o.sillY + o.openHeight < h) { Build($"Wall_{side}_Header", openMin, openMax, o.sillY + o.openHeight, h); count++; }
                if (o.sillY > 0f) { Build($"Wall_{side}_Sill", openMin, openMax, 0f, o.sillY); count++; }
            }
            return count;
        }

        // Tường ngoài: 2 box mỏng (mỗi cái = nửa thickness) xếp cạnh nhau theo trục
        // bề dày -- box phía `outwardSign` dùng extMat (mặt ngoài villa), box còn
        // lại dùng intMat (mặt trong phòng, người chơi thấy khi đứng trong nhà).
        private static void CreateDualLayerBox(Transform parent, string namePrefix, bool runIsZ, float fixedCoord,
            float outwardSign, float runMin, float runMax, float yMin, float yMax, Material outerMat, Material innerMat, float totalThickness)
        {
            float half = totalThickness * 0.5f;
            float outerCenter = fixedCoord + outwardSign * (half * 0.5f);
            float innerCenter = fixedCoord - outwardSign * (half * 0.5f);
            CreateBox(parent, namePrefix + "_Outer", runIsZ, outerCenter, runMin, runMax, yMin, yMax, outerMat, half);
            CreateBox(parent, namePrefix + "_Inner", runIsZ, innerCenter, runMin, runMax, yMin, yMax, innerMat, half);
        }

        private static void CreateBox(Transform parent, string name, bool runIsZ, float fixedCoord, float runMin, float runMax, float yMin, float yMax, Material mat, float thickness)
        {
            float runLen = runMax - runMin;
            if (runLen <= 0.001f) return;
            float runCenter = (runMin + runMax) * 0.5f;
            float yCenter = (yMin + yMax) * 0.5f;
            float yLen = yMax - yMin;
            if (yLen <= 0.001f) return;

            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            Undo.RegisterCreatedObjectUndo(go, "VoD Build Wall Segment");
            go.transform.SetParent(parent, worldPositionStays: true);

            Vector3 pos = runIsZ
                ? new Vector3(fixedCoord, yCenter, runCenter)
                : new Vector3(runCenter, yCenter, fixedCoord);
            go.transform.position = pos;
            go.transform.rotation = Quaternion.identity;
            go.transform.localScale = runIsZ
                ? new Vector3(thickness, yLen, runLen)
                : new Vector3(runLen, yLen, thickness);

            if (mat != null)
            {
                var rend = go.GetComponent<MeshRenderer>();
                // Cube nguyên thuỷ chia UV 0-1 cho mỗi mặt bất kể kích thước thật -- nếu
                // dùng chung sharedMaterial, texture sẽ bị kéo giãn khác nhau tuỳ độ dài
                // từng đoạn tường (đoạn 9m và đoạn 1.7m trông to/nhỏ khác hẳn). Tạo 1
                // material INSTANCE riêng cho từng đoạn, chỉnh mainTextureScale theo đúng
                // kích thước thật (1 lần lặp texture / 1 mét) để mật độ hoạ tiết đồng nhất
                // giữa mọi đoạn tường, không phụ thuộc kích thước từng box.
                var instMat = new Material(mat);
                instMat.mainTextureScale = new Vector2(runLen, yLen);
                rend.sharedMaterial = instMat;
            }
        }
    }
}
