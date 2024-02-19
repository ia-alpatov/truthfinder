namespace TruthFinder.Parsers;

public interface INewsParser
{
    public string GetTextByUrl(string url);

    public bool IsUrlRight(string url);
}