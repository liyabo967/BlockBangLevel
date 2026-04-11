// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("h9xFdXMPwxIzbRVnAAHDFodTgKGB2Y0knFSDifdkg/ZF38Q6mMaNMnqnJG7lJz5vUckzqF+dLmuMqV+Re8lKaXtGTUJhzQPNvEZKSkpOS0jBnp55c33X+4Mexyu6k4iQQ19tlOBHKSSxBsRAJm/FvhoYbdRM7nuBYtbluQAh9ID8qrd8XEyg8NtF3FDJSkRLe8lKQUnJSkpLhzSv942jSbrVoEy+3IYLuyDAYqd7jylxWOqP0aqI8wHUJzlzGoPq7BotkrDnQFhEx4rY362a7WvS5845UIwEg7bujIq4EH9zx6eu9EzcbwCzlFaOo+Mtu+GKfsdNVzFMYmyqwV15Buiaki+lf+KnPk5//hcXEX35uVaYA5U0Hgqdc2NvjySROklISktK");
        private static int[] order = new int[] { 2,6,10,10,12,5,10,10,12,13,11,11,13,13,14 };
        private static int key = 75;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
