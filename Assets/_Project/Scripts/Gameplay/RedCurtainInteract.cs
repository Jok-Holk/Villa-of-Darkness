using UnityEngine;

// Script này kế thừa interface IInteractable để hệ thống Raycast của game nhận diện được
public class RedCurtainInteract : MonoBehaviour, IInteractable
{
    // Hàm này sẽ tự động chạy khi người chơi đứng gần, nhìn vào vải đỏ và bấm phím E
    public void Interact()
    {
        // Ẩn tấm vải đỏ đi để lộ ra chiếc gương ma quái phía sau
        gameObject.SetActive(false); 
    }
}