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

using B3d.Engine.Cdm;
using B3d.Tools;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace B3d.Engine.Adaptors
{
   /// <summary>
   /// Generic Adaptor that can handle any source
   /// MonoBehaviour is needed for the startcoroutine parts when handling unity web requests
   /// </summary>
   public abstract class Adaptor : MonoBehaviour
   {
      [Header("Adaptor Config")]
      [Tooltip("Cdm that this adaptor will push data into. Can be left empty if we don't want to push data anywhere, eg for Login Screens that might just want to check existence of addresses or tx without actually loading them anywhere.")]
      /// <summary>
      /// Cdm that this adaptor will push data into
      /// </summary>
      public CdmCore CdmCore;

      public virtual void CheckConnectionPossible(Action callbackOnSuccess, Action callbackOnFail)
      {
         Msg.Log("Adaptor checking internet connection...");

         // Check connection with unity web request
         StartCoroutine(UwrCheckConnection(callbackOnSuccess, callbackOnFail));
      }

      public virtual bool IsAdaptorEnabled()
      {
         return this.enabled;
      }

      public IEnumerator UwrCheckConnection(Action callbackOnSuccess, Action callbackOnFail)
      {
         UnityWebRequest uwr = UnityWebRequest.Get("http://www.google.com");

         yield return uwr.SendWebRequest();

         if (uwr.isNetworkError || uwr.isHttpError)
         {
            // Fail         
            Msg.LogWarning("Adaptor.UwrCheckConnection - no internet connection");
            Msg.Log(uwr.error);

            callbackOnFail();
         }
         else
         {
            // All OK
            Msg.Log("Adaptor.UwrCheckConnection - internet connection ok");

            callbackOnSuccess();
         }
      }
   }
}