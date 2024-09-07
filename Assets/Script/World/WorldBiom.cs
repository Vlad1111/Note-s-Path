using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBiom", menuName = "ScriptableObjects/WorldBiom", order = 1)]
public class WorldBiom : ScriptableObject
{
    [System.Serializable]
    public class BiomObjects
    {
        public WorldObjectSpace biomObject;
        public Vector2 minPosition;
        public Vector2 maxPosition;
        public bool flipXAxes = false;
        public int minSlicesUntillNext = 1;
        public int maxSlicesUntillNext = 1;
        [HideInInspector]
        public int slicesUntillCanCreate = -1;
        [Min(0)]
        public float probabilityPerSlice = 0.1f;
    }
    public AnimationCurve groundHeight;
    public AnimationCurve groundHeightVariation;
    public AnimationCurve minGroundHeight;
    public BiomObjects[] biomObjects;
}
