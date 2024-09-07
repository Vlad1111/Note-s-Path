using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldObjectSpace : MonoBehaviour
{
    [System.Serializable]
    public class ObjectParts
    {
        public Transform prefab;
        public Vector3 minPosition;
        public Vector3 maxPosition;
        public Vector3 minScale;
        public Vector3 maxScale;
        public Vector3 minRotation;
        public Vector3 maxRotation;

        public void ApplyRandom()
        {
            var offsetPos = new Vector3(Random.Range(minPosition.x, maxPosition.x), 
                                        Random.Range(minPosition.y, maxPosition.y), 
                                        Random.Range(minPosition.z, maxPosition.z));
            var offsetRot = new Vector3(Random.Range(minRotation.x, maxRotation.x),
                                        Random.Range(minRotation.y, maxRotation.y),
                                        Random.Range(minRotation.z, maxRotation.z));
            var offsetSca = new Vector3(Random.Range(minScale.x, maxScale.x),
                                        Random.Range(minScale.y, maxScale.y),
                                        Random.Range(minScale.z, maxScale.z));
            prefab.localPosition += offsetPos;
            prefab.localEulerAngles += offsetRot;
            prefab.localScale += offsetSca;
        }
    }

    [System.Serializable]
    public class ObjectAncorPoints
    {
        [System.Serializable]
        public class ObjectAncorExitPoint
        {
            [System.Serializable]
            public class ObjectAncorExitObject
            {
                public WorldObjectSpace exitObject;
                [Range(0, 1)]
                public float probability;
            }

            public Transform exitAncor;
            public int minChainLenght;
            public int maxChainLenght;
            [Range(0, 1)]
            public float probability;
            public bool forceCreation = false;
            public ObjectAncorExitObject[] exitObjects;
        }

        public Transform entryPoint;
        public ObjectAncorExitPoint[] exitPoints;
    }

    public ObjectParts[] parts;
    public ObjectAncorPoints[] ancors;

    private void OnDrawGizmos()
    {
        if (ancors == null)
            return;
        foreach (var a in ancors)
        {
            if(a.entryPoint == null)
                continue;
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(a.entryPoint.position, 0.05f);
            Gizmos.DrawLine(a.entryPoint.position, a.entryPoint.position + a.entryPoint.forward * 0.2f);

            foreach (var e in a.exitPoints)
                if (e != null)
                {
                    if(e.exitAncor == null)
                        continue;
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(e.exitAncor.position, 0.05f);
                    Gizmos.DrawLine(e.exitAncor.position, e.exitAncor.position + e.exitAncor.forward * 0.2f);
                }
        }
    }
}