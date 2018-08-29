// Blockchain 3D and VR Explorer: Blockchain Technology Visualization
// Copyright (C) 2018 Kevin Small email:contactweb@blockchain3d.info
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

using UnityEngine;

namespace B3d.Demos
{
   /// MouseLook rotates the transform based on the mouse delta.
   /// Minimum and Maximum values can be used to constrain the possible rotation

   /// To make an FPS style character:
   /// - Create a capsule.
   /// - Add a rigid body to the capsule
   /// - Add the MouseLook script to the capsule.
   ///   -> Set the mouse look to use LookX. (You want to only turn character but not tilt it)
   /// - Add FPSWalker script to the capsule

   /// - Create a camera. Make the camera a child of the capsule. Reset it's transform.
   /// - Add a MouseLook script to the camera.
   ///   -> Set the mouse look to use LookY. (You want the camera to tilt up and down like a head. The character already turns.)
   public class UiMoveMouseLook : MonoBehaviour
   {

      public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
      public RotationAxes axes = RotationAxes.MouseXAndY;
      public float sensitivityX = 90F;
      public float sensitivityY = 90F;

      public float minimumX = -360F;
      public float maximumX = 360F;

      public float minimumY = -90F;
      public float maximumY = 90F;

      float rotationX = 0F;
      float rotationY = 0F;

      float _flipMouse = 1f;

      Quaternion originalRotation;

      void Update()
      {
         _flipMouse = GlobalData.Instance.FlipMouseYAxis;

         if (Cursor.lockState == CursorLockMode.Locked)      // window owns the cursor, so it is hidden 
         {
            if (axes == RotationAxes.MouseXAndY)
            {
               // Read the mouse input axis
               rotationX += Input.GetAxis("Mouse X") * sensitivityX * Time.deltaTime;
               rotationY += Input.GetAxis("Mouse Y") * _flipMouse * sensitivityY * Time.deltaTime;

               rotationX = ClampAngle(rotationX, minimumX, maximumX);
               rotationY = ClampAngle(rotationY, minimumY, maximumY);

               Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
               Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, -Vector3.right);

               transform.localRotation = originalRotation * xQuaternion * yQuaternion;
            }
            else if (axes == RotationAxes.MouseX)
            {
               rotationX += Input.GetAxis("Mouse X") * sensitivityX * Time.deltaTime;
               rotationX = ClampAngle(rotationX, minimumX, maximumX);

               Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
               transform.localRotation = originalRotation * xQuaternion;
            }
            else
            {
               rotationY += Input.GetAxis("Mouse Y") * _flipMouse * sensitivityY * Time.deltaTime;
               rotationY = ClampAngle(rotationY, minimumY, maximumY);

               Quaternion yQuaternion = Quaternion.AngleAxis(-rotationY, Vector3.right);
               transform.localRotation = originalRotation * yQuaternion;
            }
         }
      }

      void Start()
      {
         // Make the rigid body not change rotation
         if (GetComponent<Rigidbody>())
            GetComponent<Rigidbody>().freezeRotation = true;
         originalRotation = transform.localRotation;
      }

      public static float ClampAngle(float angle, float min, float max)
      {
         if (angle < -360F)
            angle += 360F;
         if (angle > 360F)
            angle -= 360F;
         return Mathf.Clamp(angle, min, max);
      }
   }
}