using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// SAFEX UNIVERSAL BACK BUTTON
///
/// Provides one safe BACK button across the complete application:
///
/// HOME
/// MODULE SELECTION
/// BRIEFING
/// AR PLACEMENT
/// TRAINING
/// TRAINING COMPLETE
/// CERTIFICATE
///
/// Navigation strategy:
/// - The current scene is reloaded.
/// - This safely clears AR placement, spawned MineRoot,
///   active training controllers, popups, routes and certificate UI.
/// - The application returns to its initial Home state.
///
/// This intentionally avoids forcing individual controllers
/// into partially-reset states.
/// </summary>
public class SAFEXGlobalBackButton : MonoBehaviour
{
    private const string CanvasName =
        "SAFEX GLOBAL BACK CANVAS";

    private Canvas canvas;
    private Button backButton;
    private CanvasGroup canvasGroup;

    private bool isReloading;

    private readonly Color panelColor =
        new Color(
            0.015f,
            0.025f,
            0.032f,
            0.94f
        );

    private readonly Color borderColor =
        new Color(
            0.10f,
            0.85f,
            0.90f,
            0.55f
        );

    private readonly Color textColor =
        new Color(
            0.88f,
            0.96f,
            0.98f,
            1f
        );

    private void Awake()
    {
        // Only one global back button should exist.
        SAFEXGlobalBackButton existing =
            FindFirstObjectByType<
                SAFEXGlobalBackButton
            >();

        if (existing != null &&
            existing != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;

        CreateBackButton();

        UpdateVisibility();
    }

    private void Start()
    {
        UpdateVisibility();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode)
    {
        isReloading = false;

        UpdateVisibility();
    }

    private void Update()
    {
        // Keep the button synchronized with the current scene.
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        if (canvasGroup == null)
            return;

        SAFEXFlowController flow =
            FindFirstObjectByType<
                SAFEXFlowController
            >();

        if (flow == null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            return;
        }

        // Keep the button available on every SAFEX screen.
        // We intentionally do not depend on a CurrentState property,
        // because the existing SAFEXFlowController API may not expose one.
        bool show = true;

        canvasGroup.alpha =
            show ? 1f : 0f;

        canvasGroup.interactable =
            show && !isReloading;

        canvasGroup.blocksRaycasts =
            show && !isReloading;
    }

    private void HandleBackPressed()
    {
        if (isReloading)
            return;

        isReloading = true;

        if (canvasGroup != null)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        Debug.Log(
            "SAFEX BACK: Returning to HOME and resetting current session."
        );

        Scene currentScene =
            SceneManager.GetActiveScene();

        if (currentScene.buildIndex < 0)
        {
            Debug.LogError(
                "SAFEX BACK: Current scene is not available in Build Settings."
            );

            isReloading = false;
            UpdateVisibility();

            return;
        }

        SceneManager.LoadScene(
            currentScene.buildIndex,
            LoadSceneMode.Single
        );
    }

    private void CreateBackButton()
    {
        GameObject old =
            GameObject.Find(CanvasName);

        if (old != null)
            Destroy(old);

        GameObject canvasObject =
            new GameObject(CanvasName);

        canvas =
            canvasObject.AddComponent<Canvas>();

        canvas.renderMode =
            RenderMode.ScreenSpaceOverlay;

        canvas.sortingOrder = 1000;

        CanvasScaler scaler =
            canvasObject.AddComponent<CanvasScaler>();

        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;

        scaler.referenceResolution =
            new Vector2(1080f, 1920f);

        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        canvasGroup =
            canvasObject.AddComponent<CanvasGroup>();

        // -------------------------------------------------
        // BACK BUTTON
        // -------------------------------------------------

        GameObject buttonObject =
            new GameObject("SAFEX BACK BUTTON");

        buttonObject.transform.SetParent(
            canvas.transform,
            false
        );

        Image buttonImage =
            buttonObject.AddComponent<Image>();

        buttonImage.color =
            panelColor;

        backButton =
            buttonObject.AddComponent<Button>();

        backButton.targetGraphic =
            buttonImage;

        ColorBlock colors =
            backButton.colors;

        colors.normalColor =
            panelColor;

        colors.highlightedColor =
            new Color(
                0.025f,
                0.12f,
                0.14f,
                0.98f
            );

        colors.pressedColor =
            new Color(
                0.04f,
                0.18f,
                0.20f,
                1f
            );

        colors.selectedColor =
            colors.highlightedColor;

        backButton.colors = colors;

        RectTransform buttonRect =
            buttonObject.GetComponent<RectTransform>();

        buttonRect.anchorMin =
            new Vector2(0f, 1f);

        buttonRect.anchorMax =
            new Vector2(0f, 1f);

        buttonRect.pivot =
            new Vector2(0f, 1f);

        buttonRect.anchoredPosition =
            new Vector2(32f, -38f);

        buttonRect.sizeDelta =
            new Vector2(190f, 64f);

        // -------------------------------------------------
        // BORDER
        // -------------------------------------------------

        GameObject borderObject =
            new GameObject("BACK BORDER");

        borderObject.transform.SetParent(
            buttonObject.transform,
            false
        );

        Image borderImage =
            borderObject.AddComponent<Image>();

        borderImage.color =
            borderColor;

        borderImage.raycastTarget = false;

        RectTransform borderRect =
            borderObject.GetComponent<RectTransform>();

        borderRect.anchorMin =
            Vector2.zero;

        borderRect.anchorMax =
            Vector2.one;

        borderRect.offsetMin =
            Vector2.zero;

        borderRect.offsetMax =
            Vector2.zero;

        // -------------------------------------------------
        // TEXT
        // -------------------------------------------------

        GameObject textObject =
            new GameObject("BACK TEXT");

        textObject.transform.SetParent(
            buttonObject.transform,
            false
        );

        Text text =
            textObject.AddComponent<Text>();

        text.font =
            Resources.GetBuiltinResource<Font>(
                "LegacyRuntime.ttf"
            );

        text.text =
            "←  BACK";

        text.fontSize = 18;

        text.fontStyle =
            FontStyle.Normal;

        text.color =
            textColor;

        text.alignment =
            TextAnchor.MiddleCenter;

        text.horizontalOverflow =
            HorizontalWrapMode.Overflow;

        text.verticalOverflow =
            VerticalWrapMode.Overflow;

        text.raycastTarget = false;

        RectTransform textRect =
            textObject.GetComponent<RectTransform>();

        textRect.anchorMin =
            Vector2.zero;

        textRect.anchorMax =
            Vector2.one;

        textRect.offsetMin =
            Vector2.zero;

        textRect.offsetMax =
            Vector2.zero;

        backButton.onClick.AddListener(
            HandleBackPressed
        );
    }
}
