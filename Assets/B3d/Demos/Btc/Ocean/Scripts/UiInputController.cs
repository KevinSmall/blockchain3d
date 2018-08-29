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
   /// <summary>
   /// Manage UI and Input
   /// See also UiMovePointerClickMoveForwards.cs
   ///          UiMoveMouseLook.cs
   ///          UiMoveCameraControl.cs
   ///          any VR scripts
   /// </summary>
   public class UiInputController : MonoBehaviour
   {
      // Use this for initialization
      void Start()
      {
         // Setting lockState to Locked buggers up the lookat gaze stuff, unless you have
         // Gvr Pointer Input Module (found on the Gvr Event System prefab GO) with "Only in VR" set
         // to FALSE, in which case it works ok
         Cursor.lockState = CursorLockMode.Locked;

         GlobalData.Instance.IsGlobalLinkLabelActive = true;

         // Start with a hidden mouse?
         // Anything that might have a mouse, we will hide it (it can be enabled by user pressing escape, see Update()
#if UNITY_WEBGL || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX
         Cursor.visible = false;
#endif
         // Basically any of these will show mouse pointer
         // UNITY_EDITOR
         // UNITY_IOS
         // UNITY_ANDROID
         // UNITY_WSA (UWP universal windows player)

      }

      // Update is called once per frame
      void Update()
      {
         // Free up the mouse
         if (Input.GetKeyDown(KeyCode.Escape))
         {
            if (Screen.fullScreen)
            {
               // if we are full screen, first Esc press takes to window
               Screen.fullScreen = !Screen.fullScreen;
            }
            else if (Cursor.lockState == CursorLockMode.Locked)
            {
               // second Esc press frees cursor
               Cursor.lockState = CursorLockMode.None;
               Cursor.visible = true;
            }
            else if (Cursor.lockState == CursorLockMode.None)
            {
               // third Esc toggle cursor back into window
               Cursor.lockState = CursorLockMode.Locked;
               Cursor.visible = false;
            }
         }

         // Mouse recapture also possible with a left click in the Window
         if (Input.GetMouseButton(0))
         {
            if (Cursor.lockState == CursorLockMode.None)
            {
               // Recapture mouse
               Cursor.lockState = CursorLockMode.Locked;
               Cursor.visible = false;
            }
         }
         
         // Toggle labels
         if (Input.GetKeyDown(KeyCode.L))
         {
            GlobalData.Instance.IsGlobalLinkLabelActive = !GlobalData.Instance.IsGlobalLinkLabelActive;

            foreach (GameObject linkGo in GameObject.FindGameObjectsWithTag("edge"))
            {
               //Debug.Log("UiInputController: Toggle Labels: examine label: " + linkGo.name);

               GraphEdgeBrain linkBrain = linkGo.GetComponent<GraphEdgeBrain>();
               if (GlobalData.Instance.IsGlobalLinkLabelActive)
               {
                  linkBrain.CanvasEnable();
               }
               else
               {
                  linkBrain.CanvasDisable();
               }
            }
         }

         // Toggle Physics
         if (Input.GetKeyDown(KeyCode.P))
         {
            GraphFactoryBtc.Instance.AllStatic = !GraphFactoryBtc.Instance.AllStatic;
         }

         // Toggle Mouse
         if (Input.GetKeyDown(KeyCode.M))
         {
            if (GlobalData.Instance.FlipMouseYAxis < 0f)
            {
               GlobalData.Instance.FlipMouseYAxis = 1f;
            }
            else
            {
               GlobalData.Instance.FlipMouseYAxis = -1f;
            }
         }
      }
   }
}