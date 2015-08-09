using System;
using Random = UnityEngine.Random;

class HardCodedConfigurationProvider : IConfigurationProvider
{
    private static readonly Guid RandomButStaticId = Guid.NewGuid();
    public Guid Id
    {
        get
        {
            return HardCodedConfigurationProvider.RandomButStaticId;
        }
    }

    public string Name
    {
        get
        {
            return "Friendly Game Name!";
        }
    }

    public int MaximumNumberOfConnections
    {
        get
        {
            return 16;
        }
    }

    private static int randomButStaticPort = -1;
    private static int RandomButStaticPort
    {
        get
        {
            return 2003;

            if (HardCodedConfigurationProvider.randomButStaticPort == -1)
            {
                HardCodedConfigurationProvider.randomButStaticPort = Random.Range(2000, 2500);        
            }
            return HardCodedConfigurationProvider.randomButStaticPort;
        }
    }

    public int Port
    {
        get
        {
            return HardCodedConfigurationProvider.RandomButStaticPort;
        }
    }
}