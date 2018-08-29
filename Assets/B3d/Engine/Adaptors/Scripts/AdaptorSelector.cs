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

namespace B3d.Engine.Adaptors
{
   /// <summary>
   /// Allow user or a script to select active adaptor from many attached to the parent game object. The job of this script is to 
   /// return the correct IAdaptor object whenever GetChosenAdaptor() is called.
   /// </summary>
   public class AdaptorSelector : MonoBehaviour
   {
      public enum AvailableAdaptor { AdaptorBtcDotInfo, AdaptorBtcOfflineFiles };

      [Info("This component allows you (or a script) to select what the active adaptor is from many that could be attached to this game object." +
         " The adaptor is the entry point from the outside world that provides us with graph data.")]
      public AvailableAdaptor ChosenAdaptor;

      private IAdaptor _chosenAdaptorObject;

      public IAdaptor GetChosenAdaptor()
      {
         if (_chosenAdaptorObject == null)
         {
            Msg.LogError("AdaptorSelector.GetChosenAdaptor cannot return an adaptor");
            return null;
         }
         else
         {
            return _chosenAdaptorObject;
         }
      }

      /// <summary>
      /// Use the inspector enum to refresh the internal adaptor object that is used by code
      /// </summary>
      private void RefreshChosenAdaptorObject()
      {
         var allAdaptors = this.GetComponents<IAdaptor>();
         for (int i = 0; i < allAdaptors.GetLength(0); i++)
         {
            string s = allAdaptors[i].ToString();
            if (ChosenAdaptor == AvailableAdaptor.AdaptorBtcDotInfo && s.Contains("AdaptorBtcDotInfo"))  // I cant remember why this is doing two checks
            {
               Msg.Log("AdaptorSelector.RefreshChosenAdaptorObject sets adaptor to AdaptorBtcDotInfo");
               _chosenAdaptorObject = allAdaptors[i];
            }
            else if (ChosenAdaptor == AvailableAdaptor.AdaptorBtcOfflineFiles && s.Contains("AdaptorBtcOfflineFiles"))
            {
               Msg.Log("AdaptorSelector.RefreshChosenAdaptorObject sets adaptor to AdaptorBtcOfflineFiles");
               _chosenAdaptorObject = allAdaptors[i];
            }
         }
      }

      public void SetChosenAdaptor(AvailableAdaptor newAdaptor)
      {
         // The enum visible in the inspector
         ChosenAdaptor = newAdaptor;
         Msg.Log("AdaptorSelector.SetAdaptor: Adaptor is now " + newAdaptor.ToString());

         // Refresh the internal adaptor object that is used by code from the inspector enum
         RefreshChosenAdaptorObject();
      }

      private void Awake()
      {
         // Refresh the internal adaptor object that is used by code from the inspector enum
         RefreshChosenAdaptorObject();
      }
   }
}
