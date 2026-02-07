using UnityEngine;

/// <summary>
/// Drives a procedural starfield shader's parallax scroll based on player velocity.
/// Attach this to the same GameObject as the background SpriteRenderer
/// (typically a child of / following the camera).
/// </summary>
[RequireComponent(typeof(Renderer))]
public class StarfieldBackground : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The player's Rigidbody2D. If left empty, found automatically at Start.")]
    [SerializeField] private Rigidbody2D playerRigidbody;

    [Tooltip("Optional direct reference to the renderer. Auto-detected if null.")]
    [SerializeField] private Renderer targetRenderer;

    [Header("Parallax")]
    [Tooltip("Global multiplier for how much player velocity scrolls the stars.")]
    [SerializeField] private float scrollSensitivity = 0.15f;

    [Tooltip("Multiply X/Y scroll independently.")]
    [SerializeField] private Vector2 scrollDirectionMultiplier = Vector2.one;

    [Tooltip("Invert the scroll direction so stars move opposite to player.")]
    [SerializeField] private bool invertScroll = true;

    private Material runtimeMaterial;
    private Vector2 scrollOffset;

    private static readonly int ScrollOffsetId = Shader.PropertyToID("_ScrollOffset");

    private void Awake()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }
    }

    private void Start()
    {
        if (playerRigidbody == null)
        {
            PlayerController pc = FindObjectOfType<PlayerController>();
            if (pc != null)
            {
                playerRigidbody = pc.GetComponent<Rigidbody2D>();
            }
        }

        runtimeMaterial = targetRenderer != null ? targetRenderer.material : null;
    }

    private void Update()
    {
        if (runtimeMaterial == null)
        {
            return;
        }

        Vector2 velocity = playerRigidbody != null ? playerRigidbody.linearVelocity : Vector2.zero;
        float sign = invertScroll ? -1f : 1f;

        Vector2 delta = Vector2.Scale(velocity, scrollDirectionMultiplier) * (scrollSensitivity * sign * Time.deltaTime);
        scrollOffset += delta;
        runtimeMaterial.SetVector(ScrollOffsetId, scrollOffset);
    }
}
