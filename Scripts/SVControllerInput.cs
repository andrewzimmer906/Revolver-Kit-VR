#define USES_STEAM_VR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public enum SVControllerType {
	SVController_None,
	SVController_Left,
	SVController_Right
};

#if USES_STEAM_VR
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
#else
public enum SVInputButton {
	SVButton_None = -1,
	SVButton_System = 0,
	SVButton_Menu,
	SVButton_Grip,
	SVButton_DPad_Left,
	SVButton_DPad_Right,
	SVButton_DPad_Down,
	SVButton_DPad_Up,
	SVButton_A,
	SVButton_Touchpad,
	SVButton_Trigger,
};
#endif

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
	// Variables
	//------------------------
	[HideInInspector]
	public SVControllerType activeController;

	#if USES_STEAM_VR
	[HideInInspector]
	public SteamVR_Controller.Device activeControllerDevice;
	[HideInInspector]
	public SteamVR_RenderModel activeRenderModel;
	private SteamVR_ControllerManager controllerManager;
	#endif

	private float rumbleStrength = 1f;

	//------------------------
	// Setup
	//------------------------
	void Start() {
		#if USES_STEAM_VR
		controllerManager = Object.FindObjectOfType<SteamVR_ControllerManager> ();
		Assert.IsNotNull (controllerManager, "SVControllerInput (with SteamVR) Needs a SteamVR_ControllerManager in the scene to function correctly.");
		#endif
	}

	//------------------------
	// Getters
	//------------------------
	#if USES_STEAM_VR
	private GameObject SteamController(SVControllerType type) {
		return (type == SVControllerType.SVController_Left ? controllerManager.left : controllerManager.right); //TODO : Cache this component for performance
	}

	private SteamVR_Controller.Device Controller(SVControllerType type) {
		GameObject steamController = (type == SVControllerType.SVController_Left ? controllerManager.left : controllerManager.right); //TODO : Cache this component for performance
		return SteamVR_Controller.Input((int)steamController.GetComponent<SteamVR_TrackedObject>().index);
	}
	#endif

	public bool LeftControllerIsConnected {
		get {
#if USES_STEAM_VR
			return (controllerManager.left != null &&
			controllerManager.left.activeInHierarchy);
#elif USES_OPEN_VR
			return (OVRInput.GetConnectedControllers() & OVRInput.Controller.LTouch);
#endif
		}
	}

	public bool RightControllerIsConnected {
		get {
#if USES_STEAM_VR
			return (controllerManager.right != null &&
			controllerManager.right.activeInHierarchy);
#elif USES_OPEN_VR
			return (OVRInput.GetConnectedControllers() & OVRInput.Controller.RTouch);
#endif
		}
	}

	public Vector3 LeftControllerPosition {
		get {
			#if USES_STEAM_VR
			if (this.LeftControllerIsConnected) {
				return controllerManager.left.transform.position;
			}
			#elif USES_OPEN_VR
			OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
			#endif
			return Vector3.negativeInfinity;
		}
	}

	public Vector3 RightControllerPosition {
		get {
			
			#if USES_STEAM_VR
			if (this.RightControllerIsConnected) {
				return controllerManager.right.transform.position;
			}
			#elif USES_OPEN_VR
			OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
			#endif
			return Vector3.negativeInfinity;
		}
	}

	public Quaternion LeftControllerRotation {
		get {
			if (this.LeftControllerIsConnected) {
				#if USES_STEAM_VR
				return controllerManager.left.transform.rotation;
				#elif USES_OPEN_VR
				OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
				#endif
			}

			return Quaternion.identity;
		}
	}

	public Quaternion RightControllerRotation {
		get {
			if (this.RightControllerIsConnected) {
				#if USES_STEAM_VR
				return controllerManager.right.transform.rotation;
				#elif USES_OPEN_VR
				OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
				#endif
			}

			return Quaternion.identity;
		}
	}

	//------------------------
	// Controller Info
	//------------------------
	public Vector3 PositionForController(SVControllerType controller) {
		if (controller == SVControllerType.SVController_Left) {
			return LeftControllerPosition;
		} else if (controller == SVControllerType.SVController_Right) {
			return RightControllerPosition;
		}

		return Vector3.negativeInfinity;
	}

	public Quaternion RotationForController(SVControllerType controller) {
		if (controller == SVControllerType.SVController_Left) {
			return LeftControllerRotation;
		} else if (controller == SVControllerType.SVController_Right) {
			return RightControllerRotation;
		}

		return Quaternion.identity;
	}

	public bool ControllerIsConnected(SVControllerType controller) {
		if (controller == SVControllerType.SVController_Left) {
			return LeftControllerIsConnected;
		} else if (controller == SVControllerType.SVController_Right) {
			return RightControllerIsConnected;
		}

		return false;
	}

	//------------------------
	// Input Checkers
	//------------------------

	public bool GetGripButtonDown(SVControllerType controller) {
		return this.GetButtonDown (controller, this.gripButton);
	}

	public bool GetGripButtonPressed(SVControllerType controller) {
		return this.GetButtonPressDown (controller, this.gripButton);
	}

	public bool GetReleaseGripButtonPressed(SVControllerType controller) {
		return this.GetButtonPressDown (controller, this.releaseGripButton);
	}

	public bool GetTriggerButtonPressed(SVControllerType controller) {
		return this.GetButtonPressDown (controller, this.triggerButton);
	}

	public bool GetOpenBarrelPressed(SVControllerType controller) {
		return this.GetButtonPressDown (controller, this.openBarrelButton);
	}

	public bool GetCloseBarrelPressed(SVControllerType controller) {
		return this.GetButtonPressDown (controller, this.closeBarrelButton);
	}

	//------------------------
	// Public
	//------------------------

	public bool GetButtonDown(SVControllerType controller, SVInputButton button) {
		if (button == SVInputButton.SVButton_None || !ControllerIsConnected(controller))
			return false;
		
		#if USES_STEAM_VR
		return Controller(controller).GetPress((Valve.VR.EVRButtonId)button);
		#else
		return false;
		#endif
	}

	public bool GetButtonPressDown(SVControllerType controller, SVInputButton button) {
		if (button == SVInputButton.SVButton_None || !ControllerIsConnected(controller))
			return false;
		
		#if USES_STEAM_VR
		return Controller(controller).GetPressDown((Valve.VR.EVRButtonId)button);
		#else
		return false;
		#endif
	}

	public bool SetActiveController(SVControllerType activeController) {
		if (activeController == SVControllerType.SVController_Left &&
		    SVControllerManager.leftControllerActive) {
			return false;
		}

		if (activeController == SVControllerType.SVController_Right &&
			SVControllerManager.rightControllerActive) {
			return false;
		}

		this.activeController = activeController;

		#if USES_STEAM_VR
		this.activeControllerDevice = Controller (activeController);
		this.activeRenderModel = SteamController(this.activeController).GetComponentInChildren<SteamVR_RenderModel>();
		#endif

		if (this.activeController == SVControllerType.SVController_Right) {
			SVControllerManager.rightControllerActive = true;
		} else {
			SVControllerManager.leftControllerActive = true;
		}
			
		return true;
	}

	public void ClearActiveController() {
		#if USES_STEAM_VR
		this.activeControllerDevice = null;
		this.activeRenderModel = null;
		#endif

		if (this.activeController == SVControllerType.SVController_Right) {
			SVControllerManager.rightControllerActive = false;
		} else {
			SVControllerManager.leftControllerActive = false;
		}

		this.activeController = SVControllerType.SVController_None;
	}

	public void RumbleActiveController(float rumbleLength) {
		#if USES_STEAM_VR
		if (activeControllerDevice != null) {
			StartCoroutine( LongVibration(activeControllerDevice, rumbleLength, rumbleStrength) );
		}
		#endif
	}

	public Vector3 ActiveControllerVelocity() {
		#if USES_STEAM_VR
		return this.activeControllerDevice.velocity;
		#endif

		return Vector3.zero;
	}

	public Vector3 ActiveControllerAngularVelocity() {
		#if USES_STEAM_VR
		return this.activeControllerDevice.angularVelocity;
		#endif

		return Vector3.zero;
	}

	//------------------------
	// Visibility
	//------------------------
	public void HideActiveModel() {
		#if USES_STEAM_VR
		this.activeRenderModel.gameObject.SetActive (false);
		#endif
	}

	public void ShowActiveModel() {
		#if USES_STEAM_VR
		this.activeRenderModel.gameObject.SetActive (true);
		#endif
	}

	//------------------------
	// Haptics
	//------------------------
	#if USES_STEAM_VR
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
	#endif
}
