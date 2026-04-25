using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extension.Ext;

namespace Extension.EventSystems
{
    // Register Systems here


    public abstract partial class EventSystem
    {
        public static GeneralEventSystem General { get; }
        public static PointerExpireEventSystem PointerExpire { get; }
        public static SaveGameEventSystem SaveGame { get; }


        private static event EventHandler OnClearTemporaryHandler;

        static EventSystem()
        {
            General = new GeneralEventSystem();
            
            General.AddPermanentHandler(General.ScenarioClearClassesEvent, (sender, e) =>
            {
                OnClearTemporaryHandler?.Invoke(sender, e);
            });
            General.AddPermanentHandler(General.ScenarioStartEvent,SessionExt.OnScenarioStart);
            EventSystem.General.AddPermanentHandler(EventSystem.General.LogicClassUpdateEvent, SessionExt.OnLogicClassUpdate);
            EventSystem.General.AddPermanentHandler(EventSystem.General.ScenarioClearClassesEvent, SessionExt.OnScenarioClearClasses);
            PointerExpire = new PointerExpireEventSystem();
            SaveGame = new SaveGameEventSystem();
        }


        private void Register()
        {
            OnClearTemporaryHandler += ClearTemporaryHandler;
        }
        private void Unregister()
        {
            OnClearTemporaryHandler -= ClearTemporaryHandler;
        }
    }
}
