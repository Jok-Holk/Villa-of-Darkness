using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace Phase1.VoVanThuan
{
    public interface IInteractable { void Interact(); }

    public static class GameData
    {
        public static HashSet<string> collectedItems = new HashSet<string>();
        public static void Reset() => collectedItems.Clear();
    }
}