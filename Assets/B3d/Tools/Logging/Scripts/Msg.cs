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

using System.Diagnostics;

namespace B3d.Tools
{
   /// <summary>
   /// Bespoke logger to allow eg calls to be compiled out in non-dev builds
   /// https://docs.unity3d.com/Manual/BestPracticeUnderstandingPerformanceInUnity7.html
   /// </summary>
   public static class Msg
   {
      public static void LogAlways(string logMsg)
      {
         UnityEngine.Debug.Log(logMsg);
      }

      [Conditional("DEVELOPMENT_BUILD"), Conditional("DEBUG"), Conditional("UNITY_EDITOR")]
      public static void Log(string logMsg)
      {
         string m = "<color=green>" + logMsg + "</color>";
         UnityEngine.Debug.Log(m);
      }

      public static void LogWarning(string logMsg)
      {
         UnityEngine.Debug.LogWarning(logMsg);
      }
            
      public static void LogError(string logMsg)
      {
         string m = "<color=red>" + logMsg + "</color>";
         UnityEngine.Debug.LogError(m);
      }
   }
}