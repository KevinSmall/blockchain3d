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

using B3d.Tools;
using UnityEngine;

namespace B3d.Demos
{
   /// <summary>
   /// Persistent singleton for inter-scene communication. Allows login scene to store runtime values (eg what address to show) that
   /// are later collected by the world scene.
   /// </summary>
   public class GlobalData : MonoBehaviour
   {
      public static GlobalData Instance;

      [Info("Persistent singleton for inter-scene communication. It exists in the login scene to allow the login scene to store global data (eg what address the user wants to see)." + 
         " The global data is later applied to the world scene by the StartUpController. Note GlobalData exists also in the world scene directly so that the world scene can be" + 
         " tested on its own without requiring the login scene to be run first.")]
      [Header("Runtime values filled by Login Screen")]
      [Tooltip("Offline tx data requested")]
      public bool OfflineTransactionDataRequested = false;
      [Tooltip("Offline address data requested")]
      public bool OfflineAddressDataRequested = false;
      [Tooltip("Start address selected on login screen")]
      public string StartAddr = null;
      [Tooltip("Start transaction selected on login screen")]
      public string StartTx = null;
      [Tooltip("Is Cardboard Available? (identified by login screen)")]
      public bool CardboardAvailable = false;
      [Tooltip("Has Cardboard been requested? (identified by login screen, used eg Esc means exit if cardboard requested and available)")]
      public bool CardboardRequested = false;
      [Tooltip("Flag first node created in the world yet or not? (used eg to put identifying shell around first node")]
      public bool FirstNodeCreatedYet = false;
      [Tooltip("Flip mouse can be 1f for normal and -1f for flipped, it is applied to Y axis input")]
      public float FlipMouseYAxis = 1f;
      [Tooltip("Global flag, are labels active?")]
      public bool IsGlobalLinkLabelActive;

      void Awake()
      {
         if (Instance == null)
         {
            Debug.Log("ControlPanel created");
            Instance = this;
            DontDestroyOnLoad(gameObject);
         }
         else
         {
            Debug.Log("ControlPanel re-creation attempted, destroying the new one");
            Destroy(gameObject);
         }
      }
   }

}

