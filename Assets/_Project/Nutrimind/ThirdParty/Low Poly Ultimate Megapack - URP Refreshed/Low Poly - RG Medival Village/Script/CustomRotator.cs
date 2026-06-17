using UnityEngine;

namespace CustomScripts
{
    public class CustomRotator : MonoBehaviour
    {
        public enum Axis { X, Y, Z }
        public Axis rotationAxis = Axis.Y;

        public float rotationSpeed = 10f;

        public bool continuousRotation = true;

        public float maxRotationAngle = 45f;
        public float pauseDuration = 1f;

        private float currentRotation = 0f;
        private float direction = 1f;
        private bool isPaused = false;
        private float pauseTimer = 0f;

        void Update()
        {
            if (continuousRotation)
            {
                RotateObject(rotationSpeed * Time.deltaTime * direction);
            }
            else
            {
                if (!isPaused)
                {
                    RotateObject(rotationSpeed * Time.deltaTime * direction);
                    if (Mathf.Abs(currentRotation) >= maxRotationAngle)
                    {
                        isPaused = true;
                        direction *= -1;
                        pauseTimer = 0f;
                    }
                }
                else
                {
                    pauseTimer += Time.deltaTime;
                    if (pauseTimer >= pauseDuration)
                    {
                        isPaused = false;
                    }
                }
            }
        }

        private void RotateObject(float angle)
        {
            switch (rotationAxis)
            {
                case Axis.X:
                    transform.Rotate(Vector3.right, angle);
                    break;
                case Axis.Y:
                    transform.Rotate(Vector3.up, angle);
                    break;
                case Axis.Z:
                    transform.Rotate(Vector3.forward, angle);
                    break;
            }

            currentRotation += angle;
        }
    }
}
