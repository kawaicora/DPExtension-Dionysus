using System;
using DynamicPatcher;

namespace Extension.Ext
{
    [Serializable]
    public partial class GScreenExt
    {
        public static Action  DrawOnTop_TheDarkSideOfTheMoonCallback =() =>{};
        static public unsafe UInt32 DrawOnTop_TheDarkSideOfTheMoon(REGISTERS* R)
        {
            try
            {
                DrawOnTop_TheDarkSideOfTheMoonCallback?.Invoke();
            }
            catch(Exception ex){
                Logger.PrintException(ex);
            }

            return 0;
        }


    }

}