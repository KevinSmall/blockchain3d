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

using System.Collections.Generic;
using UnityEngine;

namespace B3d.Tools
{
   /// <summary>
   /// Audio for whole game
   /// Allows pooling in future if needed, sounds will keep playing even when objects destroyed
   /// </summary>
   public class AudioManager : MonoBehaviour
   {
      public static AudioManager Instance;

      [Header("UI Screens")]
      [Tooltip("Menu tap")]
      public AudioClip AudioMenuSelect;
      public AudioClip AudioMenuMove;

      [Header("UI Pops")]
      public AudioClip AudioScoreHitIndicator;
      public AudioClip AudioScoreAddedToBar;
      public List<AudioClip> AudioPopping;

      [Header("Explosion")]
      public List<AudioClip> AudioExplodes;

      [Header("Hops")]
      public List<AudioClip> AudioHopping;

      private AudioSource _as;
      private const float c_default_volume = 0.677f;

      void Awake()
      {
         if (Instance == null)
         {
            Debug.Log("AudioManager created");
            Instance = this;
            _as = GetComponent<AudioSource>();
            DontDestroyOnLoad(gameObject);
         }
         else
         {
            Debug.Log("AudioManager re-creation attempted, destroying the new one");
            Destroy(gameObject);
         }
      }

      // Use this for initialization
      void Start()
      {
      }

      public void PlayScoreAddedToBar()
      {
         _as.PlayOneShot(AudioScoreAddedToBar, c_default_volume);
      }

      public void PlayScoreHitIndicator()
      {
         _as.PlayOneShot(AudioScoreHitIndicator, c_default_volume);
      }

      public void PlayMenuPressDown()
      {
         _as.PlayOneShot(AudioMenuMove, c_default_volume);
      }

      public void PlayMenuPressUp()
      {
         _as.PlayOneShot(AudioMenuSelect, c_default_volume);
      }

      public void PlayMenuSelect()
      {
         _as.PlayOneShot(AudioMenuSelect, c_default_volume);
      }

      public void PlayPop()
      {
         int i = UnityEngine.Random.Range(0, AudioPopping.Count);
         _as.PlayOneShot(AudioPopping[i], c_default_volume);
      }


      public void PlayExplosion()
      {
         int i = UnityEngine.Random.Range(0, AudioExplodes.Count);
         _as.PlayOneShot(AudioExplodes[i], c_default_volume);
      }

      // Update is called once per frame
      void Update()
      {

      }
   }
}
