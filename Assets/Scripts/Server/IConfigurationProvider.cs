using System;

internal interface IConfigurationProvider
{
    /// <summary>
    ///     Gets a unique identifier for the game.
    /// </summary>
    Guid Id
    {
        get;
    }

    /// <summary>
    ///     Gets the friendly name of the game.
    /// </summary>
    string Name
    {
        get;
    }

    /// <summary>
    ///     Gets the maximum number of players permitted to connect to this game.
    /// </summary>
    int MaximumNumberOfConnections
    {
        get;
    }

    /// <summary>
    ///     Gets the port number at which this game should be hosted.
    /// </summary>
    int Port
    {
        get;
    }
}