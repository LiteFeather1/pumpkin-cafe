using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    [Header("Start UI")]
    [SerializeField] private CanvasGroup _startGroup;
    [SerializeField] private float _fadeTimeStartGroup;

    [Header("Game UI")]
    [SerializeField] private CanvasGroup _gameGroup;
    [SerializeField] private float _fadeTimeGameGroup;

    [Header("Bottom Bar")]
    [SerializeField] private TextMeshProUGUI t_store;
    [SerializeField] private TextMeshProUGUI t_kitchen;

    [Header("Cursor")]
    [SerializeField] private Image i_cursor;
    [SerializeField] private Sprite _openHand;
    [SerializeField] private Sprite _closeHand;

    [Header("Client Dialogue")]
    [SerializeField] private RectTransform _clientDialogueRoot;
    [SerializeField] private TextMeshProUGUI t_clientDialogue;
    [SerializeField] private float _dialoguePopTime = 1f;
    [SerializeField] private AnimationCurve _dialoguePopCurve;
    [SerializeField] private AnimationCurve _dialogueHideCurve;

    [Header("Client Feedback")]
    [SerializeField] private Transform _deliverFeedbackRoot;
    [SerializeField] private TextMeshProUGUI t_deliverTitle;
    [SerializeField] private TextMeshProUGUI t_clientPatience;
    [SerializeField] private TextMeshProUGUI t_orderEquality;
    [SerializeField] private TextMeshProUGUI t_moneyGained;
    [SerializeField] private TextMeshProUGUI t_grade;
    [SerializeField] private RectTransform rt_grade;
    [SerializeField] private Image i_grade;
    [SerializeField] private DataGrade[] _dataGrade;
    [SerializeField] private Button b_nextClient;
    [SerializeField] private TextMeshProUGUI t_nextClient;

    [Header("Client Feedback Anim")]
    [SerializeField] private CanvasGroup _deliverGroup;
    [SerializeField] private float _deliverTimeRoot;
    [SerializeField] private AnimationCurve _deliverCurveRoot;
    [SerializeField] private float _gradeTime;
    [SerializeField] private AnimationCurve _gradeCurve;
    [SerializeField] private CanvasGroup _gradeGroup;
    private static readonly WaitForSeconds _waitBeforeGrade = new(.3f);

    [Header("Time")]
    [SerializeField] private TextMeshProUGUI t_time;

    [Header("Client Patience")]
    [SerializeField] private RectTransform _patienceRoot;
    private Vector2 _downPosPatience;
    private Vector2 _upPosPatience;
    [SerializeField] private Image i_patienceFill;
    [SerializeField] private Gradient g_patienceGradient;
    [SerializeField] private float _timeToAppearPatience = .5f;
    [SerializeField] private AnimationCurve _curveAppearPatience;
    private Client _currentClient;

    [Header("Client Order Helper")]
    [SerializeField] private TranslatedString _orderHelperTitle;
    [SerializeField] private TextMeshProUGUI t_titleOrderHelper;
    [SerializeField] private TextMeshProUGUI t_orderHelper;

    [Header("Money")]
    [SerializeField] private TextMeshProUGUI t_totalMoney;
    [SerializeField] private float _moneyStepTime = 1.5f;

    [Header("Mute Sprites")]
    [SerializeField] private Image i_mute;
    [SerializeField] private Sprite _mutedSprite;
    [SerializeField] private Sprite _unMutedSprite;

    [Header("Language")]
    [SerializeField] private TextMeshProUGUI t_language;

    [Header("Pause Overlay")]
    [SerializeField] private GameObject _pauseOverlay;

    [Header("Thanks Overlay")]
    [SerializeField] private GameObject _thanksForPlaying;
    [SerializeField] private TextMeshProUGUI t_thankForPlaying;
    [SerializeField] private GameObject _bottomBar;

    [Header("Block Overlay")]
    [SerializeField] private GameObject _block;

    private Languages Lang => GameManager.Instance.Language;

    private void Awake()
    {
        _gameGroup.alpha = 0f;

        _downPosPatience = _patienceRoot.localPosition;
        _patienceRoot.localPosition += new Vector3(0f, _patienceRoot.sizeDelta.y * 1.5f);
        _upPosPatience = _patienceRoot.localPosition;
        Cursor.visible = false;
    }

    private void OnApplicationFocus(bool focus) => Cursor.visible = false;

    private void OnEnable()
    {
        InputMaps.PlayerActions playerInputs = GameManager.InputManager.PlayerInputs;
        playerInputs.MuteUnmute.performed += SwapMuteSprite;

        playerInputs.LeftClick.performed += SetCloseHand;
        playerInputs.LeftClick.canceled += SetOpenHand;

        playerInputs.HIde_UI.performed += HideUi;
    }

    private void OnDisable()
    {
        InputMaps.PlayerActions playerInputs = GameManager.InputManager.PlayerInputs;
        playerInputs.MuteUnmute.performed -= SwapMuteSprite;

        playerInputs.LeftClick.performed -= SetCloseHand;
        playerInputs.LeftClick.canceled -= SetOpenHand;

        playerInputs.HIde_UI.performed -= HideUi;
    }

    private void Update()
    {
        if (_currentClient != null)
            SetPatience(_currentClient.PatienceT);

        var pos = GameManager.Camera.ScreenToWorldPoint(Input.mousePosition);
        pos.z = 0;
        i_cursor.rectTransform.position = pos;
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float toAlpha, float time)
    {
        float eTime = 0f;
        float startAlpha = group.alpha;
        while (eTime < time)
        {
            eTime += Time.deltaTime;
            float t = eTime / time;
            group.alpha = Mathf.Lerp(startAlpha, toAlpha, t);
            yield return null;
        }
    }

    public IEnumerator IntroFade()
    {
        yield return FadeCanvasGroup(_startGroup, 0f, _fadeTimeStartGroup);
        yield return FadeCanvasGroup(_gameGroup, 1f, _fadeTimeGameGroup);
        _gameGroup.interactable = true;
        yield return new WaitForSeconds(.5f);
        _startGroup.gameObject.SetActive(false);
    }

    private void SetOpenHand(InputAction.CallbackContext ctx) => i_cursor.sprite = _openHand;

    private void SetCloseHand(InputAction.CallbackContext ctx) => i_cursor.sprite = _closeHand;

    private IEnumerator DialogueTween(Vector2 from, Vector2 to, AnimationCurve curve)
    {
        _clientDialogueRoot.sizeDelta = from;
        float eTime = 0f;
        while (eTime < _dialoguePopTime)
        {
            float t = curve.Evaluate(eTime / _dialoguePopTime);
            _clientDialogueRoot.sizeDelta = Vector2.LerpUnclamped(from, to, t);
            eTime += Time.deltaTime;
            yield return null;
        }
        _clientDialogueRoot.sizeDelta = to;
    }

    private IEnumerator PopUpDialogueCO()
    {
        _clientDialogueRoot.gameObject.SetActive(true);
        yield return DialogueTween(new(64f, _clientDialogueRoot.sizeDelta.y), _clientDialogueRoot.sizeDelta, _dialoguePopCurve);
    }

    private void SetDialogue(string clientDialogue)
    {
        t_clientDialogue.text = clientDialogue;
        LayoutRebuilder.ForceRebuildLayoutImmediate(_clientDialogueRoot);
    }

    public void PopUpDialogue(string clientDialogue)
    {
        SetDialogue(clientDialogue);
        StartCoroutine(PopUpDialogueCO());
    }

    // Called by a button
    public void OpenOrder()
    {
        const string BULLET_POINT = "• ";
        const string COLOUR = "<color=#394D18>";
        Coffee coffeeCup = GameManager.Instance.CoffeeCup.DeliverCoffee;
        Coffee cauldronCoffee = GameManager.Instance.Cauldron.MixingCoffee;
        Coffee coffeeOrder = _currentClient.Coffee;

        System.Text.StringBuilder sb = new();
        if (coffeeOrder.CoffeeBean != null)
        {
            bool containCoffee = coffeeCup.CoffeeBean == coffeeOrder.CoffeeBean
                || cauldronCoffee.CoffeeBean ==  coffeeOrder.CoffeeBean;
            string colour = containCoffee ? COLOUR : ""; 
            sb.Append(BULLET_POINT).Append(colour).Append("<u>").Append(coffeeOrder.CoffeeBean.Name.String(Lang))
                .Append("</u></color>").AppendLine();
        }

        if (coffeeOrder.Milk != null)
        {
            bool containMilk = coffeeCup.Milk == coffeeOrder.Milk
                || cauldronCoffee.Milk == coffeeOrder.Milk;
            string colour = containMilk ? COLOUR : "";
            sb.Append(BULLET_POINT).Append(colour).Append("<u>").Append(coffeeOrder.Milk.Name.String(Lang))
                .Append("</u></color>").AppendLine();
        }

        for (int i = 0; i < coffeeOrder.MiscIngredients.Count; i++)
        {
            bool containIngredient = coffeeCup.Contains(coffeeOrder.MiscIngredients[i])
                || cauldronCoffee.Contains(coffeeOrder.MiscIngredients[i]);
            string colour = containIngredient ? COLOUR : "";

            sb.Append(BULLET_POINT).Append(colour).Append("<u>").Append(coffeeOrder.MiscIngredients[i].Name.String(Lang))
                .Append("</u></color>").AppendLine();
        }

        t_orderHelper.text = sb.ToString();
    }

    private IEnumerator HideDialogueCO()
    {
        var from = _clientDialogueRoot.sizeDelta;
        yield return DialogueTween(from , new(64f, _clientDialogueRoot.sizeDelta.y), _dialogueHideCurve);
        _clientDialogueRoot.gameObject.SetActive(false);
        _clientDialogueRoot.sizeDelta = from;
    }

    public void CoffeeDelivered(CoffeeComparisonResults results, float clientPatience)
    {
        // Hide Patience
        _currentClient = null;
        StartCoroutine(Move(_patienceRoot, _downPosPatience, _upPosPatience, _timeToAppearPatience, _curveAppearPatience));

        StartCoroutine(HideDialogueCO());
        _block.SetActive(true);
        string client, order;
        if (Lang == Languages.Portuguese)
        {
            client = "Cliente";
            order = "Pedido";
        }
        else
        {
            client = "Client";
            order = "Order";
        }
        t_clientPatience.text = $"{client}: {clientPatience:0}%";
        t_orderEquality.text = $"{order}: {results.Equality:0}%";
        t_moneyGained.text = $"+++${results.Money:0.00}";

        // Calculate grade
        float percentile = (results.Equality + clientPatience) * .5f;

        DataGrade grade = _dataGrade[^1];
        for (int i = 0; i < _dataGrade.Length - 1; i++)
        {
            if (percentile > _dataGrade[i].Value)
            {
                grade = _dataGrade[i];
                break;
            }
        }

        t_grade.text = new(grade.Grade, 1);
        i_grade.color = grade.Colour;
        StartCoroutine(DeliverFeedBackTween());
    }

    private IEnumerator GradeTween(Transform transform, CanvasGroup group, float time, AnimationCurve curve)
    {
        transform.gameObject.SetActive(true);
        group.alpha = 0f;
        var originalScale = transform.localScale;
        Vector2 from = new(5f, 5f);
        transform.localScale = from;
        float eTime = 0f;
        while (eTime < time)
        {
            eTime += Time.smoothDeltaTime;
            float t = curve.Evaluate(eTime / _deliverTimeRoot);
            transform.localScale = Vector2.LerpUnclamped(from, Vector2.one, t);
            group.alpha = t;
            yield return new WaitForEndOfFrame();
        }
        group.alpha = 1f;
        transform.localScale = originalScale;
    }

    private IEnumerator DeliverFeedBackTween()
    {
        b_nextClient.gameObject.SetActive(false);
        rt_grade.gameObject.SetActive(false);
        yield return GradeTween(_deliverFeedbackRoot, _deliverGroup, _deliverTimeRoot, _deliverCurveRoot);
        yield return _waitBeforeGrade;
        yield return GradeTween(rt_grade, _gradeGroup, _gradeTime, _deliverCurveRoot);
        b_nextClient.gameObject.SetActive(true);
    }

    public void OnAllClientsServed()
    {
        if (Lang ==Languages.English)
            t_nextClient.text = "Close Store";
        else
            t_nextClient.text = "Fechar Loja";

        b_nextClient.onClick = new();
        b_nextClient.onClick.AddListener(() =>
        {
            _bottomBar.SetActive(false);
            _deliverFeedbackRoot.gameObject.SetActive(false);
            _thanksForPlaying.SetActive(true);
        });
    }

    public void SetTime(float time)
    {
        int hour = Mathf.FloorToInt(time % 12);
        int minutes = Mathf.RoundToInt((time * 60f) % 60);
        t_time.text = $"{hour:0}:{minutes:00}{(hour >= 6 ? "PM" : "AM")}";
    }

    private IEnumerator Move(Transform trasnform, Vector2 from, Vector2 to, float time, AnimationCurve curve)
    {
        trasnform.localPosition = from;
        float eTime = 0f;
        while (eTime < time) 
        {
            float t = curve.Evaluate(eTime/ time);
            trasnform.localPosition = Vector2.LerpUnclamped(from, to, t);
            eTime += Time.fixedDeltaTime;
            yield return null;
        }
        trasnform.localPosition = to;
    }

    public void SetCurrentClient(Client currentClient)
    {
        _currentClient = currentClient;
        StartCoroutine(Move(_patienceRoot, _upPosPatience, _downPosPatience, _timeToAppearPatience, _curveAppearPatience));
    }

    private void SetPatience(float t)
    {
        i_patienceFill.fillAmount = t;
        i_patienceFill.color = g_patienceGradient.Evaluate(t);
    }

    public IEnumerator StepMoney(float currentMoney, float moneyAdded)
    {
        float eTime = 0f;
        while (eTime < _moneyStepTime)
        {
            eTime += Time.deltaTime;
            float m = Mathf.Lerp(currentMoney, currentMoney + moneyAdded, eTime / _moneyStepTime);
            t_totalMoney.text = $"${m:00.00}";
            yield return null;
        }
        t_totalMoney.text = $"${currentMoney + moneyAdded:00.00}";
    }

    public void SwapMuteSprite()
    {
        i_mute.sprite = i_mute.sprite == _mutedSprite ? _unMutedSprite : _mutedSprite;
    }

    private void SwapMuteSprite(InputAction.CallbackContext ctx) => SwapMuteSprite();

    public void PauseOverlay(bool state) => _pauseOverlay.SetActive(state);

    public void UpdateLanguage(Languages lang)
    {
        if (lang == Languages.English)
        {
            t_language.text = "EN";

            t_store.text = "Store";
            t_kitchen.text = "Kitchen";

            t_deliverTitle.text = "Coffee Delivered!";
            t_nextClient.text = "Next Client";

            t_thankForPlaying.text = "You survided the halloween Night!\r\nThanks for Playing!!!";
        }
        else
        {
            t_language.text = "PT";

            t_store.text = "Loja";
            t_kitchen.text = "Cozinha";

            t_deliverTitle.text = "Café Entregado!";
            t_nextClient.text = "Próximo Cliente";
            t_thankForPlaying.text = "Voce sobreviveu a noite!\r\nObrigado por Jogar!!!";
        }

        t_titleOrderHelper.text = _orderHelperTitle.String(lang);

        if (_currentClient == null)
            return;

        SetDialogue(_currentClient.Dialogue.String(lang));
    }

    private void HideUi(InputAction.CallbackContext ctx)
    {
        _gameGroup.alpha = _gameGroup.alpha == 1f ? 0f : 1f;
    }

    [System.Serializable]
    private struct DataGrade
    {
        [field: SerializeField] public float Value { get;private set; }
        [field: SerializeField] public char Grade { get;private set; }
        [field: SerializeField] public Color Colour { get;private set; }
    }
}
