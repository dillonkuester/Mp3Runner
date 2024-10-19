
using System.IO;
using System.Text.RegularExpressions;

public class Helpers
{
    public static string ConvertKeyToString(int key, int mode)
    {
        var keyMap = new Dictionary<int, string>
    {
        { 0, "C" }, { 1, "C#/Db" }, { 2, "D" }, { 3, "D#/Eb" },
        { 4, "E" }, { 5, "F" }, { 6, "F#/Gb" }, { 7, "G" },
        { 8, "G#/Ab" }, { 9, "A" }, { 10, "A#/Bb" }, { 11, "B" }
    };

        if (key < 0 || key > 11)
        {
            return "Unknown Key";
        }

        string modeString = mode == 1 ? "Major" : mode == 0 ? "Minor" : "Unknown Mode";
        return keyMap.TryGetValue(key, out var keyString) ? $"{keyString} {modeString}" : "Unknown Key";
    }

    public static string RemoveExistingBpmKeyPrefix(string fileName)
    {
        var regex = new Regex(@"^\d+_[A-G]#?_.*_");
        return regex.Replace(fileName, "");
    }
    public static string CleanFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        foreach (var c in invalidChars)
        {
            fileName = fileName.Replace(c.ToString(), "_");
        }
        return fileName;
    }

}
