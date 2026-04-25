using DynamicPatcher;

using PatcherYRpp;
using System;

using Extension.CoraExtension;
using Extension.EventSystems;
using System.Threading.Tasks;

namespace Extension.Ext
{
    [Serializable]
    public partial class SessionExt
    {
        static bool isGameBattleFirstResume = false;

        SessionExt()
        {
            Logger.Log("SessionExt instance created.");
            
        }

        public static unsafe UInt32 Resume(REGISTERS* R)
        {
            try
            {
                Pointer<SessionClass> pSession = (IntPtr)R->ECX;
                if (!isGameBattleFirstResume)
                {
                    isGameBattleFirstResume = true;
                    GameBattleFirstResume?.Invoke();
                }
                Logger.Log("SessionClass ResumeCallback invoked.");

                
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
   
            }
            return 0;
        }
        public static Action Destroy = ()=>{};


        static public unsafe UInt32 ExitGameLoop(REGISTERS* R) {

            try
            {

                Logger.Log("SessionClass ExitGameLoopCallback invoked.");
                
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
                
            }
            return 0;
        }


        
        public static Action GameBattleFirstResume;
        public static Action Start;
        public static Action Update;
        public static Action UpdateLate;

        public static void OnLogicClassUpdate(object sender, EventArgs e)
        {
            if (e is LogicClassUpdateEventArgs args )
            {
                if (args.IsBeginUpdate)
                {
                    Update?.Invoke();
                }
                else
                {
                    UpdateLate?.Invoke();
                }

                
            }
        }
    

        public static void OnScenarioStart(object sender, EventArgs e)
        {
            var _ = MainTools.Instance;
            Start?.Invoke();
            
            Logger.Log("Scenario Start");
        }

        public static void OnScenarioClearClasses(object sender, EventArgs e)
        {
            isGameBattleFirstResume = false;
            Destroy?.Invoke();
            Logger.Log("Scenario Clear Classes");
        }
    }
}