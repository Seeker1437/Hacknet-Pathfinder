﻿using System;
using System.Collections.Generic;
using Pathfinder.Event;
using Pathfinder.Util;

namespace Pathfinder.Daemon
{
    public static class Handler
    {
        internal static Dictionary<string, IInterface> idToInterface = new Dictionary<string, IInterface>();

        private static int modBacktrack = 3;

        public static bool RegisterDaemon(string id, IInterface inter)
        {
            id = Utility.GetId(id, frameSkip: modBacktrack, throwFindingPeriod: true);
            Logger.Verbose("Mod {0} attempting to add daemon interface {1} with id {2}",
                           Utility.GetPreviousStackFrameIdentity(modBacktrack-1),
                           inter.GetType().FullName,
                           id);
            if (idToInterface.ContainsKey(id))
                return false;

            idToInterface.Add(id, inter);
            return true;
        }

        [Obsolete("Use RegisterDaemon")]
        public static bool AddDaemon(string id, IInterface inter)
        {
            modBacktrack += 1;
            var b = RegisterDaemon(id, inter);
            modBacktrack = 3;
            return b;
        }

        internal static bool UnregisterDaemon(string id)
        {
            id = Utility.GetId(id);
            if (!idToInterface.ContainsKey(id))
                return true;
            return idToInterface.Remove(id);
        }

        public static bool ContainsDaemon(string id)
        {
            return idToInterface.ContainsKey(Utility.GetId(id));
        }

        public static IInterface GetDaemonById(string id)
        {
            id = Utility.GetId(id);
            IInterface i = null;
            idToInterface.TryGetValue(id, out i);
            return i;
        }

        internal static void DaemonLoadListener(LoadComputerXmlReadEvent e)
        {
            IInterface i;
            var id = e.Reader.GetAttribute("interfaceId");
            if (id != null && idToInterface.TryGetValue(id, out i))
            {
                var objs = new Dictionary<string, string>();
                var storedObjects = e.Reader.GetAttribute("storedObjects")?.Split(' ');
                if (storedObjects != null)
                    foreach (var s in storedObjects)
                        objs[s.Remove(s.IndexOf('|'))] = s.Substring(s.IndexOf('|') + 1);
                e.Computer.daemons.Add(Instance.CreateInstance(id, e.Computer, objs));
            }
        }
    }
}
