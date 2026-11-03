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
            LeftTorsoPlate = new ArmorPlate
            {
                Id = "plate_torso_left",
                DisplayName = "Titan Vanguard — Left Breastplate",
                MountedPosition = new Vector3(-0.52f, 2.7f, 0.48f),
                DetachedPosition = new Vector3(-1.8f, 2.0f, 1.2f),
                Dimensions = new Vector3(0.85f, 1.05f, 0.08f)
            };

            // 4 Corner Bolts for Left Torso Plate
            float bx = 0.32f;
            float by = 0.42f;
            LeftTorsoPlate.Bolts.Add(new Bolt { Id = "bolt_tl_1", LocalPosition = new Vector3(-bx, by, 0.045f) });
            LeftTorsoPlate.Bolts.Add(new Bolt { Id = "bolt_tl_2", LocalPosition = new Vector3(bx, by, 0.045f) });
            LeftTorsoPlate.Bolts.Add(new Bolt { Id = "bolt_tl_3", LocalPosition = new Vector3(-bx, -by, 0.045f) });
            LeftTorsoPlate.Bolts.Add(new Bolt { Id = "bolt_tl_4", LocalPosition = new Vector3(bx, -by, 0.045f) });

            // Structural crack running across Left Torso Plate
            var leftCrack = new CrackSegment
            {
                Id = "crack_tl_main",
                LocalPoints = new List<Vector3>
                {
                    new Vector3(-0.25f, -0.22f, 0.042f),
                    new Vector3(-0.12f, -0.05f, 0.042f),
                    new Vector3(0.02f, 0.08f, 0.042f),
                    new Vector3(0.18f, 0.14f, 0.042f),
                    new Vector3(0.28f, 0.30f, 0.042f)
                }
            };
            LeftTorsoPlate.Cracks.Add(leftCrack);
            Plates.Add(LeftTorsoPlate);

            // Right Torso Plate (Intact reference plate)
            RightTorsoPlate = new ArmorPlate
            {
                Id = "plate_torso_right",
                DisplayName = "Titan Vanguard — Right Breastplate",
                MountedPosition = new Vector3(0.52f, 2.7f, 0.48f),
                DetachedPosition = new Vector3(1.8f, 2.0f, 1.2f),
                Dimensions = new Vector3(0.85f, 1.05f, 0.08f)
            };

            RightTorsoPlate.Bolts.Add(new Bolt { Id = "bolt_tr_1", LocalPosition = new Vector3(-bx, by, 0.045f) });
            RightTorsoPlate.Bolts.Add(new Bolt { Id = "bolt_tr_2", LocalPosition = new Vector3(bx, by, 0.045f) });
            RightTorsoPlate.Bolts.Add(new Bolt { Id = "bolt_tr_3", LocalPosition = new Vector3(-bx, -by, 0.045f) });
            RightTorsoPlate.Bolts.Add(new Bolt { Id = "bolt_tr_4", LocalPosition = new Vector3(bx, -by, 0.045f) });
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
