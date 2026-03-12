using System;
using OpenTK;

namespace ThreeDPacking.App.Rendering
{
    /// <summary>
    /// Orbit camera for 3D scene navigation.
    /// Supports rotation via mouse drag, zoom via scroll wheel, and panning.
    /// </summary>
    public class CameraOrbit
    {
        public float Yaw { get; set; } = -45f;
        public float Pitch { get; set; } = 30f;
        public float Distance { get; set; } = 500f;
        public Vector3 Target { get; set; } = Vector3.Zero;

        public float MinDistance { get; set; } = 10f;
        public float MaxDistance { get; set; } = 5000f;

        public Matrix4 GetViewMatrix()
        {
            float yawRad = MathHelper.DegreesToRadians(Yaw);
            float pitchRad = MathHelper.DegreesToRadians(Pitch);

            float x = (float)(Distance * Math.Cos(pitchRad) * Math.Cos(yawRad));
            float y = (float)(Distance * Math.Sin(pitchRad));
            float z = (float)(Distance * Math.Cos(pitchRad) * Math.Sin(yawRad));

            var eye = new Vector3(x, y, z) + Target;
            return Matrix4.LookAt(eye, Target, Vector3.UnitY);
        }

        public Vector3 GetEyePosition()
        {
            float yawRad = MathHelper.DegreesToRadians(Yaw);
            float pitchRad = MathHelper.DegreesToRadians(Pitch);

            float x = (float)(Distance * Math.Cos(pitchRad) * Math.Cos(yawRad));
            float y = (float)(Distance * Math.Sin(pitchRad));
            float z = (float)(Distance * Math.Cos(pitchRad) * Math.Sin(yawRad));

            return new Vector3(x, y, z) + Target;
        }

        public void Rotate(float deltaYaw, float deltaPitch)
        {
            Yaw += deltaYaw;
            Pitch = MathHelper.Clamp(Pitch + deltaPitch, -89f, 89f);
        }

        public void Zoom(float delta)
        {
            Distance = MathHelper.Clamp(Distance - delta * Distance * 0.1f, MinDistance, MaxDistance);
        }

        public void Pan(float deltaX, float deltaY)
        {
            float yawRad = MathHelper.DegreesToRadians(Yaw);
            var right = new Vector3((float)Math.Sin(yawRad), 0, (float)-Math.Cos(yawRad));
            var up = Vector3.UnitY;

            float panSpeed = Distance * 0.002f;
            Target += right * deltaX * panSpeed + up * deltaY * panSpeed;
        }

        public void FitToScene(float sceneSize)
        {
            Distance = sceneSize * 2f;
            Target = new Vector3(sceneSize * 0.3f, sceneSize * 0.2f, sceneSize * 0.3f);
        }
    }
}
