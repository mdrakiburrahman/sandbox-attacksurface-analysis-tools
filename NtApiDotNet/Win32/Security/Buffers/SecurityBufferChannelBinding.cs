﻿//  Copyright 2021 Google Inc. All Rights Reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

using NtApiDotNet.Win32.Security.Authentication;
using NtApiDotNet.Win32.Security.Native;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace NtApiDotNet.Win32.Security.Buffers
{
    /// <summary>
    /// Security buffer for a channel binding.
    /// </summary>
    public sealed class SecurityBufferChannelBinding : SecurityBuffer
    {
        private readonly SecurityChannelBindings _channel_binding;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="channel_binding">The channel bindings.</param>
        public SecurityBufferChannelBinding(SecurityChannelBindings channel_binding)
            : base(SecurityBufferType.ChannelBindings | SecurityBufferType.ReadOnly)
        {
            _channel_binding = channel_binding ?? throw new ArgumentNullException(nameof(channel_binding));
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="channel_binding_token">The channel bindings token.</param>
        public SecurityBufferChannelBinding(byte[] channel_binding_token) 
            : this(new SecurityChannelBindings(channel_binding_token))
        {
        }

        /// <summary>
        /// Convert to buffer back to an array.
        /// </summary>
        /// <returns>The buffer as an array.</returns>
        public override byte[] ToArray()
        {
            MemoryStream stm = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stm);

            // Manual marshaling of SEC_CHANNEL_BINDINGS
            int current_ofs = Marshal.SizeOf<SEC_CHANNEL_BINDINGS>();
            writer.Write(_channel_binding.InitiatorAddrType);
            current_ofs = AddBuffer(writer, current_ofs, _channel_binding.Initiator?.Length);
            writer.Write(_channel_binding.AcceptorAddrType);
            current_ofs = AddBuffer(writer, current_ofs, _channel_binding.Acceptor?.Length);
            _ = AddBuffer(writer, current_ofs, _channel_binding.ApplicationData?.Length);

            WriteBuffer(writer, _channel_binding.Initiator);
            WriteBuffer(writer, _channel_binding.Acceptor);
            WriteBuffer(writer, _channel_binding.ApplicationData);

            return stm.ToArray();
        }

        private int AddBuffer(BinaryWriter writer, int current_ofs, int? length)
        {
            int next_length = length ?? 0;
            if (next_length == 0)
            {
                writer.Write(0);
                writer.Write(0);
            }
            else
            {
                writer.Write(current_ofs);
                current_ofs += next_length;
                writer.Write(next_length);
            }
            return current_ofs;
        }

        private void WriteBuffer(BinaryWriter writer, byte[] buffer)
        {
            writer.Write(buffer ?? Array.Empty<byte>());
        }

        internal override void FromBuffer(SecBuffer buffer)
        {
            return;
        }

        internal override SecBuffer ToBuffer(DisposableList list)
        {
            return SecBuffer.Create(SecurityBufferType.ChannelBindings | SecurityBufferType.ReadOnly, ToArray(), list);
        }
    }
}
