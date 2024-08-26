namespace NewFace.Helpers;

public static class Common
{
    public static string FormatPhoneNumber(string phone)
    {
        // remove letter if it is not number
        string cleanedNumber = new string(phone.Where(char.IsDigit).ToArray());

        // remove in front of '0' if it is
        cleanedNumber = cleanedNumber.TrimStart('0');

        // add +82
        return "+82" + cleanedNumber;
    }
}
