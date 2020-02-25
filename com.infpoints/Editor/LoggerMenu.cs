using System.Diagnostics;
using InfPoints;
using UnityEditor;

namespace Infpoints.Editor
{
    public static class LoggerMenu
    {
        const string kDebuggerMenu = "Logger/Logging Enabled";
        static bool isLoggingEnabled;

        static LoggerMenu()
        {
            string defines =  PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            isLoggingEnabled = defines.Contains(Logger.LOGGER_SYMBOL);
        }

        [MenuItem(kDebuggerMenu, false)]
        static void ToggleLogging()
        {
            string defines =  PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if(isLoggingEnabled) PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defines.Replace(Logger.LOGGER_SYMBOL,""));
            else PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defines+";"+Logger.LOGGER_SYMBOL);
            isLoggingEnabled = !isLoggingEnabled;
        }

        [MenuItem(kDebuggerMenu, true)]
        static bool ToggleLoggingValidate()
        {
            Menu.SetChecked(kDebuggerMenu, isLoggingEnabled);
            return true;
        }
        
    }
}