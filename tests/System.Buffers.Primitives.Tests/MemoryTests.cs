// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Runtime;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xunit;

namespace System.Buffers.Tests
{
    public class BufferHandleTests
    {
        [Fact]
        public void MemoryHandleFreeUninitialized()
        {
            var handle = default(BufferHandle);
            handle.Dispose();
        }
    }
}
