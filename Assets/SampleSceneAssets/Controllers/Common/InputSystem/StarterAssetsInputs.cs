using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector3 move;
		public Vector2 look;

		public bool sprint;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

		void Update()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				Cursor.lockState = CursorLockMode.None;
			}
			else if (Input.GetMouseButtonDown(0))
			{
				Cursor.lockState = CursorLockMode.Locked;
			}
		}

		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector3>());
		}

		public void OnLook(InputValue value)
		{
			if (cursorInputForLook && Cursor.lockState == CursorLockMode.Locked)
			{
				LookInput(value.Get<Vector2>());
			}
			else
			{
				LookInput(Vector2.zero);
			}
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}


		public void MoveInput(Vector3 newMoveDirection)
		{
			move = newMoveDirection;
		}

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}

}
