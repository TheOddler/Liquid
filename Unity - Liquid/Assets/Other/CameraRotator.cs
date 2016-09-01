using UnityEngine;
using System.Collections;

public class CameraRotator : MonoBehaviour
{
	[SerializeField]
	float _autoSpeed = 30f;

	[SerializeField]
	float _speed = 30f;

	// Update is called once per frame
	void Update ()
	{
		float rot = _autoSpeed;

		rot -= Input.GetAxis("Horizontal") * _speed;

		rot *= Time.deltaTime;
		transform.Rotate(0, rot, 0);
	}

	public void SetAutoRotationSpeed(float speed)
	{
		_autoSpeed = speed;
	}
}
