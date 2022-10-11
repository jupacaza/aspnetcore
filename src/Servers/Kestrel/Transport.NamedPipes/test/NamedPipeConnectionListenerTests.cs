// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Transport.NamedPipes.Internal;
using Microsoft.AspNetCore.Testing;

namespace Microsoft.AspNetCore.Server.Kestrel.Transport.NamedPipes.Tests;

public class NamedPipeConnectionListenerTests : TestApplicationErrorLoggerLoggedTest
{
    [Fact]
    public async Task AcceptAsync_AfterUnbind_ReturnNull()
    {
        // Arrange
        await using var connectionListener = await NamedPipeTestHelpers.CreateConnectionListenerFactory(LoggerFactory);

        // Act
        await connectionListener.UnbindAsync().DefaultTimeout();

        // Assert
        Assert.Null(await connectionListener.AcceptAsync().DefaultTimeout());
    }

    [Fact]
    public async Task AcceptAsync_ClientCreatesConnection_ServerAccepts()
    {
        // Arrange
        await using var connectionListener = await NamedPipeTestHelpers.CreateConnectionListenerFactory(LoggerFactory);

        // Stream 1
        var acceptTask1 = connectionListener.AcceptAsync();
        await using var clientStream1 = NamedPipeTestHelpers.CreateClientStream(connectionListener.EndPoint);
        await clientStream1.ConnectAsync();

        var serverConnection1 = await acceptTask1.DefaultTimeout();
        Assert.False(serverConnection1.ConnectionClosed.IsCancellationRequested);
        await serverConnection1.DisposeAsync().AsTask().DefaultTimeout();
        Assert.True(serverConnection1.ConnectionClosed.IsCancellationRequested);

        // Stream 2
        var acceptTask2 = connectionListener.AcceptAsync();
        await using var clientStream2 = NamedPipeTestHelpers.CreateClientStream(connectionListener.EndPoint);
        await clientStream2.ConnectAsync();

        var serverConnection2 = await acceptTask2.DefaultTimeout();
        Assert.False(serverConnection2.ConnectionClosed.IsCancellationRequested);
        await serverConnection2.DisposeAsync().AsTask().DefaultTimeout();
        Assert.True(serverConnection2.ConnectionClosed.IsCancellationRequested);
    }

    [Fact]
    public async Task AcceptAsync_UnbindAfterCall_CleanExitAndLog()
    {
        // Arrange
        await using var connectionListener = await NamedPipeTestHelpers.CreateConnectionListenerFactory(LoggerFactory);

        // Act
        var acceptTask = connectionListener.AcceptAsync();

        await connectionListener.UnbindAsync().DefaultTimeout();

        // Assert
        Assert.Null(await acceptTask.AsTask().DefaultTimeout());

        Assert.Contains(LogMessages, m => m.EventId.Name == "ConnectionListenerAborted");
    }

    [Fact]
    public async Task AcceptAsync_DisposeAfterCall_CleanExitAndLog()
    {
        // Arrange
        await using var connectionListener = await NamedPipeTestHelpers.CreateConnectionListenerFactory(LoggerFactory);

        // Act
        var acceptTask = connectionListener.AcceptAsync();

        await connectionListener.DisposeAsync().DefaultTimeout();

        // Assert
        Assert.Null(await acceptTask.AsTask().DefaultTimeout());

        Assert.Contains(LogMessages, m => m.EventId.Name == "ConnectionListenerAborted");
    }

    [Fact]
    public async Task BindAsync_ListenersSharePort_ThrowAddressInUse()
    {
        // Arrange
        await using var connectionListener1 = await NamedPipeTestHelpers.CreateConnectionListenerFactory(LoggerFactory);
        var pipeName = ((NamedPipeEndPoint)connectionListener1.EndPoint).PipeName;

        // Act & Assert
        await Assert.ThrowsAsync<AddressInUseException>(() => NamedPipeTestHelpers.CreateConnectionListenerFactory(LoggerFactory, pipeName: pipeName));
    }

    [Fact]
    public async Task BindAsync_ListenersSharePort_DisposeFirstListener_Success()
    {
        // Arrange
        var connectionListener1 = await NamedPipeTestHelpers.CreateConnectionListenerFactory(LoggerFactory);
        var pipeName = ((NamedPipeEndPoint)connectionListener1.EndPoint).PipeName;
        await connectionListener1.DisposeAsync();

        // Act & Assert
        await using var connectionListener2 = await NamedPipeTestHelpers.CreateConnectionListenerFactory(LoggerFactory, pipeName: pipeName);
        Assert.Equal(connectionListener1.EndPoint, connectionListener2.EndPoint);
    }
}
