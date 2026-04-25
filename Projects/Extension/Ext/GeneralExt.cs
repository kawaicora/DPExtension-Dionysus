using System;
using DynamicPatcher;

namespace Extension.Ext
{
    [Serializable]
    public partial class GeneralExt
    {
        public static Action  LMouseDownCallback =() =>{};
        public static Action  LMouseUpCallback =() =>{};
        public static Action  RMouseDownCallback =() =>{};
        public static Action  RMouseUpCallback =() =>{};
        static public unsafe UInt32 LMouseDown(REGISTERS* R)
        {
            try
            {
                LMouseDownCallback?.Invoke();
            }
            catch(Exception ex){
                Logger.PrintException(ex);
            }

            return 0;
        }

        static public unsafe UInt32 LMouseUp(REGISTERS* R)
        {
            
            try
            {
                LMouseUpCallback?.Invoke();
            }
            catch(Exception ex){
                Logger.PrintException(ex);
            }
            return 0;
        }

        static public unsafe UInt32 RMouseDown(REGISTERS* R)
        {
            try
            {
                RMouseDownCallback?.Invoke();
            }
            catch(Exception ex){
                Logger.PrintException(ex);
            }

            return 0;
        }

        static public unsafe UInt32 RMouseUp(REGISTERS* R)
        {
            try
            {
                RMouseUpCallback?.Invoke();
            }
            catch(Exception ex){
                Logger.PrintException(ex);
            }

            return 0;
        }
    }

}