using UnityEngine;
using System.Collections;

public class STM_PaintCamera : MonoBehaviour {
	private int yMinLimit = 0, yMaxLimit = 80;
	private Quaternion currentRotation, desiredRotation, rotation;
	private float yDeg=15, xDeg=0.0f;
	private float currentDistance,desiredDistance=3.0f,maxDistance = 6.0f,minDistance = 9.0f;
	private Vector3 position;
	public GameObject targetObject,camObject;
	float sensitivity=1.25f;

	public bool _isOnRotate;

	void Start () {
		currentDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
	}
	
	// Update is called once per frame
	void Update () {
		CameraControlUpdate ();
	}
	void CameraControlUpdate(){			

		if (Application.isEditor) {

			float vertAxis =Input.GetAxis ("Vertical"); 
			float horAxis = Input.GetAxis ("Horizontal");
			
			if(vertAxis != 0 ||horAxis != 0 )
				_isOnRotate = true;
			else 
				_isOnRotate = false;
			yDeg += vertAxis * sensitivity;
			xDeg -= horAxis * sensitivity;


		} else {
			
			_isOnRotate = false;
			if (Input.touchCount > 1) {
				_isOnRotate = true;
				Touch touch = Input.GetTouch (0);     
				yDeg += touch.deltaPosition.x  * sensitivity * 10;
				xDeg -= touch.deltaPosition.y * sensitivity * 10;
			}
		}


		//yDeg = ClampAngle(yDeg, yMinLimit, yMaxLimit);		
		desiredRotation = Quaternion.Euler(yDeg, xDeg, 0);		
		rotation = Quaternion.Lerp(targetObject.transform.rotation, desiredRotation, 0.05f  );
		targetObject.transform.rotation = desiredRotation;
		desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
		currentDistance = Mathf.Lerp(currentDistance, desiredDistance, 0.05f  );
		position = targetObject.transform.position - (rotation * Vector3.forward * currentDistance );
		/*Vector3 lerpedPos=Vector3.Lerp(camObject.transform.position,position,0.05f);
		camObject.transform.position = lerpedPos;*/
		
	}
	
	private static float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360)
			angle += 360;
		if (angle > 360)
			angle -= 360;
		return Mathf.Clamp(angle, min, max);
	}


}
	