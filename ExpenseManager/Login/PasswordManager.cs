using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;

namespace ExpenseManager.Login;

public static class PasswordManager {

    public static byte[] HashSecureString(SecureString input) {
        var bstr = Marshal.SecureStringToBSTR(input);
        var length = Marshal.ReadInt32(bstr, -4);
        var bytes = new byte[length];

        var bytesPin = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        try {
            Marshal.Copy(bstr, bytes, 0, length);
            Marshal.ZeroFreeBSTR(bstr);
            return SHA256.HashData(bytes);
        } finally {
            for (var i = 0; i < bytes.Length; i++) {
                bytes[i] = 0;
            }
            bytesPin.Free();
        }
    }

    public static bool ComparePasswords(SecureString input, string hash) {
        byte[] cmp = Convert.FromBase64String(hash);

        var bstr = Marshal.SecureStringToBSTR(input);
        var length = Marshal.ReadInt32(bstr, -4);
        var bytes = new byte[length];

        var bytesPin = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        try {
            Marshal.Copy(bstr, bytes, 0, length);
            Marshal.ZeroFreeBSTR(bstr);
            return SHA256.HashData(bytes).SequenceEqual(cmp);
        } finally {
            for (var i = 0; i < bytes.Length; i++) {
                bytes[i] = 0;
            }
            bytesPin.Free();
        }
    }
}