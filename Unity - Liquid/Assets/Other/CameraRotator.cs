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

	void OnGUI()
	{
		GUI.Label(new Rect(10, 10, 300, 20), "Auto-rotate speed:");
		_autoSpeed = GUI.HorizontalSlider(new Rect(10, 30, 100, 20), _autoSpeed, 0, 60);
	}
}
