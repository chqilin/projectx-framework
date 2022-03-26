
namespace UniLua
{
	internal class LuaOSLib
	{
		public const string LIB_NAME = "os";

		public static int OpenLib( ILuaState lua )
		{
			NameFuncPair[] define = new NameFuncPair[]
			{
				new NameFuncPair("clock", 	OS_Clock),
			};

			lua.L_NewLib( define );
			return 1;
		}

		private static int OS_Clock( ILuaState lua )
		{
            // rokyado : support windows phone 8
			lua.PushNumber( UnityEngine.Time.realtimeSinceStartup );
			return 1;
		}
	}
}

