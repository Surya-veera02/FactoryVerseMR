
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
[RequireComponent(typeof(ConveyorModule))]
[RequireComponent(typeof(Rigidbody))]
public class ConveyorGrabSnap : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;
    private ConveyorModule conveyorModule;
    private FactoryGrid grid;
    private Rigidbody rb;

    private Transform handAttach;
    private Vector3 grabOffset;

    private Quaternion grabRotationOffset;

    private bool isGrabbed;
    private Coroutine snapCoroutine;

    [Header("Grab Movement")]
    [SerializeField] private float maxLiftHeight = 0.35f;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        conveyorModule = GetComponent<ConveyorModule>();
        grid = GetComponentInParent<FactoryGrid>();
        rb = GetComponent<Rigidbody>();
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
        {
            StopCoroutine(snapCoroutine);
            snapCoroutine = null;
        }

        isGrabbed = false;
        handAttach = null;
    }

    private void OnGrabStarted(SelectEnterEventArgs args)
    {
        // Cancel any pending snap if grabbed again.
        if (snapCoroutine != null)
        {
            StopCoroutine(snapCoroutine);
            snapCoroutine = null;
        }

        if (grid == null)
            return;

        handAttach = args.interactorObject.GetAttachTransform(
            grabInteractable
        );

        if (handAttach == null)
            return;

        grabOffset = transform.position - handAttach.position;

        // Preserve the initial hand-to-object rotation.
        grabRotationOffset =
            Quaternion.Inverse(handAttach.rotation)
            * transform.rotation;

        isGrabbed = true;
    }


    private void LateUpdate()
    {
        if (!isGrabbed || handAttach == null || grid == null)
            return;

        // Calculate the desired position based on hand movement.
        Vector3 targetWorld =
            handAttach.position + grabOffset;

        // Convert the hand position to board-local coordinates.
        Vector3 localPosition =
            grid.transform.InverseTransformPoint(targetWorld);

        // Allow lifting above the board, but not below it.
        localPosition.y = Mathf.Clamp(
            localPosition.y,
            grid.BoardTopY,
            grid.BoardTopY + maxLiftHeight
        );

        // FIX: Actually move the conveyor!
        transform.position =
            grid.transform.TransformPoint(localPosition);

        // Follow the hand's rotation freely.
        transform.rotation =
            handAttach.rotation * grabRotationOffset;
    }


    private void OnGrabReleased(SelectExitEventArgs args)
    {
        if (!isGrabbed)
            return;

        isGrabbed = false;
        handAttach = null;

        // Remember exactly where the player released it.
        Vector3 releasePosition = transform.position;
        Quaternion releaseRotation = transform.rotation;

        snapCoroutine = StartCoroutine(
            SnapAfterRelease(releasePosition, releaseRotation)
        );
    }


    private IEnumerator SnapAfterRelease(
        Vector3 releasePosition,
        Quaternion releaseRotation)
    {
        // Wait until XR release and physics processing finish.
        yield return new WaitForFixedUpdate();
        yield return new WaitForEndOfFrame();

        if (isGrabbed)
        {
            snapCoroutine = null;
            yield break;
        }

        // Restore the exact pose at the moment of release.
        transform.SetPositionAndRotation(
            releasePosition,
            releaseRotation
        );

        // Determine the nearest 90-degree orientation.
        conveyorModule.SetQuarterTurnsFromWorldRotation(
            releaseRotation
        );


        // Snap position and check whether placement succeeded.
        bool snapSucceeded =
            conveyorModule.TrySnapToNearestCell();

        // Synchronize Rigidbody with the final snapped pose.
        rb.position = transform.position;
        rb.rotation = transform.rotation;

        // Notify the shop only when a new conveyor
        // has been placed on a valid grid cell.
        ShopPendingPlacement pendingPlacement =
            GetComponent<ShopPendingPlacement>();

        if (pendingPlacement != null &&
            pendingPlacement.IsPending &&
            snapSucceeded)
        {
            pendingPlacement.ConfirmPlacement();
        }


        Debug.Log(
            $"{name} snapped at {transform.position}"
        );

        snapCoroutine = null;
    }

}
