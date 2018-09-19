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
   [System.Serializable]
   public class GraphVisualSettings
   {
      // VISUALS   
      [Header("Appearance")]
      public float NodeFadeInTimeSeconds = 1f;

      [Header("Links")]
      public Color ColLinkInput = Color.blue;
      public Color ColLinkOutput = Color.grey;
      public Color ColLinkMixed = Color.yellow;
      public Color ColLinkDefault = Color.magenta;
      public float WidthLinkMin = 0.05f;
      public float WidthLinkDefault = 0.1f;
      public float WidthLinkMax = 0.2f;

      [Header("Addresses and Transactions")]
      [Tooltip("Nodes are non-interactable for this number of seconds after spawning")]
      public float NodeInteractionSuppressTime = 1f;

      [Tooltip("Color that addresses take on when gazed at")]
      public Color ColAddrGazeAt = Color.white;
      public Color ColNodeTxIncomplete = Color.blue;
      public Color ColNodeTxComplete = Color.blue;
      public Color ColNodeAddrIncomplete = Color.red;
      public Color ColNodeAddrComplete = Color.red;
      public Color ColNodeDefault = Color.magenta;
   }
}