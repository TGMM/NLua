using System;
using LuaDebug = LuaNET.Lua51.Lua.lua_Debug;

namespace NLua.Event
{
    /// <summary>
    /// Event args for hook callback event
    /// </summary>
    public class DebugHookEventArgs : EventArgs
    {
        public DebugHookEventArgs(LuaDebug luaDebug)
        {
            LuaDebug = luaDebug;
        }

        /// <summary>
        /// Lua Debug Information
        /// </summary>
        public LuaDebug LuaDebug { get; }
    }
}