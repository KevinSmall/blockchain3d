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
using B3d.Tools;

namespace B3d.Demos
{
   /// <summary>
   /// Move player if we're gazing at nothing in particular and clicking
   /// </summary>
   public class UiMovePointerClickMoveForwards : MonoBehaviour
   {
      /// <summary>
      /// Flag if we're gazing at something (updated by the objects we look at via BitStreamBrain)
      /// </summary>
      public static bool IsGazing;

      float clicked = 0;
      float clicktime = 0;
      float clickdelay = 0.7f;

      // Use this for initialization
      void Start()
      {
         IsGazing = false;
      }

      // Update is called once per frame
      void Update()
      {
         // Anything that has a keyboard doesnt need this feature
//#if UNITY_EDITOR || UNITY_WEBGL || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX
//         return;
//#endif
         // UNITY_IOS
         // UNITY_ANDROID
         // UNITY_WSA (UWP universal windows player)

         if (Input.GetMouseButtonDown(0))
         {
            //----------------------------------------------------------------------------
            // Detect double tap
            //----------------------------------------------------------------------------
            bool doubleTap = false;
            clicked++;
            if (clicked == 1)
            {
               clicktime = Time.time;
            }
            else if (clicked > 1 && ((Time.time - clicktime) < clickdelay))
            {
               clicked = 0;
               clicktime = 0;
               Msg.Log("Double tap ");
               doubleTap = true;
            }
            else if (clicked > 2 || Time.time - clicktime > 1)
            {
               clicked = 0;
            }

            //----------------------------------------------------------------------------
            // Apply single or double tap
            //----------------------------------------------------------------------------
            if (IsGazing)
            {
               //Debug.Log("Pressed left click, but move suppressed because gazing.");
            }
            else if (doubleTap)
            {
               Stop();
            }
            else
            {
               //Debug.Log("Pressed left click, moving.");
               MoveForwards();
            }
         }
      }

      private void Stop()
      {
         Rigidbody rb = GetComponent<Rigidbody>();
         rb.velocity = Vector3.zero;
         rb.angularVelocity = Vector3.zero;
         Debug.Log("Stopped...");
      }

      private void MoveForwards()
      {
         Vector3 fwd = Camera.main.transform.forward;
         //Vector3 actualForceDir = new Vector3(desiredTerrainDir.x, 2f, desiredTerrainDir.z);
         GetComponent<Rigidbody>().AddForce(fwd * 14f);

         //Debug.Log("Moving...");
      }
   }
}