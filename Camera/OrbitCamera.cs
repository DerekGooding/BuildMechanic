using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BuildMechanic.Camera
{
    public class OrbitCamera
    {
        public Vector3 Target { get; private set; } = new Vector3(0, 2.5f, 0);
        public float Distance { get; private set; } = 4.0f;
        public float Yaw { get; private set; } = 0.0f;
        public float Pitch { get; private set; } = 0.15f;

        private Vector3 _desiredTarget;
        private float _desiredDistance;
        private float _desiredYaw;
        private float _desiredPitch;

        public Matrix ViewMatrix { get; private set; }
        public Matrix ProjectionMatrix { get; private set; }
        public Vector3 Position { get; private set; }

        private MouseState _prevMouse;
        private int _prevScroll;

        public OrbitCamera(GraphicsDevice device)
        {
            _desiredTarget = Target;
            _desiredDistance = Distance;
            _desiredYaw = Yaw;
            _desiredPitch = Pitch;
            _prevScroll = Mouse.GetState().ScrollWheelValue;

            UpdateMatrices(device.Viewport);
        }

        public void FocusSector(Vector3 target, float distance, float yaw, float pitch)
        {
            _desiredTarget = target;
            _desiredDistance = Math.Clamp(distance, 1.2f, 15f);
            _desiredYaw = yaw;
            _desiredPitch = Math.Clamp(pitch, -1.4f, 1.4f);
        }

        public void Update(GameTime gameTime, Viewport viewport)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            MouseState mouse = Mouse.GetState();
            KeyboardState kbd = Keyboard.GetState();

            // Right Click Drag = Orbit
            if (mouse.RightButton == ButtonState.Pressed && _prevMouse.RightButton == ButtonState.Pressed)
            {
                float dx = mouse.X - _prevMouse.X;
                float dy = mouse.Y - _prevMouse.Y;

                if (kbd.IsKeyDown(Keys.LeftShift) || kbd.IsKeyDown(Keys.RightShift))
                {
                    // Shift + Right Click = Pan
                    Matrix camRot = Matrix.CreateRotationY(Yaw);
                    Vector3 right = camRot.Right;
                    Vector3 up = Vector3.Up;
                    _desiredTarget += (-right * dx * 0.004f * Distance) + (up * dy * 0.004f * Distance);
                }
                else
                {
                    // Orbit
                    _desiredYaw += dx * 0.006f;
                    _desiredPitch = Math.Clamp(_desiredPitch - dy * 0.006f, -1.4f, 1.4f);
                }
            }

            // Middle Click Drag = Pan
            if (mouse.MiddleButton == ButtonState.Pressed && _prevMouse.MiddleButton == ButtonState.Pressed)
            {
                float dx = mouse.X - _prevMouse.X;
                float dy = mouse.Y - _prevMouse.Y;
                Matrix camRot = Matrix.CreateRotationY(Yaw);
                Vector3 right = camRot.Right;
                Vector3 up = Vector3.Up;
                _desiredTarget += (-right * dx * 0.004f * Distance) + (up * dy * 0.004f * Distance);
            }

            // Scroll Wheel = Zoom
            int scrollDelta = mouse.ScrollWheelValue - _prevScroll;
            if (scrollDelta != 0)
            {
                _desiredDistance = Math.Clamp(_desiredDistance - (scrollDelta * 0.002f), 1.2f, 12f);
            }
            _prevScroll = mouse.ScrollWheelValue;
            _prevMouse = mouse;

            // Smooth interpolation
            float lerpFactor = Math.Min(1.0f, dt * 10f);
            Target = Vector3.Lerp(Target, _desiredTarget, lerpFactor);
            Distance = MathHelper.Lerp(Distance, _desiredDistance, lerpFactor);
            Yaw = MathHelper.Lerp(Yaw, _desiredYaw, lerpFactor);
            Pitch = MathHelper.Lerp(Pitch, _desiredPitch, lerpFactor);

            UpdateMatrices(viewport);
        }

        private void UpdateMatrices(Viewport viewport)
        {
            Matrix rotation = Matrix.CreateRotationX(Pitch) * Matrix.CreateRotationY(Yaw);
            Vector3 offset = Vector3.Transform(new Vector3(0, 0, Distance), rotation);
            Position = Target + offset;

            ViewMatrix = Matrix.CreateLookAt(Position, Target, Vector3.Up);
            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(50f),
                viewport.AspectRatio,
                0.1f,
                100f
            );
        }

        public Ray GetMouseRay(Point mousePosition, Viewport viewport)
        {
            Vector3 nearPoint = viewport.Unproject(
                new Vector3(mousePosition.X, mousePosition.Y, 0.0f),
                ProjectionMatrix,
                ViewMatrix,
                Matrix.Identity
            );

            Vector3 farPoint = viewport.Unproject(
                new Vector3(mousePosition.X, mousePosition.Y, 1.0f),
                ProjectionMatrix,
                ViewMatrix,
                Matrix.Identity
            );

            Vector3 direction = Vector3.Normalize(farPoint - nearPoint);
            return new Ray(nearPoint, direction);
        }
    }
}
