using UnityEngine;
using System.Collections.Generic;

namespace CatchCo.Insula.Puzzle
{
   public class ProbeRope : MonoBehaviour
   {
      public Transform StartTx;
      public Transform EndTx;
      public float Radius;
      public LayerMask HitLayerMask;
      public LineRenderer Line;
      public float SkinWidth;
      public bool DoStringPhysics;
      public StringPhysicsConfig Config;
      public float SweepIterations = 6;
      public float CollapseDistance = .01f;
      public int UpdateLoopCap = 100;
      public float AttachmentHeight;
      public float RotationalFriction = .005f;
      [Range(0, 1f)]
      public float Springiness = 0f;

      [ReadOnlyPropertyAttribute]
      public int PointsCount;
      public float MaxLength = 1f;
      private bool _ropeIsPlanted;
      private List<Corner> _corners;
      private Stack<Corner> _pool;
      private Transform _tx;

      [ReadOnlyPropertyAttribute]
      private float _ropeLength;
      private RotationMovement _rotMovement;


      #region types
      private class Corner
      {
         public Vector3 LastPoint;
         public Vector3 Point;
         public Vector3 Normal;
         public StringPhysicsInfo Physx;

         public Corner()
         {
            Physx = new StringPhysicsInfo();
         }

         public Corner(Vector3 inPoint)
         {
            LastPoint = Point = inPoint;
            Physx = new StringPhysicsInfo();
         }

         public void Reset(Vector3 inPoint, Vector3 inNorm)
         {
            LastPoint = Point = inPoint;
            Normal = inNorm;
            Physx.Init(inPoint);
         }
      }

      private struct RotationMovement
      {
         public float Radius;
         public Vector3 Dir;
         public Vector3 Normal;
         public bool IsRotating;
         public Vector3 Velocity;

         public Vector3 Tangent()
         {
            return Quaternion.AngleAxis(90, Normal) * Dir;
         }

      }
      #endregion

      #region MonoBehaviour
      private void Awake()
      {
         _tx = transform;
      }

      private void Start()
      {
         _corners = new List<Corner>(UpdateLoopCap);
         _pool = new Stack<Corner>(UpdateLoopCap);
         for (int i = 0; i < UpdateLoopCap; i++)
         {
            _pool.Push(new Corner());
         }
         ClearRope();
       
      }

      public void Update()
      {
         CheckInput();
      }

      public void LateUpdate()
      {
         DrawRope();
      }
      #endregion

      #region input
      private void HandleOn_ButtonPress(string buttonName)
      {
         if (buttonName == "Rope")
         {
            ToggleRope();
         }
      }
      
      private void CheckInput()
      {
         #if !UNITY_IOS || UNITY_EDITOR
         if (Input.GetButtonDown("Rope"))
         {
            ToggleRope();
         }
         #endif
      }
      #endregion

      private void ToggleRope()
      {
         if (_ropeIsPlanted)
         {
            ClearRope();
            _ropeIsPlanted = false;
         }
         else
         {
            _ropeIsPlanted = TryStartNewRope();
         }
      }

      private bool TryStartNewRope()
      {
         var position = _tx.position+Vector3.up * .25f;
         var ray = new Ray(position, Vector3.down);
         RaycastHit hit;
         if (Physics.SphereCast(ray,.1f, out hit, .5f, HitLayerMask))
         {
            var normal = hit.point.DirTo(position);
            StartTx.parent = _tx.parent;
            StartTx.position = hit.point + normal * AttachmentHeight;
            AddPoint(StartTx.position, Vector3.zero);
            AddPoint(EndTx.position, Vector3.zero);
            _ropeLength = FirstCorner().Point.MagTo(LastCorner().Point);
            return true;
         }

         return false;
      }

      private void ClearRope()
      {
         for (int i = 0; i < _corners.Count; i++)
         {
            _pool.Push(_corners[i]);
         }
         _corners.Clear();
         PointsCount = _corners.Count;
         StartTx.parent = _tx;
         Line.SetVertexCount(0);
         _ropeLength = 0f;
      }

      private void AddPoint(Vector3 position, Vector3 norm)
      {
         if (_pool.Count > 0)
         {
            var c = _pool.Pop();
            c.Reset(position, norm);
            _corners.Add(c);
            PointsCount = _corners.Count;
         }
         else
         {
            Debug.Log("too many corners");
         }
      }

      private void InsertPoint(int i, Vector3 position, Vector3 norm)
      {
         if (_pool.Count > 0)
         {
            var c = _pool.Pop();
            c.Reset(position, norm);
            _corners.Insert(i, c);
            PointsCount = _corners.Count;
         }
         else
         {
            Debug.Log("too many corners");
         }
      }

      private void RemoveCorner(int index)
      {
         _pool.Push(_corners[index]);
         _corners.RemoveAt(index);
         PointsCount = _corners.Count;
      }

      private Corner LastCorner()
      {
         return _corners[_corners.Count - 1];
      }

      private Corner SecondLastCorner()
      {
         return _corners[_corners.Count - 2];
      }


      private Corner FirstCorner()
      {
         return _corners[0];
      }

      public bool IsRotating()
      {
         return _rotMovement.IsRotating;
      }

      public Vector3 GetRopeVelocity(Vector3 vec_velocity)
      {
         var result = Vector3.zero;
         if (_ropeIsPlanted)
         {
            var mag_overshoot = (_ropeLength - MaxLength);//Mathf.Max((_ropeLength - MaxLength), 0f);
            var pos_last = LastCorner().Point;
            var pos_2ndLast = SecondLastCorner().Point;

            // check for rotation start
            _rotMovement.IsRotating = mag_overshoot > 0 ;

            // apply rotation but make sure we can actually calculate a cross product
            if (_rotMovement.IsRotating && vec_velocity.sqrMagnitude > 0f)
            {
               var vec_radius = pos_last.VecTo(pos_2ndLast);
               _rotMovement.Dir = vec_radius.normalized;
               _rotMovement.Radius = vec_radius.magnitude - mag_overshoot;
               _rotMovement.Normal = Vector3.Cross(_rotMovement.Dir, vec_velocity.normalized);
               var tangent = _rotMovement.Tangent();
               // project onto tangent to find initial velocity
               _rotMovement.Velocity = tangent * Vector3.Dot(vec_velocity, tangent) * Time.deltaTime;
               result += _rotMovement.Velocity *(1f-RotationalFriction) ;
            }
         }

         return result;
      }

      public Vector3 GetRopePullDistance(Vector3 pos_last)
      {
         if (_ropeIsPlanted)
         {
            var mag_overshoot = Mathf.Max((_ropeLength - MaxLength), 0f);
            var pos_2ndLast = SecondLastCorner().Point;
            return (pos_last.DirTo(pos_2ndLast) * mag_overshoot)/ Mathf.Lerp( 1f , Time.deltaTime, Springiness);
         }
         return Vector3.zero;
      }

      public void Solve(Vector3 endPositionDelta)
      {
         if (!_ropeIsPlanted)
            return;

         FirstCorner().Point = StartTx.position;
         LastCorner().Point = EndTx.position + endPositionDelta;
         // look for updates
         if (DoStringPhysics)
         {
            var dt = Time.deltaTime;
            StringPhysicsInfo info;
            int i;
            for (i = 1; i < _corners.Count-1; i++)
            {
               info = _corners[i].Physx;
               info.Position = _corners[i].Point;
               info.ResetForNewFrame(Config, _corners[i - 1].Point, _corners[i + 1].Point, dt);
               info.SolveSpringForces();
               info.Step();

               _corners[i].Point = GetCornerMovePosition(_corners[i].LastPoint, info.GetFramePosition());
               info.ResolveCollisionAtPosition(_corners[i].Point);
               info.UpdatePosition();
            }
         }
         Vector3 pos_newCorner, norm_newCorner;
         for (int i = 0; i < _corners.Count - 1; i++)
         {
            if (Sweep(_corners[i], _corners[i + 1], out pos_newCorner, out norm_newCorner))
            {
               if (_corners[i + 1].Point.MagTo(pos_newCorner) > CollapseDistance && _corners[i].Point.MagTo(pos_newCorner) > CollapseDistance)
               {
                  InsertPoint(i + 1, pos_newCorner, norm_newCorner);
               }
            }
            _corners[i].LastPoint = _corners[i].Point;
         }
         LastCorner().LastPoint = LastCorner().Point;

         // look for straight lines
         for (int i = _corners.Count - 3; i >= 0; i--)
         {
            if (CornersAreSimilar(_corners[i].Point, _corners[i + 1].Point, _corners[i + 2].Point))
            {
               RemoveCorner(i + 1);
            }
         }

         // calculate length
         _ropeLength = 0f;
         for (int i = 1; i < _corners.Count; i++)
         {
            _ropeLength += _corners[i-1].Point.MagTo(_corners[i].Point);
         }
      }

      private bool CornersAreSimilar(Vector3 c_a, Vector3 c_b, Vector3 c_c)
      {
         return Mathf.Abs(c_a.MagSqrTo(c_b) - c_a.MagSqrTo(c_c)) < CollapseDistance 
            || Mathf.Abs(Vector3.Dot(c_a.DirTo(c_b), c_b.DirTo(c_c))) > Config.CollapseDotProductThreshold;
      }

      private Vector3 GetCornerMovePosition(Vector3 pos_start, Vector3 pos_end)
      {
//         if (Physics.CheckSphere(pos_start, .001f, HitLayerMask))
//         {
//            return pos_start;
//         }

         var ray = new Ray(pos_start, pos_start.DirTo(pos_end));
         var distance = pos_start.MagTo(pos_end);
         RaycastHit hit;
         if (Physics.SphereCast(ray, Radius, out hit, distance, HitLayerMask))
         {
            return pos_start + ray.direction * Mathf.Max(hit.distance - SkinWidth, 0f);
         }
         
         return pos_end;
      }

      private bool Sweep(Corner corner_a, Corner corner_b, out Vector3 pos_result, out Vector3 norm_result)
      {
         if (corner_b.LastPoint != corner_b.Point)// && !Physics.CheckSphere(corner_b.LastPoint, Radius, HitLayerMask))
         {
            var pos_start = corner_a.Point;
            var pos_lastEnd = corner_b.LastPoint;
            var pos_end = corner_b.Point;

            RaycastHit hit;
            var dir_start = pos_start.DirTo(pos_lastEnd);
            var dir_final = pos_start.DirTo(pos_end);
            var ray = new Ray(pos_start, dir_start);
            var distance = pos_start.MagTo(pos_end);

            for (int i =0; i < SweepIterations; i++)
            {
               ray.direction = Vector3.Lerp(dir_start, dir_final, i / SweepIterations).normalized;
               if (Physics.SphereCast(ray, Radius, out hit, distance, HitLayerMask))
               {
                  pos_result = HitToCorner(dir_start, pos_start, hit, out norm_result);
                  return true;
               }
            }
         }

         norm_result = pos_result = Vector3.zero;
         return false;
      }

      private Vector3 HitToCorner(Vector3 dirStart, Vector3 posStart, RaycastHit hit, out Vector3 normal)
      {
         var vec_proj = dirStart * Vector3.Dot(posStart.VecTo(hit.point), dirStart);
         normal = hit.point.DirTo(posStart + vec_proj);
         return hit.point + normal * (SkinWidth + Radius);
      }

      private void DrawRope()
      {
         int lineIndex = 0;
         if (_ropeIsPlanted)
         {
            Line.SetVertexCount(_corners.Count);

            for (int i = 0; i < _corners.Count; i++)
            {
               Line.SetPosition(lineIndex, _corners[i].Point);
               lineIndex++;
            }
         }
      }
   }
}
