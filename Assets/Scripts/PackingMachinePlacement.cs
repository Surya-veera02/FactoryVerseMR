
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
[RequireComponent(typeof(Rigidbody))]
public class PackingMachinePlacement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FactoryPlacementManager placementManager;
    [SerializeField] private FactoryGrid grid;

    [Header("Placement")]
    [SerializeField] private float heightOffset = 0f;
    [SerializeField] private float maxLiftHeight = 0.35f;

    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;
    private ShopPendingPlacement pendingPlacement;

    private Transform handAttach;
    private Vector3 grabOffset;
    private Quaternion grabRotationOffset;

    private bool isGrabbed;
    private bool isPlaced;
    public bool IsPlaced => isPlaced;
    private Coroutine snapCoroutine;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
        //pendingPlacement = GetComponent<ShopPendingPlacement>();

        if (grid == null)
            grid = GetComponentInParent<FactoryGrid>();

        if (placementManager == null)
            placementManager =
                GetComponentInParent<FactoryPlacementManager>();
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

        if (snapCoroutine != null)
            StopCoroutine(snapCoroutine);

        isGrabbed = false;
        handAttach = null;
    }

    private void OnGrabStarted(SelectEnterEventArgs args)
    {
        if (isPlaced)
            return;

        if (snapCoroutine != null)
        {
            StopCoroutine(snapCoroutine);
            snapCoroutine = null;
        }

        handAttach = args.interactorObject.GetAttachTransform(
            grabInteractable);

        if (handAttach == null)
            return;

        grabOffset = transform.position - handAttach.position;

        grabRotationOffset =
            Quaternion.Inverse(handAttach.rotation) *
            transform.rotation;

        isGrabbed = true;
    }

    private void LateUpdate()
    {
        if (!isGrabbed || handAttach == null || grid == null)
            return;

        Vector3 targetWorld =
            handAttach.position + grabOffset;

        Vector3 localPosition =
            grid.transform.InverseTransformPoint(targetWorld);

        localPosition.y = Mathf.Clamp(
            localPosition.y,
            grid.BoardTopY,
            grid.BoardTopY + maxLiftHeight);

        transform.position =
            grid.transform.TransformPoint(localPosition);

        transform.rotation =
            handAttach.rotation * grabRotationOffset;
    }

    private void OnGrabReleased(SelectExitEventArgs args)
    {
        if (!isGrabbed)
            return;

        isGrabbed = false;
        handAttach = null;

        Vector3 releasePosition = transform.position;

        snapCoroutine = StartCoroutine(
            SnapAfterRelease(releasePosition));
    }

    private IEnumerator SnapAfterRelease(Vector3 releasePosition)
    {
        yield return new WaitForFixedUpdate();
        yield return new WaitForEndOfFrame();

        if (isGrabbed)
        {
            snapCoroutine = null;
            yield break;
        }

        transform.position = releasePosition;

        if (grid == null || placementManager == null ||
            !grid.TryGetCell(
                transform.position,
                out int column,
                out int row))
        {
            Debug.LogWarning(
                "[Packing Machine] Released outside grid.");
            snapCoroutine = null;
            yield break;
        }

        Vector2Int cell = new Vector2Int(column, row);

        ConveyorModule conveyor =
            placementManager.GetModuleAtCell(cell);

        if (conveyor == null)
        {
            Debug.LogWarning(
                "[Packing Machine] Must be placed over a conveyor.");
            snapCoroutine = null;
            yield break;
        }

        Vector3 targetPosition =
            grid.GetCellCenterWorld(column, row) +
            grid.transform.up * heightOffset;

        Quaternion targetRotation = conveyor.transform.rotation;

        transform.SetPositionAndRotation(
            targetPosition, targetRotation);

        rb.position = transform.position;
        rb.rotation = transform.rotation;


        isPlaced = true;

        // Packing machine is fixed after successful placement.
        grabInteractable.enabled = false;

        // Get the component at the moment placement succeeds,
        // rather than relying on an earlier Awake reference.
        ShopPendingPlacement pending =
            GetComponent<ShopPendingPlacement>();

        if (pending == null)
        {
            Debug.LogError(
                "[Packing Machine] ShopPendingPlacement " +
                "component not found on this GameObject!"
            );
        }
        else
        {
            Debug.Log(
                "[Packing Machine] Confirming shop placement..."
            );

            pending.ConfirmPlacement();
        }

        Debug.Log(
            $"[Packing Machine] Placed over conveyor at {cell}."
        );
    }

    public void RealignToConveyor(ConveyorModule conveyor)
    {
        if (!isPlaced || conveyor == null ||
            !conveyor.CurrentCell.HasValue || grid == null)
            return;

        Vector2Int cell = conveyor.CurrentCell.Value;

        Vector3 targetPosition =
            grid.GetCellCenterWorld(cell.x, cell.y) +
            grid.transform.up * heightOffset;

        Quaternion targetRotation = conveyor.transform.rotation;

        transform.SetPositionAndRotation(
            targetPosition,
            targetRotation
        );

        rb.position = targetPosition;
        rb.rotation = targetRotation;
    }

}
