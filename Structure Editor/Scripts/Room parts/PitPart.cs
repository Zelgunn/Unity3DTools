using UnityEngine;
using System.Collections;

namespace RoomParts
{
    public class PitPart : RoomPart
    {
        static protected readonly float s_defaultPitDepth = 20;

        [SerializeField] protected RoomPart m_pitWall;
        [SerializeField] protected RoomPart m_pitBottom;
        protected float m_depth;

        protected void Setup(Transform parent, string partName, Vector3 size)
        {
            base.Setup(parent, partName, new Vector2(size.x, size.z));
            m_depth = size.y;
        }

        public virtual void Configure(Transform parent, string partName, Vector3 size, bool makeStatic = true)
        {
            Setup(parent, partName, size);

            RoomPart pitBottom = Instantiate(m_pitBottom);
            pitBottom.Configure(transform, partName, m_size, makeStatic);
            pitBottom.transform.localPosition = Vector3.down * m_depth;

            ConfigurePitWalls();
        }

        public override void Configure(Transform parent, string partName, Vector2 size, bool makeStatic = true)
        {
            Configure(parent, partName, new Vector3(size.x, s_defaultPitDepth, size.y), makeStatic);
        }

        public virtual void ConfigurePitWalls()
        {
            Transform pitStagesParent = new GameObject("Pit stages").transform;
            pitStagesParent.SetParent(transform);
            int subpartCount = Mathf.CeilToInt(m_depth);

            GameObject pitStage = null;

            for (int i = 0; i < subpartCount; i++)
            {
                if(pitStage == null)
                {
                    pitStage = ConfigurePitStage(pitStagesParent);
                }
                else
                {
                    pitStage = Instantiate(pitStage);
                    pitStage.transform.SetParent(pitStagesParent);
                    pitStage.transform.localPosition = Vector3.down * i;
                }
                pitStage.name = string.Format("Pit stage {0}", i);
            }
        }

        public virtual GameObject ConfigurePitStage(Transform pitStagesParent)
        {
            GameObject pitStage = new GameObject("Pit stage");
            pitStage.transform.SetParent(pitStagesParent);

            foreach (Room.Direction direction in Room.allDirection)
            {
                ConfigurePitWall(pitStage.transform, direction);
            }

            return pitStage;
        }

        virtual protected void ConfigurePitWall(Transform pitStage, Room.Direction direction)
        {
            Vector3 directionVector = Room.DirectionToVector(direction);
            float width = Room.DirectionScale(direction, new Vector3(m_size.x, 1, m_size.y)).magnitude;
            Vector2 wallSize = new Vector2(width, 1);
            string wallName = string.Format("Wall_{0}", direction);

            RoomPart wall = Instantiate(m_pitWall);
            wall.Configure(pitStage, wallName, wallSize);

            wall.transform.localPosition = Vector3.Scale(new Vector3(m_size.x, 1, m_size.y), directionVector) / 2;
            wall.transform.Rotate(Vector3.up, Room.DirectionAngle(direction) - 90, Space.World);
        }
    }
}