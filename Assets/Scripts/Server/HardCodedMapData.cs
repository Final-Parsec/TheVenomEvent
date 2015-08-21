using UnityEngine;

class HardCodedMapData : IMapData
{
    public Vector3 GetSpawnPosition(int teamId)
    {
        return new Vector3(-17.327f, 3.871f, -1f);
    }


}