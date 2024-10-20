﻿using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using WebSmsNet.Abstractions.Models;

namespace WebSmsNet.Abstractions.Connectors;

/// <summary>
/// Represents an interface for sending both text and binary SMS messages.
/// </summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public interface IMessagingConnector
{
    /// <summary>
    /// Sends a text message asynchronously and returns a response containing the result.
    /// </summary>
    /// <param name="request">The request containing message details.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>A task representing the asynchronous operation, with a response containing the result.</returns>
    Task<MessageSendResponse> SendTextMessage(TextSmsSendRequest request, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Sends a binary message asynchronously and returns a response containing the result.
    /// </summary>
    /// <param name="request">The request containing binary message details.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>A task representing the asynchronous operation, with a response containing the result.</returns>
    Task<MessageSendResponse> SendBinaryMessage(BinarySmsSendRequest request, [Optional] CancellationToken cancellationToken);
}
