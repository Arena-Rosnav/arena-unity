using UnityEngine;

namespace RestServer.Helper {
    public class CICD {
        public static YieldInstruction SafeWaitForEndOfFrame() {
            // In batch mode there is no WaitForEndOfFrame (as there is no graphic device), so we have to use a different yield instruction 
            if (Application.isBatchMode) {
                return new WaitForSeconds(0.02f);
            }

            return new WaitForEndOfFrame();
        }
    }
}