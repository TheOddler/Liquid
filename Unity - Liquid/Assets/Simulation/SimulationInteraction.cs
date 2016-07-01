using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Simulation))]
public class SimulationInteraction : MonoBehaviour
{
	//
	// Settings
	// ---
	[SerializeField]
	float _addingSpeed = 1000f;

	[SerializeField]
	Brush _addingBrush;

	//
	// Other
	// ---
	Simulation _sim;
	Collider _collider;

	void Start()
	{
		_sim = GetComponent<Simulation>();
		_collider = GetComponent<Collider>();
	}

	void Update()
	{
		bool addWater = Input.GetMouseButton(0);
		bool addSand = Input.GetMouseButton(1);
		if (addWater || addSand)
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hitInfo;
			if (_collider.Raycast(ray, out hitInfo, float.PositiveInfinity))
			{
				Vector2 hitPos = hitInfo.textureCoord;

				Vector4 amount = new Vector4(
					addWater ? _addingSpeed : 0f, // water
					addSand ? _addingSpeed : 0f, // sand
					0, 0);

				_sim.AddSource(_addingBrush, hitPos, amount * Time.deltaTime);
			}
		}

		if (Input.GetKeyDown(KeyCode.Space))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hitInfo;
			if (_collider.Raycast(ray, out hitInfo, float.PositiveInfinity))
			{
				Vector2 hitPos = hitInfo.textureCoord;

				Vector4 amount = new Vector4(100f, 100f, 0, 0);
				_sim.AddSource(_addingBrush, hitPos, amount);
			}
		}
	}
}
