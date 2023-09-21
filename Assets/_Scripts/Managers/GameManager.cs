using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static Camera Camera { get; private set; }


    [Header("Managers")]
    [SerializeField] private ClientManager _clientManager;
    [SerializeField] private UIManager _uiManager;
    public static ManagerInput InputManager { get; private set; }


    public static Client ClientManager;

    private void Awake()
    {
        Instance = this;
        InputManager = new();
        Camera = Camera.main;
    }

    private void OnEnable()
    {
        _clientManager.OnNewClient += AppearDialogue;
        _clientManager.OnClientServed += _uiManager.CoffeeDelivered;

        InputManager.PlayerInputs.LeftClick.performed += StartGame;
        InputManager.PlayerInputs.RightClick.performed += StartGame;
    }

    private void OnDisable()
    {
        _clientManager.OnNewClient -= AppearDialogue;
        _clientManager.OnClientServed -= _uiManager.CoffeeDelivered;
    }

    public IEnumerator StartGameCO()
    {
        InputManager.PlayerInputs.LeftClick.performed -= StartGame;
        InputManager.PlayerInputs.RightClick.performed -= StartGame;
        yield return _uiManager.IntroFade();
        _clientManager.AppearClient();
    }

    private void StartGame(InputAction.CallbackContext ctx) => StartCoroutine(StartGameCO());

    public static Vector2 MousePosition() => Camera.ScreenToWorldPoint(Input.mousePosition);

    private void AppearDialogue(Client client)
    {
        _uiManager.PopUpDialogue(client.Dialogue);
    }

    public class ManagerInput
    {
        private readonly InputMaps _inputMaps;

        public ManagerInput()
        {
            _inputMaps = new();
            _inputMaps.Player.Enable();
        }

        public InputMaps.PlayerActions PlayerInputs => _inputMaps.Player;
    }
}

