
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
[RequireComponent(typeof(Rigidbody))]
public class ProductGrabReturn : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FactoryReturnZone returnZone;
    [SerializeField] private FactoryCreditManager creditManager;

    [Header("Grab Settings")]
    [SerializeField] private float maxLiftHeight = 0.4f;

    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;
    private ProductMover mover;
    private ProductQuality quality;

    private Transform handAttach;
    private Vector3 grabOffset;
    private Quaternion grabRotationOffset;

    private Vector3 savedPosition;
    private Quaternion savedRotation;

    [SerializeField]
    private FactoryDifficultyManager difficultyManager;

    private bool isGrabbed;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
        mover = GetComponent<ProductMover>();
        quality = GetComponent<ProductQuality>();

        if (returnZone == null)
            returnZone = FindFirstObjectByType<FactoryReturnZone>();

        if (creditManager == null)
            creditManager = FindFirstObjectByType<FactoryCreditManager>();

        if (difficultyManager == null)
            difficultyManager =
                FindFirstObjectByType<FactoryDifficultyManager>();

        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void OnEnable()
    {
        grabInteractable.selectEntered.AddListener(OnGrabStarted);
        grabInteractable.selectExited.AddListener(OnGrabReleased);
    }

    private void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrabStarted);
        grabInteractable.selectExited.RemoveListener(OnGrabReleased);
    }

    private void OnGrabStarted(SelectEnterEventArgs args)
    {
        if (mover == null)
            return;

        savedPosition = transform.position;
        savedRotation = transform.rotation;

        mover.SetPlayerGrabbed(true);

        handAttach =
            args.interactorObject.GetAttachTransform(
                grabInteractable
            );

        if (handAttach == null)
        {
            mover.SetPlayerGrabbed(false);
            return;
        }

        grabOffset =
            transform.position - handAttach.position;

        grabRotationOffset =
            Quaternion.Inverse(handAttach.rotation) *
            transform.rotation;

        isGrabbed = true;

        Debug.Log("[Product] Grabbed by player.");
    }

    private void LateUpdate()
    {
        if (!isGrabbed || handAttach == null)
            return;

        Vector3 targetPosition =
            handAttach.position + grabOffset;

        transform.position = targetPosition;

        transform.rotation =
            handAttach.rotation * grabRotationOffset;
    }

    private void OnGrabReleased(SelectExitEventArgs args)
    {
        if (!isGrabbed)
            return;

        isGrabbed = false;
        handAttach = null;

        bool insideReturnBin =
            returnZone != null &&
            returnZone.ContainsPoint(transform.position);

        if (insideReturnBin)
        {
            bool defective =
                quality != null && quality.IsDefective;

            if (defective && creditManager != null)
            {
                creditManager.AddCredits(
                    creditManager.productionCost
                );

                Debug.Log(
                    "[Quality Control] Defective product rejected. " +
                    $"Refund: +{creditManager.productionCost} CR"
                );
            }

            else
            {
                int penalty = difficultyManager != null
                    ? difficultyManager.WrongRejectionPenalty
                    : 0;

                if (creditManager != null && penalty > 0)
                {
                    int deduction = Mathf.Min(
                        penalty,
                        creditManager.currentCredits
                    );

                    if (deduction > 0)
                        creditManager.TrySpend(deduction);
                }

                Debug.LogWarning(
                    "[Quality Control] Normal product discarded! " +
                    $"Penalty: -{penalty} CR"
                );
            }


            mover.UnregisterProduct();

            gameObject.SetActive(false);
            Destroy(gameObject);
            return;
        }

        // Invalid rejection: return to the exact conveyor
        // position where the product was picked up.
        transform.SetPositionAndRotation(
            savedPosition,
            savedRotation
        );

        rb.position = savedPosition;
        rb.rotation = savedRotation;

        mover.SetPlayerGrabbed(false);

        Debug.Log(
            "[Product] Returned to conveyor."
        );
    }
}
