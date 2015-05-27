using UnityEngine;

namespace CatchCo.Insula.Puzzle
{
   public class CharacterControllerMover : MonoBehaviour
   {
      #region inspector
      public float MoveSpeed;
      public Vector2 JumpSpeed;
      public ProbeRope Rope;

      /// <summary>
      /// The Euler angle that keyboard arrow input should be rotated by to help with the
      /// iso perspective angle.
      /// </summary>
      public float KeyboardMovementRotation;
      public bool UseMouseFollowOnDesktop;
      public float TouchSmashSpeed;
      public bool DisableKeyboardInput;
      public float TouchJoystickValueScaler = 1f;
      public float JumpTolerance = 0f;
      public LayerMask EnvironmentLayerMask;
      [Range(0,.2f)]
      public float LateralCollisionPenalty = 0f;

      public float MaxVelocity = 10f;
      #endregion

      #region properties
      public delegate void CharacterMoverEventDelegate();

      public event CharacterMoverEventDelegate OnJumpStart = delegate{};
      public event CharacterMoverEventDelegate OnStopPlayerControl = delegate{};
      public event CharacterMoverEventDelegate OnStartPlayerControl = delegate {};

      public CharacterController CharController {get; private set;}
      private Vector3 _movementVector;
      private Vector3 _inputStrength;
      private bool _jumpStarted;
      private float _currentSpeed;
      private bool _isJumping;
      private Vector3 _lastGroundPosition;

      #endregion

      private void Awake()
      {
         CharController = GetComponent<CharacterController>();
      }

      private void OnEnable()
      {
         OnStartPlayerControl();
      }

      private void OnDisable()
      {
         OnStopPlayerControl();
      }

      private void HandleOn_ButtonPress(string buttonName)
      {
         if (buttonName == "Jump")
         {
            _jumpStarted = true;
         }
      }
      

      private void HandleOn_JoystickMove()
      {
         var horizontalAxis = Input.GetAxis("Horizontal");

        var verticalAxis = Input.GetAxis("Vertical");
         var gameCamTx = Camera.main.transform;
         var gameCamForward = gameCamTx.forward.Y(0).normalized;
         var gameCamRight = gameCamTx.right.Y(0).normalized;

         var axisMovement = Vector3.ClampMagnitude(new Vector3(horizontalAxis,verticalAxis, 0) * TouchJoystickValueScaler, 1f);

         var movement = gameCamForward * axisMovement.y;
         movement += gameCamRight * axisMovement.x;
         _inputStrength = movement;

         _currentSpeed = MoveSpeed;
      }

      private Vector2 GetMouseFollowMovement()
      {
         var charPosition = Camera.main.WorldToScreenPoint(transform.position);
         var mousePosition = Input.mousePosition;
         var direction = charPosition.DirTo(mousePosition);
         
         var vertical = Input.GetAxis("Vertical");

         return Vector2.ClampMagnitude(direction * vertical, 1f);
      }

      private Vector2 GetKeyboardArrowMovement()
      {
         var vertical = Input.GetAxis("Vertical");
         var horizontal = Input.GetAxis("Horizontal");

         var movementVector = Vector3.ClampMagnitude(new Vector3(horizontal, vertical, 0), 1f);

         // if we are a perfectly diagonal adjust the rotation
         if (Mathf.Abs(vertical * horizontal) >= .999f)
         {
            movementVector = Quaternion.Euler(0, 0, KeyboardMovementRotation) * movementVector;
         }

         return movementVector;
      }

      private void HandleKeyboardInput()
      {
         #if !UNITY_IOS || UNITY_EDITOR

         if (DisableKeyboardInput)
            return;

         var axisMovement = Vector2.zero;
         if (UseMouseFollowOnDesktop)
         {
            axisMovement = GetMouseFollowMovement();
         }
         else
         {
            axisMovement = GetKeyboardArrowMovement();
         }

         if (axisMovement.sqrMagnitude > .001f)
         {
         var gameCamTx = Camera.main.transform;
            var gameCamForward = gameCamTx.forward.Y(0).normalized;
            var gameCamRight = gameCamTx.right.Y(0).normalized;
            var movement = gameCamForward * axisMovement.y;
            movement += gameCamRight * axisMovement.x;
            _inputStrength = movement;
         }

         _currentSpeed = MoveSpeed;
         _jumpStarted = Input.GetButtonDown("Jump");
         #endif
      }


      public void Update()
      {
         HandleKeyboardInput();

         if (CharController.isGrounded || (CharController.collisionFlags & CollisionFlags.Below) != 0 || (_lastGroundPosition.MagTo(transform.position) < JumpTolerance * _currentSpeed && !_isJumping && _jumpStarted))
         {
            _movementVector = _inputStrength * _currentSpeed;
            _lastGroundPosition = transform.position;

            if (_jumpStarted)
            {
               _movementVector.y = JumpSpeed.y;

               if (_movementVector.Y(0).magnitude > .001)
               {
                  _movementVector += _inputStrength.normalized * JumpSpeed.x;
               }

               _isJumping = true;
               OnJumpStart();
            }
            else
            {
               _movementVector.y = 0;
               _isJumping = false;
            }

            if (_movementVector.Y(0).magnitude > .001f)
            {
               transform.forward = _inputStrength.normalized;
            }

            _inputStrength = Vector3.zero;
         }

         var previousPosition = transform.position;

         // apply gravity
         _movementVector += Physics.gravity * Time.deltaTime;

         // solve the rope with the new distance
         Rope.Solve(_movementVector * Time.deltaTime);

         // the rope may have rotational velocity
         _movementVector += Rope.GetRopeVelocity(_movementVector);

         // likewise the rope might also retract
         _movementVector += Rope.GetRopePullDistance(previousPosition);

         // clamp the velocity to make life more predicatable
         _movementVector = _movementVector.normalized * Mathf.Min(_movementVector.magnitude , MaxVelocity/Time.deltaTime);

         // do the movement
         var collisionFlags= CharController.Move((_movementVector * Time.deltaTime));

         // remove Y collisions from velocity for next calculation
         if ((collisionFlags & (CollisionFlags.Below | CollisionFlags.Above)) != 0 )
         {
            _movementVector.y = 0;
         }

         // slow the XZ movement for collisions
         if ((collisionFlags & CollisionFlags.Sides)!= 0)
         {
         var deltaPosition = transform.position - previousPosition;
            deltaPosition.y = _movementVector.y;
            _movementVector = Vector3.Lerp( _movementVector,  deltaPosition, LateralCollisionPenalty);
         }

         // reset values as touch input could happen at any time
         _jumpStarted = false;
      }
   }
}
