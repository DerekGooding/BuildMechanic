using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace BuildMechanic.Simulation
{
    public class MechChassis
    {
        public List<ArmorPlate> Plates { get; set; } = new List<ArmorPlate>();

        public ArmorPlate LeftTorsoPlate { get; private set; }
        public ArmorPlate RightTorsoPlate { get; private set; }

        public void Initialize()
        {
            Plates.Clear();

            // Left Torso Plate (The primary repair focus in Contract #1)
            // Mounted: sits cleanly on the mounting bosses at Z = 0.50f (front of boss Z = 0.46f)
            // Detached: sits in the inspection clamp cradle at Z = 1.20f, Y = 2.10f (above table surface Y = 1.24f)
            LeftTorsoPlate = new ArmorPlate
            {
                Id = "plate_torso_left",
                DisplayName = "Titan Vanguard - Left Breastplate",
                MountedPosition = new Vector3(-0.52f, 2.70f, 0.50f),
                DetachedPosition = new Vector3(-1.80f, 2.10f, 1.20f),
                Dimensions = new Vector3(0.85f, 1.05f, 0.08f)
            };

            // 4 Corner Bolts for Left Torso Plate (placed with clean margins from plate edges)
            float bx = 0.30f;
            float by = 0.38f;
            float bz = 0.041f; // Sits directly on front face of 0.08f-thick plate (half-thickness = 0.04f)

            LeftTorsoPlate.Bolts.Add(new Bolt { Id = "bolt_tl_1", LocalPosition = new Vector3(-bx, by, bz) });
            LeftTorsoPlate.Bolts.Add(new Bolt { Id = "bolt_tl_2", LocalPosition = new Vector3(bx, by, bz) });
            LeftTorsoPlate.Bolts.Add(new Bolt { Id = "bolt_tl_3", LocalPosition = new Vector3(-bx, -by, bz) });
            LeftTorsoPlate.Bolts.Add(new Bolt { Id = "bolt_tl_4", LocalPosition = new Vector3(bx, -by, bz) });

            // Structural crack running across Left Torso Plate front surface
            var leftCrack = new CrackSegment
            {
                Id = "crack_tl_main",
                LocalPoints = new List<Vector3>
                {
                    new Vector3(-0.24f, -0.22f, bz),
                    new Vector3(-0.11f, -0.05f, bz),
                    new Vector3(0.02f, 0.08f, bz),
                    new Vector3(0.16f, 0.14f, bz),
                    new Vector3(0.26f, 0.28f, bz)
                }
            };
            LeftTorsoPlate.Cracks.Add(leftCrack);
            Plates.Add(LeftTorsoPlate);

            // Right Torso Plate (Symmetrical intact reference plate)
            RightTorsoPlate = new ArmorPlate
            {
                Id = "plate_torso_right",
                DisplayName = "Titan Vanguard - Right Breastplate",
                MountedPosition = new Vector3(0.52f, 2.70f, 0.50f),
                DetachedPosition = new Vector3(1.80f, 2.10f, 1.20f),
                Dimensions = new Vector3(0.85f, 1.05f, 0.08f)
            };

            RightTorsoPlate.Bolts.Add(new Bolt { Id = "bolt_tr_1", LocalPosition = new Vector3(-bx, by, bz) });
            RightTorsoPlate.Bolts.Add(new Bolt { Id = "bolt_tr_2", LocalPosition = new Vector3(bx, by, bz) });
            RightTorsoPlate.Bolts.Add(new Bolt { Id = "bolt_tr_3", LocalPosition = new Vector3(-bx, -by, bz) });
            RightTorsoPlate.Bolts.Add(new Bolt { Id = "bolt_tr_4", LocalPosition = new Vector3(bx, -by, bz) });
            Plates.Add(RightTorsoPlate);
        }

        public void Update(float dt)
        {
            foreach (var plate in Plates)
            {
                plate.Update(dt);
            }
        }
    }
}
