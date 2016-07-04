using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshCollider))]
public class SimulationInteraction : MonoBehaviour
{
	[SerializeField]
	Simulation _sim;

	//
	// Settings
	// ---
	[SerializeField]
	float _addingSpeed = 100f;

	[SerializeField]
	Brush _addingBrush;

	//
	// Cache
	// ---
	Collider _collider;

	void Start()
	{
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

				Vector4 amount = new Vector4(20f, 20f, 0, 0);
				_sim.AddSource(_addingBrush, hitPos, amount);
			}
		}
	}
}
