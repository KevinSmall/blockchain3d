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
// Some Force Directed Graph code based on:
// Bamfax/ForceDirectedNodeGraph3DUnity licensed under GNU GPL v3.0
// https://github.com/Bamfax/ForceDirectedNodeGraph3DUnity

using UnityEngine;

namespace B3d.Demos
{
   public class GraphNodePhysics : MonoBehaviour
   {
      public string Id;
      public string Text;

      private Rigidbody thisRigidbody;
      private float sphRadius;
      private float sphRadiusSqr;
      private static GraphFactoryBtc graphFactory;
      private string id;
      private string text;
      private string type;

      private void Start()
      {
         UpdateSphereRadius();

         graphFactory = FindObjectOfType<GraphFactoryBtc>();
         thisRigidbody = this.GetComponent<Rigidbody>();
      }

      void FixedUpdate()
      {
         if (!graphFactory.AllStatic && graphFactory.RepulseActive)
         {
            FdgRepulse();
         }

         if (!graphFactory.AllStatic)
         {
            FdgGravity();
         }
      }

      private void FdgGravity()
      {
         // Apply global gravity pulling node towards center of universe
         Vector3 dirToCenter = -this.transform.position;
         Vector3 impulse = dirToCenter.normalized * thisRigidbody.mass * graphFactory.GlobalGravity;
         thisRigidbody.AddForce(impulse);
      }

      private void FdgRepulse()
      {
         // test which node nearby
         Collider[] hitColliders = Physics.OverlapSphere(this.transform.position, sphRadius);

         // only apply force to nodes within forceSphere, with Falloff towards the boundary of the Sphere and no force if outside Sphere.
         foreach (Collider hitCollider in hitColliders)
         {
            Rigidbody hitRb = hitCollider.attachedRigidbody;

            if (hitRb != null && hitRb != thisRigidbody)
            {
               Vector3 direction = hitCollider.transform.position - this.transform.position;
               float distSqr = direction.sqrMagnitude;

               // Normalize the distance from forceSphere Center to node into 0..1
               float impulseExpoFalloffByDist = Mathf.Clamp(1 - (distSqr / sphRadiusSqr), 0, 1);

               // apply normalized distance
               hitRb.AddForce(direction.normalized * graphFactory.RepulseForceStrength * impulseExpoFalloffByDist);
            }
         }
      }

      void Update()
      {
         UpdateSphereRadius();
      }

      private void UpdateSphereRadius()
      {
         // updating variable here, as opposed to doing it only in Start(), otherwise we won't see runtime updates of forceSphereRadius
         if (graphFactory != null)
         {
            sphRadius = graphFactory.NodeForceSphereRadius;
         }
         else
         {
            sphRadius = 10f; // for first frame or so (TODO ugly hack, check why factory not available, given that factory created us)
         }
         sphRadiusSqr = sphRadius * sphRadius;
      }
   }
}