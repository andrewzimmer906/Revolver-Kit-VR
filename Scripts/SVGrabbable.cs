using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


public class SVGrabbable : MonoBehaviour {

    //------------------------
    // Variables
    //------------------------
    public float grabDistance = 1;
	public float grabFlyTime = 2f;
	public bool shouldFly = true;

	[HideInInspector]
	public bool inHand = false;

    private SVOutline outlineComponent;

	private SVControllerInput input;
	private float grabStartTime;
	private Vector3 grabStartPosition;

	private float kickStartTime;
	private float maxKickDuration = 1f;
	private float minKickAngle = .25f;
	private bool isKicking = false;
	private Quaternion kickOffset;

	private Quaternion grabStartRotation;
		
    //------------------------
    // Init
    //------------------------
    void Start () {
        outlineComponent = this.gameObject.GetComponent<SVOutline>();
		this.input = this.gameObject.GetComponent<SVControllerInput> ();
    }

    //------------------------
    // Update
    //------------------------
    void Update() {
		if (!this.input.hasActiveController) {
            this.UngrabbedUpdate();
        } else {
            this.GrabbedUpdate();
        }
    }

    private void UngrabbedUpdate() {
		this.inHand = false;

        float distanceToLeftHand = 1000;
		if (input.LeftHandController != null) {
			distanceToLeftHand = (this.transform.position - input.LeftHandController.transform.position).magnitude;
        }

		float distanceToRightHand = 1000;
		if (input.RightHandController != null) {
			distanceToRightHand = (this.transform.position - input.RightHandController.transform.position).magnitude;
		}

        if (grabDistance > distanceToLeftHand ||
            grabDistance > distanceToRightHand) {
            float distance = Mathf.Min(distanceToLeftHand, distanceToRightHand);
            if (this.outlineComponent) {
                float distanceForHighlight = grabDistance / 4f;
                float highlight = Mathf.Max(0, Mathf.Min(1, (grabDistance - distance) / distanceForHighlight));
                outlineComponent.outlineActive = highlight;
            }

			// order them based on distance
			GameObject firstController = null;
			GameObject secondController = null;

			if (distanceToLeftHand < distanceToRightHand) {
				if (SVControllerManager.nearestGrabbableToLeftController == this)
					firstController = input.LeftHandController;
				
				if (SVControllerManager.nearestGrabbableToRightController == this)
					secondController = input.RightHandController;	
			} else {
				if (SVControllerManager.nearestGrabbableToRightController == this)
					firstController = input.RightHandController;

				if (SVControllerManager.nearestGrabbableToLeftController == this)
					secondController = input.LeftHandController;
			}

			TrySetActiveController (firstController);
			TrySetActiveController (secondController);

			// Update grabbable distance so we always grab the nearest revolver
			if (distanceToLeftHand < SVControllerManager.distanceToLeftController || 
				SVControllerManager.nearestGrabbableToLeftController == null ||
				SVControllerManager.nearestGrabbableToLeftController == this) {
				SVControllerManager.nearestGrabbableToLeftController = this;
				SVControllerManager.distanceToLeftController = distanceToLeftHand;
			}

			if (distanceToRightHand < SVControllerManager.distanceToRightController || 
				SVControllerManager.nearestGrabbableToRightController == null ||
				SVControllerManager.nearestGrabbableToRightController == this) {
				SVControllerManager.nearestGrabbableToRightController = this;
				SVControllerManager.distanceToRightController = distanceToRightHand;
			}
				
        } else {
            outlineComponent.outlineActive = 0;
        }
    }

    private void GrabbedUpdate() {
		if (input.gripAutoHolds) {
			if (input.GetReleaseGripButtonPressed (input.activeController)) {
				this.ClearActiveController ();
				return;
			}
		} else if (!input.GetGripButtonDown(input.activeController)) {
			this.ClearActiveController ();
			return;
        }

		float percComplete = (Time.time - this.grabStartTime) / this.grabFlyTime;
		if (percComplete < 1 && this.shouldFly) {
			this.inHand = false;
			transform.position = Vector3.Lerp (this.grabStartPosition, this.input.activeController.transform.position, percComplete);
			transform.rotation = Quaternion.Lerp (this.grabStartRotation, this.input.activeController.transform.rotation, percComplete);
		} else if (isKicking) {
			this.kickOffset = Quaternion.Lerp (this.kickOffset, Quaternion.identity, 0.05f);
			this.transform.SetPositionAndRotation (this.input.activeController.transform.position, this.input.activeController.transform.rotation * this.kickOffset);

			float curAngle = Quaternion.Angle (this.kickOffset, Quaternion.identity);
			if (curAngle < minKickAngle || Time.time - this.kickStartTime > maxKickDuration) {
				this.isKicking = false;
			}
		} else {
			this.inHand = true;
			this.transform.SetPositionAndRotation(this.input.activeController.transform.position, this.input.activeController.transform.rotation);
		}
    }

	//------------------------
	// Kick
	//------------------------
	public void EditGripForKick(float kickForce) {
		kickStartTime = Time.time;
		isKicking = true;

		Quaternion upRotation = Quaternion.AngleAxis (-90, Vector3.right);
		this.kickOffset = Quaternion.RotateTowards (Quaternion.identity, upRotation, kickForce);
	}

    //------------------------
    // State Changes
    //------------------------
	private void TrySetActiveController(GameObject controller) {
		if (this.input.hasActiveController)
			return;	

		if (controller == null)
			return;

		if (input.gripAutoHolds) {
			if (!input.GetGripButtonPressed(controller)) {
				return;
			}
		} else {
			if (!input.GetGripButtonDown (controller)) {
				return;
			}
		}

		if (this.input.SetActiveController (controller)) {
			this.grabStartTime = Time.time;
			this.grabStartPosition = this.gameObject.transform.position;
			this.grabStartRotation = this.gameObject.transform.rotation;
			outlineComponent.outlineActive = 0;
		
			Rigidbody rigidbody = this.GetComponent<Rigidbody> ();
			rigidbody.isKinematic = true;

			// hide the controller model
			this.input.activeRenderModel.gameObject.SetActive (false);
		}
    }

	private void ClearActiveController() {
		Rigidbody rigidbody = this.GetComponent<Rigidbody> ();
		rigidbody.isKinematic = false;
		rigidbody.velocity = this.input.activeControllerDevice.velocity;
		rigidbody.angularVelocity =  this.input.activeControllerDevice.angularVelocity;

		// Show the render model
		this.input.activeRenderModel.gameObject.SetActive (true);

		this.input.ClearActiveController ();
	}
}
