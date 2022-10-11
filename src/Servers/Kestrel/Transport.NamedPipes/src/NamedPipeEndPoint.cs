// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;

namespace Microsoft.AspNetCore.Server.Kestrel.Transport.NamedPipes;

/// <summary>
/// Represents a Named Pipe endpoint.
/// </summary>
public sealed class NamedPipeEndPoint : EndPoint
{
    internal const string LocalComputerServerName = ".";

    /// <summary>
    /// Initializes a new instance of the <see cref="NamedPipeEndPoint"/> class.
    /// </summary>
    /// <param name="pipeName">The name of the pipe.</param>
    /// <param name="serverName">The name of the remote computer to connect to, or "." to specify the local computer.</param>
    public NamedPipeEndPoint(string pipeName, string serverName = LocalComputerServerName)
    {
        ServerName = serverName;
        PipeName = pipeName;
    }

    /// <summary>
    /// Gets the name of the remote computer. The server name must be ".", the local computer, when creating a server.
    /// </summary>
    public string ServerName { get; }
    /// <summary>
    /// Gets the name of the pipe.
    /// </summary>
    public string PipeName { get; }

    /// <summary>
    /// Gets the pipe name represented by this <see cref="NamedPipeEndPoint"/> instance.
    /// </summary>
    public override string ToString()
    {
        return $"pipe:{ServerName}/{PipeName}";
    }
}
