
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

        isGrabbed = true;
    }

    private void LateUpdate()
    {
        if (!isGrabbed || handAttach == null || grid == null)
            return;

        Vector3 targetWorld = handAttach.position + grabOffset;

        Vector3 localPosition =
            grid.transform.InverseTransformPoint(targetWorld);

        // Allow the player to lift the conveyor above the board,
        // but prevent it from moving underneath the board.
        localPosition.y = Mathf.Clamp(
            localPosition.y,
            grid.BoardTopY,
            grid.BoardTopY + maxLiftHeight
        );

        transform.position =
            grid.transform.TransformPoint(localPosition);

        transform.rotation =
            grid.transform.rotation *
            Quaternion.Euler(
                0f,
                conveyorModule.QuarterTurns * 90f,
                0f
            );
    }

    private void OnGrabReleased(SelectExitEventArgs args)
    {
        if (!isGrabbed)
            return;

        isGrabbed = false;
        handAttach = null;

        // Remember exactly where the player released it.
        Vector3 releasePosition = transform.position;

        snapCoroutine = StartCoroutine(
            SnapAfterRelease(releasePosition)
        );
    }

    private IEnumerator SnapAfterRelease(Vector3 releasePosition)
    {
        // Allow XR and physics release processing to finish.
        yield return new WaitForFixedUpdate();
        yield return new WaitForEndOfFrame();

        if (isGrabbed)
        {
            snapCoroutine = null;
            yield break;
        }

        // Restore the intended release position before
        // calculating the nearest grid cell.
        transform.position = releasePosition;

        // Use our existing snapping and occupancy logic.
        conveyorModule.SnapToNearestCell();

        // Synchronize the Rigidbody with the snapped pose.
        rb.position = transform.position;
        rb.rotation = transform.rotation;

        Debug.Log(
            $"{name} final snapped position: {transform.position}"
        );

        snapCoroutine = null;
    }
}
