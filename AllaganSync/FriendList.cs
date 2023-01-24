/* Copyright XivCommon
 * MIT License
 * 
 * Copyright (c) 2021 Anna Clemens
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 *     of this software and associated documentation files (the "Software"), to deal
 *     in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 *     furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 *     copies or substantial portions of the Software.
 * 
 *     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *     IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *     AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */


using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace AllaganSync
{
    /// <summary>
    /// The class containing friend list functionality
    /// </summary>
    public class FriendList
    {
        // Updated: 5.58-HF1
        private const int InfoOffset = 0x28;
        private const int LengthOffset = 0x10;
        private const int ListOffset = 0x98;

        /// <summary>
        /// <para>
        /// A live list of the currently-logged-in player's friends.
        /// </para>
        /// <para>
        /// The list is empty if not logged in.
        /// </para>
        /// </summary>
        public static unsafe IList<FriendListEntry> List
        {
            get
            {
                var friendListAgent = (nint)Framework.Instance()->GetUiModule()->GetAgentModule()->GetAgentByInternalId(AgentId.SocialFriendList);
                if (friendListAgent == IntPtr.Zero)
                {
                    return Array.Empty<FriendListEntry>();
                }

                var info = *(nint*)(friendListAgent + InfoOffset);
                if (info == IntPtr.Zero)
                {
                    return Array.Empty<FriendListEntry>();
                }

                var length = *(ushort*)(info + LengthOffset);
                if (length == 0)
                {
                    return Array.Empty<FriendListEntry>();
                }

                var list = *(nint*)(info + ListOffset);
                if (list == IntPtr.Zero)
                {
                    return Array.Empty<FriendListEntry>();
                }

                var entries = new List<FriendListEntry>(length);
                for (var i = 0; i < length; i++)
                {
                    var entry = *(FriendListEntry*)(list + i * FriendListEntry.Size);
                    entries.Add(entry);
                }

                return entries;
            }
        }
    }

    /// <summary>
    /// An entry in a player's friend list.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = Size)]
    public unsafe struct FriendListEntry
    {
        internal const int Size = 96;

        /// <summary>
        /// The content ID of the friend.
        /// </summary>
        [FieldOffset(0x00)]
        public readonly ulong ContentId;

        /// <summary>
        /// The current world of the friend.
        /// </summary>
        [FieldOffset(0x16)]
        public readonly ushort CurrentWorld;

        /// <summary>
        /// The home world of the friend.
        /// </summary>
        [FieldOffset(0x18)]
        public readonly ushort HomeWorld;

        /// <summary>
        /// The job the friend is currently on.
        /// </summary>
        [FieldOffset(0x21)]
        public readonly byte Job;

        /// <summary>
        /// The friend's raw SeString name. See <see cref="Name"/>.
        /// </summary>
        [FieldOffset(0x22)]
        public fixed byte RawName[32];

        /// <summary>
        /// The friend's raw SeString free company tag. See <see cref="FreeCompany"/>.
        /// </summary>
        [FieldOffset(0x42)]
        public fixed byte RawFreeCompany[5];

        /// <summary>
        /// The friend's name.
        /// </summary>
        public SeString Name
        {
            get
            {
                fixed (byte* ptr = RawName)
                {
                    return MemoryHelper.ReadSeStringNullTerminated((IntPtr)ptr);
                }
            }
        }

        /// <summary>
        /// The friend's free company tag.
        /// </summary>
        public SeString FreeCompany
        {
            get
            {
                fixed (byte* ptr = RawFreeCompany)
                {
                    return MemoryHelper.ReadSeStringNullTerminated((IntPtr)ptr);
                }
            }
        }
    }
}
