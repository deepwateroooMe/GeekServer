using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using Geek.Server.Core.Net.Bedrock.Infrastructure;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Http.Features;

namespace Geek.Server.Core.Net.Bedrock.Transports.Sockets {

    internal class SocketConnection : ConnectionContext, IConnectionInherentKeepAliveFeature {

        private readonly Socket _socket;
        private volatile bool _aborted;
        private readonly EndPoint _endPoint;
        private IDuplexPipe _application;

        private readonly SocketSender _sender;
        private readonly SocketReceiver _receiver;
        private readonly CancellationTokenSource _connectionClosedTokenSource = new CancellationTokenSource();
        private readonly TaskCompletionSource _waitForConnectionClosedTcs = new TaskCompletionSource();
        private readonly CancellationToken _connectTimeoutToken;

        public SocketConnection(EndPoint endPoint, CancellationToken cancellationToken = default) {
            _socket = new Socket(endPoint.AddressFamily, SocketType.Stream, DetermineProtocolType(endPoint));
            _connectTimeoutToken = cancellationToken;
            _endPoint = endPoint;
            _sender = new SocketSender(_socket, PipeScheduler.ThreadPool);
            _receiver = new SocketReceiver(_socket, PipeScheduler.ThreadPool);
            // Add IConnectionInherentKeepAliveFeature to the tcp connection impl since Kestrel doesn't implement
            // the IConnectionHeartbeatFeature
            Features.Set<IConnectionInherentKeepAliveFeature>(this);
            ConnectionClosed = _connectionClosedTokenSource.Token;
        }
        public override IDuplexPipe Transport { get; set; }
        public override IFeatureCollection Features { get; } = new FeatureCollection();
        public override string ConnectionId { get; set; } = Guid.NewGuid().ToString();
        public override IDictionary<object, object> Items { get; set; } = new ConnectionItems();
        // We claim to have inherent keep-alive so the client doesn't kill the connection when it hasn't seen ping frames.
        public bool HasInherentKeepAlive { get; } = true;

        public override async ValueTask DisposeAsync() {
            if (Transport != null) {
                await Transport.Output.CompleteAsync().ConfigureAwait(false);
                await Transport.Input.CompleteAsync().ConfigureAwait(false);
            }
            // Completing these loops will cause ExecuteAsync to Dispose the socket.
        }
        public async ValueTask<ConnectionContext> StartAsync() {
            if (_connectTimeoutToken != default)
                await _socket.ConnectAsync(_endPoint, _connectTimeoutToken).ConfigureAwait(false);
            else
                await _socket.ConnectAsync(_endPoint).ConfigureAwait(false);
            var pair = DuplexPipe.CreateConnectionPair(PipeOptions.Default, PipeOptions.Default);
            LocalEndPoint = _socket.LocalEndPoint;
            RemoteEndPoint = _socket.RemoteEndPoint;
            Transport = pair.Transport;
            _application = pair.Application;
            _ = ExecuteAsync();
            return this;
        }
        private async Task ExecuteAsync() {
            Exception sendError = null;
            try {
                // Spawn send and receive logic
                var receiveTask = DoReceive();
                var sendTask = DoSend();
                // If the sending task completes then close the receive
                // We don't need to do this in the other direction because the kestrel
                // will trigger the output closing once the input is complete.
                if (await Task.WhenAny(receiveTask, sendTask).ConfigureAwait(false) == sendTask) {
                    // Tell the reader it's being aborted
                    _socket.Dispose();
                }
                // Now wait for both to complete
                await receiveTask;
                sendError = await sendTask;
                // Dispose the socket(should noop if already called)
                _socket.Dispose();
            }
            catch (Exception ex) {
                Console.WriteLine($"Unexpected exception in {nameof(SocketConnection)}.{nameof(StartAsync)}: " + ex);
            }
            finally {
                // Complete the output after disposing the socket
                _application.Input.Complete(sendError);
            }
        }

        private async Task DoReceive() {
            Exception error = null;
            try {
                await ProcessReceives().ConfigureAwait(false);
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionReset) {
                    error = new ConnectionResetException(ex.Message, ex);
                }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.OperationAborted ||
                                             ex.SocketErrorCode == SocketError.ConnectionAborted ||
                                             ex.SocketErrorCode == SocketError.Interrupted ||
                                             ex.SocketErrorCode == SocketError.InvalidArgument) {
                    if (!_aborted) {
                        // Calling Dispose after ReceiveAsync can cause an "InvalidArgument" error on *nix.
                        error = new ConnectionAbortedException();
                    }
                }
            catch (ObjectDisposedException) {
                if (!_aborted) {
                    error = new ConnectionAbortedException();
                }
            }
            catch (IOException ex) {
                error = ex;
            }
            catch (Exception ex) {
                error = new IOException(ex.Message, ex);
            }
            finally {
                if (_aborted) {
                    error ??= new ConnectionAbortedException();
                }
                await _application.Output.CompleteAsync(error).ConfigureAwait(false);
                FireConnectionClosed();
                await _waitForConnectionClosedTcs.Task;
            }
        }

        private bool _connectionClosed;
        private void FireConnectionClosed() {
            // Console.WriteLine($"{ConnectionId} closed");
            // Guard against scheduling this multiple times
            if (_connectionClosed) {
                return;
            }
            _connectionClosed = true;
            ThreadPool.UnsafeQueueUserWorkItem(state => {
                state.CancelConnectionClosedToken();
                state._waitForConnectionClosedTcs.TrySetResult();
            },
                this,
                preferLocal: false);
        }

        public override CancellationToken ConnectionClosed { get; set; }
        private void CancelConnectionClosedToken() {
            try {
                _connectionClosedTokenSource.Cancel();
            }
            catch (Exception ex) {
                Console.WriteLine($"Unexpected exception in {nameof(SocketConnection)}.{nameof(CancelConnectionClosedToken)}.{ex}");
            }
        }

        private async Task ProcessReceives() {
            while (true) {
                // Ensure we have some reasonable amount of buffer space
                var buffer = _application.Output.GetMemory();
                var bytesReceived = await _receiver.ReceiveAsync(buffer);
                if (bytesReceived == 0) {
                    // FIN
                    break;
                }
                _application.Output.Advance(bytesReceived);
                var flushTask = _application.Output.FlushAsync();
                if (!flushTask.IsCompleted) {
                    await flushTask.ConfigureAwait(false);
                }
                var result = flushTask.Result;
                if (result.IsCompleted) {
                    // Pipe consumer is shut down, do we stop writing
                    break;
                }
            }
        }

        private async Task<Exception> DoSend() {
            Exception error = null;
            try {
                await ProcessSends().ConfigureAwait(false);
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.OperationAborted) {
                    error = null;
                }
            catch (ObjectDisposedException) {
                error = null;
            }
            catch (IOException ex) {
                error = ex;
            }
            catch (Exception ex) {
                error = new IOException(ex.Message, ex);
            }
            finally {
                _aborted = true;
                _socket.Shutdown(SocketShutdown.Both);
            }
            return error;
        }

        private async Task ProcessSends() {
            while (true) {
                // Wait for data to write from the pipe producer
                var result = await _application.Input.ReadAsync().ConfigureAwait(false);
                var buffer = result.Buffer;
                if (result.IsCanceled) {
                    break;
                }
                var end = buffer.End;
                var isCompleted = result.IsCompleted;
                if (!buffer.IsEmpty) {
                    await _sender.SendAsync(buffer);
                }
                _application.Input.AdvanceTo(end);
                if (isCompleted) {
                    break;
                }
            }
        }

        private static ProtocolType DetermineProtocolType(EndPoint endPoint) {
            switch (endPoint) {
            case UnixDomainSocketEndPoint _:
                return ProtocolType.Unspecified;
            default:
                return ProtocolType.Tcp;
            }
        }
    }
}
