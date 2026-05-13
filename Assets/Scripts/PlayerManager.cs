using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    #region Singleton

    public static PlayerManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    #endregion

    public GameObject player;
    public Camera alternateCamera;

    private void Update()
    {
        alternateCamera.enabled = Keyboard.current.tabKey.isPressed;
    }
}
