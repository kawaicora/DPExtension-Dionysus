using System;
using System.Collections;

namespace Extension.Coroutines
{
    // ===================== 新增：协程管理器（模拟Unity Coroutine） =====================
    public class CoroutineUtils
    {
        public static IEnumerator WaitForSeconds(double seconds)
        {
            var end = DateTime.UtcNow + TimeSpan.FromSeconds(seconds);

            while (DateTime.UtcNow < end)
                yield return null;
        }
    }
    
}