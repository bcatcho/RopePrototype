namespace CatchCo.Insula.Puzzle
{
   [System.Serializable]
   public class StringPhysicsConfig
   {
      public float MassPerMeter;
      public float Stiffness;
      public float Friction;
      public float RotationStrength = 20f;
      public float MaxVelocity = 100f;
      public bool UseRotationLerping;
      public float CollapseDotProductThreshold = .9999f;
   }
}