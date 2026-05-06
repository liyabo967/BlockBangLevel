// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("KKvmtLPB9oEHvouiVTzgaO/aguC9xuSfbbhLVR9274aAdkH+3IssNBelJgUXKiEuDaFvodAqJiYmIickyROOy1IiE5J7e30RldU69G/5WHKMK0VI3WqoLEoDqdJ2dAG4IIIX7euwKRkfY69+XwF5C2xtr3rrP+zN7bXhSPA47+WbCO+aKbOoVvSq4V4Wy0gCiUtSAz2lX8Qz8UIH4MUz/ebUfBMfq8vCmCCwA2zf+Driz49B143mEqshO10gDgDGrTEVaoT2/kMOuonVbE2Y7JDG2xAwIMyctymwPK3y8hUfEbuX73KrR9b/5PwvMwH41rnMINKw6mfXTKwOyxfjRR00huOlJignF6UmLSWlJiYn61jDm+HPJWbxHw8D40j9ViUkJicm");
        private static int[] order = new int[] { 11,13,11,8,5,11,6,10,10,9,10,12,12,13,14 };
        private static int key = 39;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
