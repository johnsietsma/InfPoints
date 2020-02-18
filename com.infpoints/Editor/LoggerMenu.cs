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
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, Logger.LOGGER_SYMBOL);
        }

        [MenuItem(kDebuggerMenu, true)]
        static bool SwitchJobsDebuggerValidate()
        {
            bool enabled =  PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup).Contains(Logger.LOGGER_SYMBOL);
            Menu.SetChecked(kDebuggerMenu, enabled);
            return true;
        }
        
    }
}