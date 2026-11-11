using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BuildMechanic.Inspector
{
    public enum CameraPreset
    {
        Front,
        Side,
        Top,
        Isometric
    }

    public class InspectorCamera
    {
        public Vector3 Target { get; private set; } = Vector3.Zero;
        public float Distance { get; private set; } = 3.0f;
        public float Yaw { get; private set; } = 0.45f;
        public float Pitch { get; private set; } = 0.35f;

        public bool IsSpinning { get; set; } = true;
        public float SpinSpeed { get; set; } = 0.4f;

        private Vector3 _desiredTarget;
        private float _desiredDistance;
        private float _desiredYaw;
        private float _desiredPitch;

        public Matrix ViewMatrix { get; private set; }
        public Matrix ProjectionMatrix { get; private set; }
        public Vector3 Position { get; private set; }

        private MouseState _prevMouse;
        private int _prevScroll;

        public InspectorCamera(GraphicsDevice device)
        {
            _desiredTarget = Target;
            _desiredDistance = Distance;
            _desiredYaw = Yaw;
            _desiredPitch = Pitch;
            _prevScroll = Mouse.GetState().ScrollWheelValue;

            UpdateMatrices(device.Viewport);
        }

        public void FocusOnBounds(BoundingBox bounds)
        {
            Vector3 center = (bounds.Min + bounds.Max) * 0.5f;
            float radius = Vector3.Distance(bounds.Min, bounds.Max) * 0.5f;
            radius = Math.Max(radius, 0.15f);

            _desiredTarget = center;
            _desiredDistance = Math.Clamp(radius * 2.3f, 0.4f, 25f);
        }

        public void SetPreset(CameraPreset preset)
        {
            switch (preset)
            {
                case CameraPreset.Front:
                    _desiredYaw = 0f;
                    _desiredPitch = 0.05f;
                    break;
                case CameraPreset.Side:
                    _desiredYaw = MathF.PI * 0.5f;
                    _desiredPitch = 0.05f;
                    break;
                case CameraPreset.Top:
                    _desiredYaw = 0f;
                    _desiredPitch = 1.45f;
                    break;
                case CameraPreset.Isometric:
                    _desiredYaw = MathF.PI * 0.25f;
                    _desiredPitch = 0.55f;
                    break;
            }
        }

        public void Update(GameTime gameTime, Viewport viewport)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            MouseState mouse = Mouse.GetState();
            KeyboardState kbd = Keyboard.GetState();

            bool isDragging = (mouse.LeftButton == ButtonState.Pressed && mouse.X > 280) ||
                              mouse.RightButton == ButtonState.Pressed;

            if (isDragging)
            {
                float dx = mouse.X - _prevMouse.X;
                float dy = mouse.Y - _prevMouse.Y;

                if (kbd.IsKeyDown(Keys.LeftShift) || kbd.IsKeyDown(Keys.RightShift) || mouse.MiddleButton == ButtonState.Pressed)
                {
                    Matrix camRot = Matrix.CreateRotationY(Yaw);
                    Vector3 right = camRot.Right;
                    Vector3 up = Vector3.Up;
                    _desiredTarget += (-right * dx * 0.003f * Distance) + (up * dy * 0.003f * Distance);
                }
                else
                {
                    _desiredYaw += dx * 0.007f;
                    _desiredPitch = Math.Clamp(_desiredPitch - dy * 0.007f, -1.45f, 1.45f);
                }
            }

            if (mouse.MiddleButton == ButtonState.Pressed && !isDragging)
            {
                float dx = mouse.X - _prevMouse.X;
                float dy = mouse.Y - _prevMouse.Y;
                Matrix camRot = Matrix.CreateRotationY(Yaw);
                Vector3 right = camRot.Right;
                Vector3 up = Vector3.Up;
                _desiredTarget += (-right * dx * 0.003f * Distance) + (up * dy * 0.003f * Distance);
            }

            int scrollDelta = mouse.ScrollWheelValue - _prevScroll;
            if (scrollDelta != 0)
            {
                _desiredDistance = Math.Clamp(_desiredDistance - (scrollDelta * 0.0025f), 0.35f, 30f);
            }

            if (IsSpinning && !isDragging)
            {
                _desiredYaw += dt * SpinSpeed;
            }

            _prevScroll = mouse.ScrollWheelValue;
            _prevMouse = mouse;

            float lerpFactor = Math.Min(1.0f, dt * 10f);
            Target = Vector3.Lerp(Target, _desiredTarget, lerpFactor);
            Distance = MathHelper.Lerp(Distance, _desiredDistance, lerpFactor);
            Yaw = MathHelper.Lerp(Yaw, _desiredYaw, lerpFactor);
            Pitch = MathHelper.Lerp(Pitch, _desiredPitch, lerpFactor);

            UpdateMatrices(viewport);
        }

        private void UpdateMatrices(Viewport viewport)
        {
            float cosPitch = MathF.Cos(Pitch);
            float sinPitch = MathF.Sin(Pitch);
            float cosYaw = MathF.Cos(Yaw);
            float sinYaw = MathF.Sin(Yaw);

            Vector3 offset = new Vector3(
                Distance * cosPitch * sinYaw,
                Distance * sinPitch,
                Distance * cosPitch * cosYaw
            );

            Position = Target + offset;
            ViewMatrix = Matrix.CreateLookAt(Position, Target, Vector3.Up);
            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45f),
                viewport.AspectRatio,
                0.05f,
                100f
            );
        }
    }
}
