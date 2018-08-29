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

namespace B3d.Tools
{
   /// <summary>
   /// Scales a material to match the owning object's transform
   /// Silently fails if the textures do not exist (perf impact? if so could add public var and control in inspector
   /// what gets done here)
   /// </summary>
   public class MaterialCubeScaler : MonoBehaviour
   {
      // Use this for initialization
      void Start()
      {
      }

      void Update()
      {
         //Vector2 newScale = new Vector2(Mathf.Cos(Time.time) * 0.5F + 1, Mathf.Sin(Time.time) * 0.5F + 1);
         Vector2 newScale = new Vector2(transform.localScale.x, transform.localScale.y);

         //print(scaleX.ToString() + ", " + scaleY.ToString());
         //renderer.material.mainTextureScale = newScale;  // <-- this is same as line below
         GetComponent<Renderer>().material.SetTextureScale("_MainTex", newScale);
         GetComponent<Renderer>().material.SetTextureScale("_BumpMap", newScale);

         // renderer.material.GetTexture(_MainTex).
         // shader properties
         // _Color
         // _MainTex
         // _BumpMap
      }
   }
}

