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

using System.Collections;
using UnityEngine;

namespace B3d.Demos
{
   /// <summary>
   /// if static isbeinggazed has been false for 3 seconds, add an alpha fade to canvas group of 3dHud 
   /// if static isbeinggazed at becomes true, gradually fade in (quite quick)
   /// </summary>
   public class UiHudBrain : MonoBehaviour
   {
      private enum FadeState
      {
         Off,
         FadingIn,
         On,
         FadingOut,
      }

      CanvasGroup _cg;
      FadeState _fs;

      private void Awake()
      {
         _cg = GetComponent<CanvasGroup>();
      }

      // Use this for initialization
      void Start()
      {
         _cg.alpha = 0f;
         _fs = FadeState.Off;
      }

      // Update is called once per frame
      void Update()
      {
         if (!GlobalData.Instance.IsGlobalLinkLabelActive)
         {
            // toggled off
            _fs = FadeState.Off;
            //FadeCanvasGroup(_cg, 3f, 0f);
            _cg.alpha = 0f;
         }
         else if (GraphNodeBrain.IsAnythingBeingGazedAt)
         {
            // fade in fast
            if (_fs == FadeState.FadingOut || _fs == FadeState.Off)
            {
               _fs = FadeState.FadingIn;
               //FadeCanvasGroup(_cg, 0.5f, 1f);
               _cg.alpha = 1f;
            }
         }
         else
         {
            // fade out a bit more slowly
            if (_fs == FadeState.FadingIn || _fs == FadeState.On)
            {
               _fs = FadeState.FadingOut;
               //FadeCanvasGroup(_cg, 3f, 0f);
               _cg.alpha = 0f;
            }
         }
      }

      public static IEnumerator FadeCanvasGroup(CanvasGroup target, float duration, float targetAlpha)
      {
         if (target == null)
            yield break;

         float currentAlpha = target.alpha;

         float t = 0f;
         while (t < 1.0f)
         {
            if (target == null)
               yield break;

            float newAlpha = Mathf.SmoothStep(currentAlpha, targetAlpha, t);
            target.alpha = newAlpha;

            t += Time.deltaTime / duration;

            yield return null;

         }
         target.alpha = targetAlpha;
      }
   }
}