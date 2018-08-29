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
// 
// Some code came from:
// Copyright 2014 Jason Graves (GodLikeMouse/Collaboradev)
// http://www.collaboradev.com

using UnityEngine;

namespace B3d.Demos
{
   public class UiMoveCameraControl : MonoBehaviour
   {
      public float speed = 12f;
      private Vector3 move = new Vector3();

      void Update()
      {
         move.x = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
         move.z = Input.GetAxis("Vertical") * speed * Time.deltaTime;

         move.y = 0;
         if (Input.GetKey("e"))
         {
            move.y = speed * Time.deltaTime;
         }

         if (Input.GetKey("q"))
         {
            move.y = -speed * Time.deltaTime;
         }

         //adjust speed with mouse wheel
         speed += Input.GetAxis("Mouse ScrollWheel") * 15;
         if (speed < 5)
            speed = 5;

         move = transform.TransformDirection(move);
         transform.position += move;
      }
   }
}
