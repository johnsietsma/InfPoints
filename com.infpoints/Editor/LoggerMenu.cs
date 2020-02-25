using System.Diagnostics;
using InfPoints;
using UnityEditor;

namespace Infpoints.Editor
{
    public static class LoggerMenu
    {
        const string kDebuggerMenu = "Logger/Logging Enabled";

        [MenuItem(kDebuggerMenu, false)]
        static void SwitchJobsDebugger()
        {
            string defines =  PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            bool enabled = defines.Contains(Logger.LOGGER_SYMBOL);
            if(enabled) PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defines.Replace(Logger.LOGGER_SYMBOL,""));
            else PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defines+Logger.LOGGER_SYMBOL);
            UnityEngine.Debug.Log("Defs:" + defines);
        }

        [MenuItem(kDebuggerMenu, true)]
        static bool SwitchJobsDebuggerValidate()
        {
            bool enabled =  PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup).Contains(Logger.LOGGER_SYMBOL);
            Menu.SetChecked(kDebuggerMenu, enabled);
            return enabled;
        }
        
    }
}