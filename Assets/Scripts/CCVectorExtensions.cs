using UnityEngine;

public static class CCVectorExtensions
{
   public static Vector3 copy(this Vector3 v)
   {
      return new Vector3(v.x, v.y, v.z);
   }
 
   public static string Pretty(this Vector3 v, int precision = 1)
   {
      var format = "F"+precision;
      return string.Format("({0}, {1}, {2})", v.x.ToString(format), v.y.ToString(format), v.z.ToString(format)); 
   }
   
   public static string Pretty(this Vector2 v, int precision = 1)
   {
      var format = "F"+precision;
      return string.Format("({0}, {1})", v.x.ToString(format), v.y.ToString(format)); 
   }
   
 
   public static Vector3 RotateX(this Vector3 v, float rads)
   {
      float sin = Mathf.Sin(rads);
      float cos = Mathf.Cos(rads);

      float ty = v.y;
      float tz = v.z;

      v.y = (cos * ty) - (sin * tz);
      v.z = (cos * tz) + (sin * ty);
    
      return v;
   }

   public static Vector3 RotateY(this Vector3 v, float rads)
   {
      float sin = Mathf.Sin(rads);
      float cos = Mathf.Cos(rads);

      float tx = v.x;
      float tz = v.z;

      v.x = (cos * tx) + (sin * tz);
      v.z = (cos * tz) - (sin * tx);
    
      return v;
   }

   public static Vector3 RotateZ(this Vector3 v, float rads)
   {
      float sin = Mathf.Sin(rads);//* Mathf.Rad2Deg );
      float cos = Mathf.Cos(rads);//* Mathf.Rad2Deg );

      float tx = v.x;
      float ty = v.y;

      v.x = (cos * tx) - (sin * ty);
      v.y = (cos * ty) + (sin * tx);
    
      return v;
   }
 
   public static Vector2 ToXY(this Vector3 v)
   {
      return new Vector2(v.x, v.y);
   }
 
   public static Vector2 ToXZ(this Vector3 v)
   {
      return new Vector2(v.x, v.z);
   }
 
   public static Vector2 ToYX(this Vector3 v)
   {
      return new Vector2(v.y, v.x);
   }
 
   public static Vector2 ToYZ(this Vector3 v)
   {
      return new Vector2(v.y, v.z);
   }
 
   public static Vector2 ToZX(this Vector3 v)
   {
      return new Vector2(v.z, v.x);
   }
 
   public static Vector2 ToZY(this Vector3 v)
   {
      return new Vector2(v.z, v.y);
   }

   public static Vector3 Scale(this Vector3 me, float x, float y, float z)
   {
      return new Vector3(me.x * x, me.y * y, me.z * z);
   }
 
   public static Vector3 SetX(this Vector3 me, float x)
   {
      return new Vector3(x, me.y, me.z);
   }

   public static Vector3 SetY(this Vector3 me, float y)
   {
      return new Vector3(me.x, y, me.z);
   }

   public static Vector3 SetZ(this Vector3 me, float z)
   {
      return new Vector3(me.x, me.y, z);
   }

   public static Vector3 X(this Vector3 me, float x)
   {
      return me.SetX(x);
   }

   public static Vector3 Y(this Vector3 me, float y)
   {
      return me.SetY(y);
   }

   public static Vector3 Z(this Vector3 me, float z)
   {
      return me.SetZ(z);
   }

   public static Vector3 XY(this Vector3 me, float x, float y)
   {
      return new Vector3(x, y, me.z);
   }

   public static Vector3 XZ(this Vector3 me, float x, float z)
   {
      return new Vector3(x, me.y, z);
   }

   public static Vector3 YZ(this Vector3 me, float y, float z)
   {
      return new Vector3(me.x, y, z);
   }

   public static Vector3 AddX(this Vector3 me, float val)
   {
      return new Vector3(me.x + val, me.y, me.z);
   }

   public static Vector3 AddY(this Vector3 me, float val)
   {
      return new Vector3(me.x, me.y + val, me.z);
   }

   public static Vector3 AddZ(this Vector3 me, float val)
   {
      return new Vector3(me.x, me.y, me.z + val);
   }

   public static Vector3 ScaleXYZ(this Vector3 me, float x = 1f, float y = 1f, float z = 1f)
   {
      return Vector3.Scale(me, new Vector3(x, y, z));
   }

   public static Vector3 ScaleXYZ(this Vector3 me, Vector3 scale)
   {
      return Vector3.Scale(me, scale);
   }

   public static Vector3 DirTo(this Vector3 point0, Vector3 otherPoint)
   {
      return  point0.VecTo(otherPoint).normalized;
   }

   public static Vector3 VecTo(this Vector3 point0, Vector3 otherPoint)
   {
      return otherPoint - point0;
   }

   public static Vector3 VecTo(this Vector3 point0, float x, float y, float z )
   {
      return point0.VecTo(new Vector3(x,y,z));
   }

   public static Vector3 Extents(this Vector3 me)
   {
      return me * .5f;
   }
    
   public static float MagTo(this Vector3 point0, Vector3 otherPoint)
   {
      return (otherPoint - point0).magnitude;
   }
   
   public static float MagSqrTo(this Vector3 point0, Vector3 otherPoint)
   {
      return (otherPoint - point0).sqrMagnitude;
   }

 #region Vector2
 
   public static Vector3 FromXY(this Vector2 v, float defaultZ = 0f)
   {
      return new Vector3(v.x, v.y, defaultZ);
   }
 
   public static Vector3 FromXZ(this Vector2 v, float defaultY = 0f)
   {
      return new Vector3(v.x, defaultY, v.y);
   }
 
   public static Vector3 FromYZ(this Vector2 v, float defaultX = 0f)
   {
      return new Vector3(defaultX, v.x, v.y);
   }

   public static Vector2 X(this Vector2 me, float x)
   {
      return new Vector2(x, me.y);
   }

   public static Vector2 Y(this Vector2 me, float y)
   {
      return new Vector2(me.x, y);
   }

   public static Vector2 XY(this Vector2 me, float x, float y)
   {
      return new Vector2(x, y);
   }

   public static Vector2 AddX(this Vector2 me, float val)
   {
      return new Vector2(me.x + val, me.y);
   }

   public static Vector2 AddY(this Vector2 me, float val)
   {
      return new Vector2(me.x, me.y + val);
   }

   public static Vector2 ScaleXY(this Vector2 me, float x = 1f, float y = 1f)
   {
      return Vector2.Scale(me, new Vector2(x, y));
   }

   public static Vector2 ScaleXY(this Vector2 me, Vector2 scale)
   {
      return Vector2.Scale(me, scale);
   }

   public static Vector2 Extents(this Vector2 me)
   {
      return me * .5f;
   }

   public static Vector2 DirTo(this Vector2 point0, Vector2 otherPoint)
   {
      return  point0.VecTo(otherPoint).normalized;
   }

   public static Vector2 VecTo(this Vector2 point0, Vector2 otherPoint)
   {
      return otherPoint - point0;
   }

   public static float MagTo(this Vector2 point0, Vector2 otherPoint)
   {
      return (otherPoint - point0).magnitude;
   }

   public static float MagSqrTo(this Vector2 point0, Vector2 otherPoint)
   {
      return (otherPoint - point0).sqrMagnitude;
   }
   
 #endregion 

   public static Vector3 Vec(this System.Collections.Generic.IList<Vector3> me, int from, int to)
   {
      return me[from].VecTo(me[to]);
   }

   public static Vector2 Vec(this System.Collections.Generic.IList<Vector2> me, int from, int to)
   {
      return me[from].VecTo(me[to]);
   }
}
