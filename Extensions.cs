using System.Text;

namespace EsmLib3;

public static class Extensions
{
    public static uint FourCC(this string str)
    {
        if (str.Length != 4)
            throw new ArgumentException("String length must be 4");

        return (uint)str[3] << 24 | (uint)str[2] << 16 | (uint)str[1] << 8 | str[0];
    }

    public static string ToMagic(this RecordName recordName)
    {
        return Encoding.ASCII.GetString(BitConverter.GetBytes((int)recordName));
    }
}