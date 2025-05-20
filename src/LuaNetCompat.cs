namespace NLua;

using System;
using L = LuaNET.Lua51.Lua;
using LuaState = LuaNET.Lua51.lua_State;
using LuaNativeFunction = LuaNET.Lua51.Lua.lua_CFunction;

public static class LuaNetCompat
{
    public static int GetTop(this LuaState s)
    {
        return L.lua_gettop(s);
    }

    public static void SetTop(this LuaState s, int newTop)
    {
        L.lua_settop(s, newTop);
    }

    public static int PushThread(this LuaState s)
    {
        return L.lua_pushthread(s);
    }

    public static void PushString(this LuaState s, string str)
    {
        L.lua_pushstring(s, str);
    }

    public static void PushNil(this LuaState s)
    {
        L.lua_pushnil(s);
    }

    public static void PushBoolean(this LuaState s, bool b)
    {
        L.lua_pushboolean(s, b ? 1 : 0);
    }

    public static void AtPanic(this LuaState s, LuaNativeFunction panic)
    {
        L.lua_atpanic(s, panic);
    }

    public static void GetTable(this LuaState s, int index)
    {
        L.lua_gettable(s, index);
    }

    public static bool ToBoolean(this LuaState s, int index)
    {
        return L.lua_toboolean(s, index) != 0;
    }

    public static void GetRef(this LuaState s, int index)
    {
        L.lua_getref(s, index);
    }

    public static void GetGlobal(this LuaState s, string name)
    {
        L.lua_getglobal(s, name);
    }

    public static void SetGlobal(this LuaState s, string name)
    {
        L.lua_setglobal(s, name);
    }

    public static void SetTable(this LuaState s, int index)
    {
        L.lua_settable(s, index);
    }

    public static void XMove(this LuaState from, LuaState to, int n)
    {
        L.lua_xmove(from, to, n);
    }

    public static LuaState NewThread(this LuaState s)
    {
        return L.lua_newthread(s);
    }

    public static void Close(this LuaState s)
    {
        L.lua_close(s);
    }

    public static void NewTable(this LuaState s)
    {
        L.lua_newtable(s);
    }

    public static void DoString(this LuaState s, string str)
    {
        L.luaL_dostring(s, str);
    }

    public static KeraLua.LuaStatus LoadFile(this LuaState s, string filename)
    {
        return (KeraLua.LuaStatus)L.luaL_loadfile(s, filename);
    }

    public static KeraLua.LuaStatus LoadBuffer(this LuaState s, byte[] chunk, string chunkName)
    {
        return (KeraLua.LuaStatus)L.luaL_loadbuffer(s, System.Text.Encoding.ASCII.GetString(chunk), (ulong)chunk.Length, chunkName);
    }

    public static KeraLua.LuaStatus PCall(this LuaState s, int nArgs, int nResults, int errFunc)
    {
        return (KeraLua.LuaStatus)L.lua_pcall(s, nArgs, nResults, errFunc);
    }

    public static KeraLua.LuaStatus LoadString(this LuaState s, string chunk)
    {
        return (KeraLua.LuaStatus)L.luaL_loadstring(s, chunk);
    }

    public static LuaState ToThread(this LuaState s, int index)
    {
        return L.lua_tothread(s, index);
    }

    public static string GetUpValue(this LuaState s, int funcIndex, int upIndex)
    {
        return L.lua_getupvalue(s, funcIndex, upIndex);
    }

    public static string SetUpValue(this LuaState s, int funcIndex, int upIndex)
    {
        return L.lua_setupvalue(s, funcIndex, upIndex);
    }

    public static bool AreEqual(this LuaState s, int index1, int index2)
    {
        return L.lua_equal(s, index1, index2) != 0;
    }
}
