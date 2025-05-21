namespace NLua.LuaNetCompat;

using System;
using System.Text;
using System.Runtime.InteropServices;

using NativeMethods = LuaNET.Lua51.Lua;
using LuaState = LuaNET.Lua51.lua_State;
using LuaNativeFunction = LuaNET.Lua51.Lua.lua_CFunction;
using System.Runtime.CompilerServices;

public class Lua
{
    private LuaState _luaState;
    private readonly Lua _mainState;

    /// <summary>
    /// Internal Lua handle pointer.
    /// </summary>
    public LuaState Handle => _luaState;

    /// <summary>
    /// Encoding for the string conversions
    /// ASCII by default.
    /// </summary>
    public Encoding Encoding { get; set; }

    /// <summary>
    ///  Returns a pointer to a raw memory area associated with the given Lua state. The application can use this area for any purpose; Lua does not use it for anything.
    ///  Each new thread has this area initialized with a copy of the area of the main thread. 
    /// </summary>
    /// <returns></returns>
    public IntPtr ExtraSpace => LuaStateHandle - IntPtr.Size;

    /// <summary>
    /// Get the main thread object, if the object is the main thread will be equal this
    /// </summary>
    public Lua MainThread => _mainState ?? this;

    /// <summary>
    /// Initialize Lua state, and open the default libs
    /// </summary>
    /// <param name="openLibs">flag to enable/disable opening the default libs</param>
    public Lua(bool openLibs = true)
    {
        Encoding = Encoding.ASCII;

        _luaState = NativeMethods.luaL_newstate();

        if (openLibs)
            OpenLibs();

        SetExtraObject(this, true);
    }

    public Lua(LuaState s)
    {
        this._luaState = s;
    }

    private IntPtr LuaStateHandle
    {
        get
        {
            return new IntPtr(BitConverter.ToInt64(BitConverter.GetBytes(_luaState.Handle), 0));
        }
    }

    public static Lua FromIntPtr(IntPtr luaState)
    {
        return new Lua(new LuaState
        {
            Handle = new UIntPtr(BitConverter.ToUInt64(BitConverter.GetBytes(luaState.ToInt64()), 0))
        });
    }

    private Lua(IntPtr luaThread, Lua mainState)
    {
        _mainState = mainState;
        _luaState = new LuaState
        {
            Handle = new UIntPtr(BitConverter.ToUInt64(BitConverter.GetBytes(luaThread.ToInt64()), 0))
        };
        Encoding = mainState.Encoding;

        SetExtraObject(this, false);
        GC.SuppressFinalize(this);
    }

    private Lua(LuaState luaThread, Lua mainState)
    {
        _mainState = mainState;
        _luaState = luaThread;
        Encoding = mainState.Encoding;

        SetExtraObject(this, false);
        GC.SuppressFinalize(this);
    }

    private void SetExtraObject<T>(T obj, bool weak) where T : class
    {
        var handle = GCHandle.Alloc(obj, weak ? GCHandleType.Weak : GCHandleType.Normal);
        IntPtr extraSpace = LuaStateHandle - IntPtr.Size;
        Marshal.WriteIntPtr(extraSpace, GCHandle.ToIntPtr(handle));
    }

    private static T GetExtraObject<T>(IntPtr luaState) where T : class
    {
        IntPtr extraSpace = luaState - IntPtr.Size;
        IntPtr pointer = Marshal.ReadIntPtr(extraSpace);
        var handle = GCHandle.FromIntPtr(pointer);
        if (!handle.IsAllocated)
            return null;

        return (T)handle.Target;
    }

    /// <summary>
    /// Opens all standard Lua libraries into the given state. 
    /// </summary>
    public void OpenLibs()
    {
        NativeMethods.luaL_openlibs(_luaState);
    }

    /// <summary>
    /// Raises an error. The error message format is given by fmt plus any extra arguments
    /// </summary>
    /// <param name="value"></param>
    /// <param name="v"></param>
    /// <returns></returns>
    public int Error(string value, params object[] v)
    {
        string message = string.Format(value, v);
        return NativeMethods.luaL_error(_luaState, message);
    }

    /// <summary>
    /// Generates a Lua error, using the value at the top of the stack as the error object. This function does a long jump
    /// (We want it to be inlined to avoid issues with managed stack)
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Error()
    {
        return NativeMethods.lua_error(_luaState);
    }

    public enum LuaType
    {
        None = NativeMethods.LUA_TNONE,
        Nil = NativeMethods.LUA_TNIL,
        Boolean = NativeMethods.LUA_TBOOLEAN,
        LightUserData = NativeMethods.LUA_TLIGHTUSERDATA,
        Number = NativeMethods.LUA_TNUMBER,
        String = NativeMethods.LUA_TSTRING,
        Table = NativeMethods.LUA_TTABLE,
        Function = NativeMethods.LUA_TFUNCTION,
        UserData = NativeMethods.LUA_TUSERDATA,
        Thread = NativeMethods.LUA_TTHREAD
    }


    public int GetTop()
    {
        return NativeMethods.lua_gettop(_luaState);
    }

    public void SetTop(int newTop)
    {
        NativeMethods.lua_settop(_luaState, newTop);
    }

    public int PushThread()
    {
        return NativeMethods.lua_pushthread(_luaState);
    }

    public void PushString(string str)
    {
        NativeMethods.lua_pushstring(_luaState, str);
    }

    public void PushNil()
    {
        NativeMethods.lua_pushnil(_luaState);
    }

    public void PushBoolean(bool b)
    {
        NativeMethods.lua_pushboolean(_luaState, b ? 1 : 0);
    }

    public void AtPanic(LuaNativeFunction panic)
    {
        NativeMethods.lua_atpanic(_luaState, panic);
    }

    public void GetTable(int index)
    {
        NativeMethods.lua_gettable(_luaState, index);
    }

    public bool ToBoolean(int index)
    {
        return NativeMethods.lua_toboolean(_luaState, index) != 0;
    }

    public void GetGlobal(string name)
    {
        NativeMethods.lua_getglobal(_luaState, name);
    }

    public void SetGlobal(string name)
    {
        NativeMethods.lua_setglobal(_luaState, name);
    }

    public void SetTable(int index)
    {
        NativeMethods.lua_settable(_luaState, index);
    }

    public void XMove(LuaState to, int n)
    {
        NativeMethods.lua_xmove(/* from */ _luaState, to, n);
    }

    public void XMove(Lua to, int n)
    {
        NativeMethods.lua_xmove(/* from */ _luaState, to.Handle, n);
    }

    public Lua NewThread()
    {
        var thread = NativeMethods.lua_newthread(_luaState);
        return new Lua(thread, this);
    }

    public void Close()
    {
        NativeMethods.lua_close(_luaState);
    }

    public void NewTable()
    {
        NativeMethods.lua_newtable(_luaState);
    }

    public void DoString(string str)
    {
        NativeMethods.luaL_dostring(_luaState, str);
    }

    public LuaStatus LoadFile(string filename)
    {
        return (LuaStatus)NativeMethods.luaL_loadfile(_luaState, filename);
    }

    public LuaStatus LoadBuffer(byte[] chunk, string chunkName)
    {
        return (LuaStatus)NativeMethods.luaL_loadbuffer(_luaState, System.Text.Encoding.ASCII.GetString(chunk), (ulong)chunk.Length, chunkName);
    }

    public LuaStatus PCall(int nArgs, int nResults, int errFunc)
    {
        return (LuaStatus)NativeMethods.lua_pcall(_luaState, nArgs, nResults, errFunc);
    }

    public LuaStatus LoadString(string chunk, string name)
    {
        byte[] buffer = Encoding.GetBytes(chunk);
        return LoadBuffer(buffer, name);
    }

    public Lua ToThread(int index)
    {
        return new Lua(NativeMethods.lua_tothread(_luaState, index));
    }

    public string GetUpValue(int funcIndex, int upIndex)
    {
        return NativeMethods.lua_getupvalue(_luaState, funcIndex, upIndex);
    }

    public string SetUpValue(int funcIndex, int upIndex)
    {
        return NativeMethods.lua_setupvalue(_luaState, funcIndex, upIndex);
    }

    public bool AreEqual(int index1, int index2)
    {
        return NativeMethods.lua_equal(_luaState, index1, index2) != 0;
    }

    public void PushInteger(long n)
    {
        NativeMethods.lua_pushinteger(_luaState, n);
    }

    public void PushNumber(double n)
    {
        NativeMethods.lua_pushnumber(_luaState, n);
    }

    public string ToString(int index)
    {
        return NativeMethods.lua_tostring(_luaState, index);
    }

    public string ToString(int index, bool _ /* Here for compat reasons, ignored */)
    {
        return this.ToString(index);
    }

    public int SetMetaTable(int index)
    {
        return NativeMethods.lua_setmetatable(_luaState, index);
    }

    public void PushCFunction(LuaNativeFunction f)
    {
        NativeMethods.lua_pushcfunction(_luaState, f);
    }

    public int NewMetaTable(string name)
    {
        return NativeMethods.luaL_newmetatable(_luaState, name);
    }

    public void RawSet(int index)
    {
        NativeMethods.lua_rawset(_luaState, index);
    }

    public double ToNumber(int index)
    {
        return NativeMethods.lua_tonumber(_luaState, index);
    }

    public bool GetMetaTable(int index)
    {
        return NativeMethods.lua_getmetatable(_luaState, index) != 0;
    }

    public void GetMetaTable(string tableName)
    {
        GetField(LuaRegistry.Index, tableName);
    }

    public string TypeName(LuaType type)
    {
        return NativeMethods.lua_typename(_luaState, (int)type);
    }

    public bool IsBoolean(int index) => Type(index) == LuaType.Boolean;

    public bool IsString(int index) => Type(index) == LuaType.String;

    public bool IsNumber(int index) => NativeMethods.lua_isnumber(_luaState, index) != 0;

    public void PushCopy(int index)
    {
        NativeMethods.lua_pushvalue(_luaState, index);
    }

    public void Remove(int index)
    {
        NativeMethods.lua_remove(_luaState, index);
    }

    public void RawSetInteger(int index, int n)
    {
        NativeMethods.lua_rawseti(_luaState, index, n);
    }

    public void RawSetInteger(int index, long n)
    {

        NativeMethods.lua_rawseti(_luaState, index, Convert.ToInt32(n));
    }

    public void RawGet(int index)
    {
        NativeMethods.lua_rawget(_luaState, index);
    }

    public void PushLightUserData(nuint udata)
    {
        NativeMethods.lua_pushlightuserdata(_luaState, udata);
    }

    public void PushLightUserData(nint udata)
    {
        NativeMethods.lua_pushlightuserdata(_luaState, new UIntPtr(BitConverter.ToUInt64(BitConverter.GetBytes(udata), 0)));
    }

    public bool IsNil(int index)
    {
        return NativeMethods.lua_isnil(_luaState, index) != 0;
    }

    public void RawGetInteger(int index, int n)
    {
        NativeMethods.lua_rawgeti(_luaState, index, n);
    }

    public LuaType Type(int index)
    {
        return (LuaType)NativeMethods.lua_type(_luaState, index);
    }

    public string SetLocal(NativeMethods.lua_Debug ar, int n)
    {
        return NativeMethods.lua_setlocal(_luaState, ar, n);
    }

    public string GetLocal(NativeMethods.lua_Debug ar, int n)
    {
        return NativeMethods.lua_getlocal(_luaState, ar, n);
    }

    public long ToInteger(int index)
    {
        return NativeMethods.lua_tointegerx(_luaState, index);
    }

    public int Ref(int index)
    {
        return NativeMethods.luaL_ref(_luaState, index);
    }

    public bool IsInteger(int index)
    {
        // ?
        return false;
    }

    public bool CheckStack(int size)
    {
        return NativeMethods.lua_checkstack(_luaState, size) != 0;
    }

    public void GetField(int index, string name)
    {
        NativeMethods.lua_getfield(_luaState, index, name);
    }

    public void Insert(int index)
    {
        NativeMethods.lua_insert(_luaState, index);
    }

    public void Where(int level)
    {
        NativeMethods.luaL_where(_luaState, level);
    }

    public bool GetInfo(string what, NativeMethods.lua_Debug ar)
    {
        return NativeMethods.lua_getinfo(_luaState, what, ar) != 0;
    }

    public void PushGlobalTable()
    {
        NativeMethods.lua_rawgeti(_luaState, NativeMethods.LUA_REGISTRYINDEX, NativeMethods.LUA_GLOBALSINDEX);
    }

    public IntPtr ToUserData(int index)
    {
        return new IntPtr(BitConverter.ToInt64(BitConverter.GetBytes(NativeMethods.lua_touserdata(_luaState, index)), 0));
    }

    public bool Next(int index) => NativeMethods.lua_next(_luaState, index) != 0;

    public void Unref(int t, int reference)
    {
        NativeMethods.luaL_unref(_luaState, t, reference);
    }

    public bool RawEqual(int index1, int index2)
    {
        return NativeMethods.lua_rawequal(_luaState, index1, index2) != 0;
    }

    public void Pop(int n) => NativeMethods.lua_settop(_luaState, -n - 1);

    /// <summary>
    /// Converts the Lua value at the given as byte array
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public byte[] ToBuffer(int index)
    {
        return ToBuffer(index, true);
    }
    /// <summary>
    /// Converts the Lua value at the given index to a byte array.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="callMetamethod">Calls __tostring field if present</param>
    /// <returns></returns>
    public byte[] ToBuffer(int index, bool _)
    {
        ulong len = 0;
        string result = NativeMethods.lua_tolstring(_luaState, index, ref len);

        if (len == 0)
            return [];

        return Encoding.GetBytes(result);
    }

    public LuaType GetMetaField(int obj, string field)
    {
        return (LuaType)NativeMethods.luaL_getmetafield(_luaState, obj, field);
    }
}

public static class LuaRegistry
{
    public const int Index = NativeMethods.LUA_REGISTRYINDEX;
    public const int Globals = NativeMethods.LUA_GLOBALSINDEX;
}

public static class LuaRegistryIndex
{
    public const int Index = NativeMethods.LUA_REGISTRYINDEX;
    public const int Globals = NativeMethods.LUA_GLOBALSINDEX;
}

/// <summary>
/// Lua Load/Call status return
/// </summary>
public enum LuaStatus
{
    /// <summary>
    ///  success
    /// </summary>
    OK = 0,
    /// <summary>
    /// Yield
    /// </summary>
    Yield = 1,
    /// <summary>
    /// a runtime error. 
    /// </summary>
    ErrRun = 2,
    /// <summary>
    /// syntax error during precompilation
    /// </summary>
    ErrSyntax = 3,
    /// <summary>
    ///  memory allocation error. For such errors, Lua does not call the message handler. 
    /// </summary>
    ErrMem = 4,
    /// <summary>
    ///  error while running the message handler. 
    /// </summary>
    ErrErr = 5,
}

/// <summary>
/// Used by Compare
/// </summary>
public enum LuaCompare
{
    /// <summary>
    ///  compares for equality (==)
    /// </summary>
    Equal = 0,
    /// <summary>
    ///  compares for less than 
    /// </summary>
    LessThen = 1,
    /// <summary>
    /// compares for less or equal 
    /// </summary>
    LessOrEqual = 2
}