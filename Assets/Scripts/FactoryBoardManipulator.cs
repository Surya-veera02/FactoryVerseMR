
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRSimpleInteractable))]
public class FactoryBoardManipulator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform boardRoot;
    [SerializeField] private Transform headTransform;

    [Header("Scale Limits")]
    [SerializeField] private float minScale = 0.75f;
    [SerializeField] private float maxScale = 1.35f;

    [Header("Movement Limits")]
    [SerializeField] private float minHorizontalDistance = 0.35f;
    [SerializeField] private float maxHorizontalDistance = 1.35f;
    [SerializeField] private float minBelowEyes = 0.35f;
    [SerializeField] private float maxBelowEyes = 1.25f;

    private XRSimpleInteractable grab;
    private readonly List<IXRSelectInteractor> hands =
        new List<IXRSelectInteractor>();

    private Vector3 startBoardPosition;
    private Quaternion startBoardRotation;
    private float startBoardScale;

    private Vector3 startAnchor;
    private Quaternion startHandRotation;
    private Vector3 startTwoHandDirection;
    private float startHandDistance;

    private void Awake()
    {
        grab = GetComponent<XRSimpleInteractable>();
        if (boardRoot == null)
            boardRoot = transform.parent;
    }

    private void OnEnable()
    {
        grab.selectEntered.AddListener(OnSelected);
        grab.selectExited.AddListener(OnDeselected);
    }

    private void OnDisable()
    {
        grab.selectEntered.RemoveListener(OnSelected);
        grab.selectExited.RemoveListener(OnDeselected);
        hands.Clear();
    }

    private Transform GetHand(IXRSelectInteractor interactor)
    {
        return interactor.GetAttachTransform(grab);
    }

    private void OnSelected(SelectEnterEventArgs args)
    {
        if (hands.Contains(args.interactorObject) ||
            hands.Count >= 2)
            return;

        hands.Add(args.interactorObject);

        CaptureStartingPose();

        Debug.Log(
            $"BOARD GRAB: {hands.Count} hand(s) selected."
        );
    }

    private void OnDeselected(SelectExitEventArgs args)
    {
        if (!hands.Remove(args.interactorObject))
            return;

        CaptureStartingPose();
    }

    private void CaptureStartingPose()
    {
        if (boardRoot == null || hands.Count == 0)
            return;

        startBoardPosition = boardRoot.position;
        startBoardRotation = boardRoot.rotation;
        startBoardScale = boardRoot.localScale.x;

        Transform first = GetHand(hands[0]);

        if (first == null)
            return;

        if (hands.Count == 1)
        {
            startAnchor = first.position;
            startHandRotation = first.rotation;
        }
        else
        {
            Transform second = GetHand(hands[1]);

            if (second == null)
                return;

            startAnchor =
                (first.position + second.position) * 0.5f;

            Vector3 direction =
                Vector3.ProjectOnPlane(
                    second.position - first.position,
                    Vector3.up
                );

            startTwoHandDirection = direction.normalized;

            startHandDistance = Vector3.Distance(
                first.position,
                second.position
            );
        }
    }

    private void LateUpdate()
    {
        if (hands.Count == 0 ||
            boardRoot == null ||
            headTransform == null)
            return;

        Transform first = GetHand(hands[0]);

        if (first == null)
            return;

        Vector3 currentAnchor;
        float targetScale = startBoardScale;
        float yawAngle = 0f;

        if (hands.Count == 1)
        {
            // One hand: move and rotate around world Y.
            currentAnchor = first.position;

            Vector3 initialForward =
                Vector3.ProjectOnPlane(
                    startHandRotation * Vector3.forward,
                    Vector3.up
                );

            Vector3 currentForward =
                Vector3.ProjectOnPlane(
                    first.forward,
                    Vector3.up
                );

            // Use the hand's right axis if its forward
            // direction is nearly vertical.
            if (initialForward.sqrMagnitude < 0.001f)
            {
                initialForward =
                    Vector3.ProjectOnPlane(
                        startHandRotation * Vector3.right,
                        Vector3.up
                    );

                currentForward =
                    Vector3.ProjectOnPlane(
                        first.right,
                        Vector3.up
                    );
            }

            if (initialForward.sqrMagnitude > 0.001f &&
                currentForward.sqrMagnitude > 0.001f)
            {
                yawAngle = Vector3.SignedAngle(
                    initialForward,
                    currentForward,
                    Vector3.up
                );
            }
        }
        else
        {
            // Two hands: move, rotate and scale.
            Transform second = GetHand(hands[1]);

            if (second == null)
                return;

            currentAnchor =
                (first.position + second.position) * 0.5f;

            float currentDistance = Vector3.Distance(
                first.position,
                second.position
            );

            // Allow scaling even when the hands start fairly close.
            if (startHandDistance >= 0.03f)
            {
                float scaleFactor =
                    currentDistance / startHandDistance;

                targetScale = Mathf.Clamp(
                    startBoardScale * scaleFactor,
                    minScale,
                    maxScale
                );
            }

            Vector3 currentDirection =
                Vector3.ProjectOnPlane(
                    second.position - first.position,
                    Vector3.up
                );

            if (startTwoHandDirection.sqrMagnitude > 0.001f &&
                currentDirection.sqrMagnitude > 0.001f)
            {
                yawAngle = Vector3.SignedAngle(
                    startTwoHandDirection,
                    currentDirection,
                    Vector3.up
                );
            }
        }

        // Rotate only around the vertical axis.
        Quaternion targetRotation =
            Quaternion.AngleAxis(yawAngle, Vector3.up) *
            startBoardRotation;

        // Preserve the hand's original grip position
        // relative to the board while moving and scaling.
        Vector3 localAnchorOffset =
            Quaternion.Inverse(startBoardRotation) *
            (startAnchor - startBoardPosition);

        float scaleRatio =
            targetScale / Mathf.Max(startBoardScale, 0.001f);

        Vector3 targetPosition =
            currentAnchor -
            targetRotation * (localAnchorOffset * scaleRatio);

        // Limit horizontal distance from the player's head.
        Vector3 horizontal =
            Vector3.ProjectOnPlane(
                targetPosition - headTransform.position,
                Vector3.up
            );

        if (horizontal.sqrMagnitude < 0.0001f)
        {
            horizontal = Vector3.ProjectOnPlane(
                headTransform.forward,
                Vector3.up
            ).normalized * minHorizontalDistance;
        }

        horizontal = horizontal.normalized *
            Mathf.Clamp(
                horizontal.magnitude,
                minHorizontalDistance,
                maxHorizontalDistance
            );

        targetPosition.x =
            headTransform.position.x + horizontal.x;

        targetPosition.z =
            headTransform.position.z + horizontal.z;

        // Keep the board at a comfortable height.
        targetPosition.y = Mathf.Clamp(
            targetPosition.y,
            headTransform.position.y - maxBelowEyes,
            headTransform.position.y - minBelowEyes
        );

        boardRoot.SetPositionAndRotation(
            targetPosition,
            targetRotation
        );

        boardRoot.localScale = Vector3.one * targetScale;
    }
}
