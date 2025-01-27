using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;

namespace CrimeGame.Class_Code
{


    public class Camera
    {
    private Vector2 cameraPosition = Vector2.Zero; //Camera offset
    public float Zoom = 1.0f, MaxZoom = 1.4f, MinZoom = 0.6f; //Zoom Level

        public Camera() { }

        public void Pan(Vector2 delta) { cameraPosition -= delta / Zoom; }
        public void SmoothZoom(float targetZoom, float deltaTime, float zoomSpeed = 2f)
        {
            // Interpolate current zoom towards the target zoom
            Zoom = MathHelper.Lerp(Zoom, targetZoom, zoomSpeed * deltaTime);

            // Optional: Clamp zoom to avoid extreme zoom levels
            Zoom = MathHelper.Clamp(Zoom, 0.1f, 10f); // Adjust min and max zoom as needed
        }
        public Matrix GetTransformMatrix() { return Matrix.CreateTranslation(-cameraPosition.X, -cameraPosition.Y, 0) * Matrix.CreateScale(Zoom);
        }

    }
    
}
