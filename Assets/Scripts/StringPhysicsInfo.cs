using UnityEngine;

namespace CatchCo.Insula.Puzzle
{
   public class StringPhysicsInfo
   {
      public Vector3 Position;

      public bool IsDirty { get; set; }

      private StringPhysicsConfig _config;
      private Vector3 _pos_frame;
      private Vector3 _pos_next;
      private Vector3 _pos_prev;
      private Vector3 _force;
      private Vector3 _velocity;
      private float _dt;

      public void Init(Vector3 inPosition)
      {
         Position = inPosition;
         _pos_frame = Position;
         _dt = 0;
         _velocity = Vector3.zero;
         _force = Vector3.zero;
      }

      public void ResetForNewFrame(StringPhysicsConfig inConfig, Vector3 inPrevPosition, Vector3 inNextPosition, float inDeltaTime)
      {
         _config = inConfig;
         _force = Vector3.zero;
         _pos_next = inNextPosition;
         _pos_prev = inPrevPosition;
         _dt = inDeltaTime;
         _pos_frame = Position;
         IsDirty = false;
      }

      public void SolveSpringForcesWithRotationLerping()
      {
         var vec_current = _pos_prev.VecTo(_pos_frame);
         var vec_dest = _pos_prev.VecTo(_pos_next);
         vec_dest = vec_dest.normalized * vec_current.magnitude;
         var vec_new = Vector3.RotateTowards(vec_current, vec_dest, _config.RotationStrength * Mathf.PI * _dt, _config.RotationStrength * 100f);
         var pos_new = _pos_prev + vec_new;

         _force = (_velocity.VecTo(_pos_frame.VecTo(pos_new) / (_dt)) / _dt) * StringMass();
      }

      public void SolveSpringForcesWithPhysics()
      {
         var vec = _pos_frame.VecTo(_pos_next);
         var dir = vec.normalized;
         _force += _config.Stiffness * (vec.magnitude) * dir;

         // make sure the revese uses the same force but it's own direction
//            vec = _pos_frame.VecTo(_pos_prev);
         dir = _pos_frame.DirTo(_pos_prev);
         _force += _config.Stiffness * (vec.magnitude) * dir;
      }

      public void SolveSpringForces()
      {
         if (_config.UseRotationLerping)
            SolveSpringForcesWithRotationLerping();
         else
            SolveSpringForcesWithPhysics();
      }

      public void Step()
      {
         var mass = StringMass();
         //         _forces += Physics.gravity * mass;
         _velocity += (_force / mass) * _dt;
         _force += -_velocity * _config.Friction;
         _velocity = _velocity.normalized * Mathf.Min(_velocity.magnitude, _config.MaxVelocity);
            
         _pos_frame += _velocity * _dt;
      }

      public void ClearVelocity()
      {
         _velocity = Vector3.zero;
      }

      public Vector3 GetFramePosition()
      {
         return _pos_frame;
      }

      public void UpdatePosition()
      {
         _velocity = Position.VecTo(_pos_frame) / _dt;
         IsDirty = true;
         Position = _pos_frame;
      }

      private float StringMass()
      {
         var length = _pos_frame.MagTo(_pos_next) + _pos_frame.MagTo(_pos_prev);
         return length * _config.MassPerMeter;
      }

      public void ResolveCollisionAtPosition(Vector3 inPosition)
      {
         var vec = _pos_frame.VecTo(inPosition);
         _pos_frame = inPosition;
         _velocity += vec / _dt;
      }
   }
   
}
