using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public enum SVInputButton {
	SVButton_None = -1,
	SVButton_System = Valve.VR.EVRButtonId.k_EButton_System,
	SVButton_Menu = Valve.VR.EVRButtonId.k_EButton_ApplicationMenu,
	SVButton_Grip = Valve.VR.EVRButtonId.k_EButton_Grip,
	SVButton_DPad_Left = Valve.VR.EVRButtonId.k_EButton_DPad_Left,
	SVButton_DPad_Right = Valve.VR.EVRButtonId.k_EButton_DPad_Right,
	SVButton_DPad_Down = Valve.VR.EVRButtonId.k_EButton_DPad_Down,
	SVButton_DPad_Up = Valve.VR.EVRButtonId.k_EButton_DPad_Up,
	SVButton_A = Valve.VR.EVRButtonId.k_EButton_A,
	SVButton_Touchpad = Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad,
	SVButton_Trigger = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger
};
	

public class SVControllerInput : MonoBehaviour {

	//------------------------
	// Variables
	//------------------------
	[Space(15)]
	[Header("Grip Settings")]
	public SVInputButton gripButton = SVInputButton.SVButton_Grip;
	public SVInputButton releaseGripButton = SVInputButton.SVButton_None;
	public bool gripAutoHolds = false;

	[Space(15)]
	[Header("Firing Settings")]
	public SVInputButton triggerButton = SVInputButton.SVButton_Trigger;

	[Space(15)]
	[Header("Reload Settings")]
	public SVInputButton openBarrelButton = SVInputButton.SVButton_None;
	public SVInputButton closeBarrelButton = SVInputButton.SVButton_None;

	public bool openWithPhysics = true;
	public float openAcceleration = 4f;
	public float openEmptyAcceleration = 2f;
	public float closeAcceleration = -2f;

	//------------------------
	// Private Variables
	//------------------------
	[HideInInspector]
	public GameObject activeController;
	[HideInInspector]
	public SteamVR_Controller.Device activeControllerDevice;
	[HideInInspector]
	public SteamVR_RenderModel activeRenderModel;
	[HideInInspector]
	public bool activeControllerIsRight = false;
	[HideInInspector]
	public bool hasActiveController = false;

	private float rumbleStrength = 1f;



	private SteamVR_ControllerManager controllerManager;

	//------------------------
	// Setup
	//------------------------
	void Start() {
		controllerManager = Object.FindObjectOfType<SteamVR_ControllerManager> ();
		Assert.IsNotNull (controllerManager, "SVControllerInput Needs a SteamVR_ControllerManager in the scene to function correctly.");
	}

	//------------------------
	// Getters
	//------------------------
	private SteamVR_Controller.Device Controller(SteamVR_TrackedObject trackedObject) {
		return SteamVR_Controller.Input((int)trackedObject.index);
	}

	public GameObject LeftHandController {
		get {
			if (controllerManager.left != null &&
			    controllerManager.left.activeInHierarchy) {
				return controllerManager.left;
			}
			return null;
		}
	}

	public GameObject RightHandController {
		get {
			if (controllerManager.right != null &&
				controllerManager.right.activeInHierarchy) {
				return controllerManager.right;
			}
			return null;
		}
	}

	//------------------------
	// Input Checkers
	//------------------------

	public bool GetGripButtonDown(GameObject controller) {
		return this.GetButtonDown (controller, this.gripButton);
	}

	public bool GetGripButtonPressed(GameObject controller) {
		return this.GetButtonPressDown (controller, this.gripButton);
	}

	public bool GetReleaseGripButtonPressed(GameObject controller) {
		return this.GetButtonPressDown (controller, this.releaseGripButton);
	}

	public bool GetTriggerButtonPressed(GameObject controller) {
		return this.GetButtonPressDown (controller, this.triggerButton);
	}

	public bool GetOpenBarrelPressed(GameObject controller) {
		return this.GetButtonPressDown (controller, this.openBarrelButton);
	}

	public bool GetCloseBarrelPressed(GameObject controller) {
		return this.GetButtonPressDown (controller, this.closeBarrelButton);
	}

	//------------------------
	// Public
	//------------------------

	public bool GetButtonDown(GameObject controller, SVInputButton button) {
		if (button == SVInputButton.SVButton_None)
			return false;
		return Controller (controller.GetComponent<SteamVR_TrackedObject>()).GetPress((Valve.VR.EVRButtonId)button); //TODO : Cache this component for performance
	}

	public bool GetButtonPressDown(GameObject controller, SVInputButton button) {
		if (button == SVInputButton.SVButton_None)
			return false;
		return Controller (controller.GetComponent<SteamVR_TrackedObject>()).GetPressDown((Valve.VR.EVRButtonId)button); //TODO : Cache this component for performance
	}

	public bool SetActiveController(GameObject activeController) {
		if (activeController == SVControllerManager.activeLeftController ||
		    activeController == SVControllerManager.activeRightController) {
			return false;
		}

		this.activeController = activeController;
		this.activeControllerDevice = Controller (activeController.GetComponent<SteamVR_TrackedObject>());  //TODO : Cache this component for performance
		this.activeRenderModel = this.activeController.GetComponentInChildren<SteamVR_RenderModel>();
		this.activeControllerIsRight = (controllerManager.right == activeController);

		if (this.activeControllerIsRight) {
			SVControllerManager.activeRightController = this.activeController;
		} else {
			SVControllerManager.activeLeftController = this.activeController;
		}

		hasActiveController = true;

		return true;
	}

	public void ClearActiveController() {
		hasActiveController = false;

		this.activeController = null;
		this.activeControllerDevice = null;
		this.activeRenderModel = null;

		if (this.activeControllerIsRight) {
			SVControllerManager.activeRightController = null;
		} else {
			SVControllerManager.activeLeftController = null;
		}
	}

	public void RumbleActiveController(float rumbleLength) {
		if (activeControllerDevice != null) {
			StartCoroutine( LongVibration(activeControllerDevice, rumbleLength, rumbleStrength) );
		}
	}

	//------------------------
	// Haptics
	//------------------------

	//length is how long the vibration should go for
	//strength is vibration strength from 0-1
	private IEnumerator LongVibration(SteamVR_Controller.Device device, float totalLength, float strength) {
		ushort rLength = (ushort)Mathf.Lerp (0, 3999, strength);
		for (float i = 0f; i < totalLength; i += Time.deltaTime) {
			device.TriggerHapticPulse(rLength);
			yield return null;
		}
	}

	//vibrationCount is how many vibrations
	//vibrationLength is how long each vibration should go for
	//gapLength is how long to wait between vibrations
	//strength is vibration strength from 0-1
	IEnumerator LongVibration(SteamVR_Controller.Device device, int vibrationCount, float vibrationLength, float gapLength, float strength) {
		strength = Mathf.Clamp01(strength);
		for(int i = 0; i < vibrationCount; i++) {
			if(i != 0) yield return new WaitForSeconds(gapLength);
			yield return StartCoroutine(LongVibration(device, vibrationLength, strength));
		}
	}
}
