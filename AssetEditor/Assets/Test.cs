using Merlin;
using System.IO;
using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField]
    private Material srcMat;

    [SerializeField]
    private Material dstMat;

    [ContextMenu("Copy As Json")]
    public void CopyAsJson()
    {
        var matJson = MaterialSerializer.Serialize(srcMat);
        File.WriteAllText($"{Application.dataPath}/mat.json", matJson);
        MaterialSerializer.Deserialize(dstMat, matJson);
    }

    [ContextMenu("Copy As Binary")]
    public void CopyAsBinary()
    {
        var matBin = MaterialSerializer.SerializeBinary(srcMat);
        File.WriteAllBytes($"{Application.dataPath}/mat.bin", matBin);
        MaterialSerializer.DeserializeBinary(dstMat, matBin);
    }
}
