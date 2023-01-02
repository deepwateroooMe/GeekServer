// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.IO.Pipelines {

// 这是传说中的　双全工　吗？　双全工　到底是什么意思呢？    
    internal class DuplexPipe : IDuplexPipe {

        public DuplexPipe(PipeReader reader, PipeWriter writer) {
            Input = reader;
            Output = writer;
        }

        public PipeReader Input { get; }
        public PipeWriter Output { get; }

        public static DuplexPipePair CreateConnectionPair(PipeOptions inputOptions, PipeOptions outputOptions) {
            var input = new Pipe(inputOptions);
            var output = new Pipe(outputOptions);
            var transportToApplication = new DuplexPipe(output.Reader, input.Writer);
            var applicationToTransport = new DuplexPipe(input.Reader, output.Writer);
            return new DuplexPipePair(applicationToTransport, transportToApplication);
        }

        // This class exists to work around issues with value tuple on .NET Framework
        public readonly struct DuplexPipePair {
            public IDuplexPipe Transport { get; }
            public IDuplexPipe Application { get; }
            public DuplexPipePair(IDuplexPipe transport, IDuplexPipe application) {
                Transport = transport;
                Application = application;
            }
        }
    }
}