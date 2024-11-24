namespace Blazor.Monaco;

public static class MonacoDefaultScript
{
    public static readonly Dictionary<MonacoLanguage, string> Get = new()
    {
        {MonacoLanguage.PlainText, @"Sample Text"},
        {MonacoLanguage.CSharp, """
                                // C# Sample
                                Console.WriteLine("Hello World from C#");
                                """},
        {MonacoLanguage.JavaScript, """
                                    // JavaScript Sample
                                    console.log('Hello World from JavaScript');
                                    """},
        {MonacoLanguage.TypeScript, """
                                    // TypeScript Sample
                                    console.log('Hello World from TypeScript');
                                    """},
        {MonacoLanguage.HTML, """
                              <!-- HTML Sample -->
                              <p>Hello World from HTML</p>
                              """},
        {MonacoLanguage.CSS, """
                             /* CSS Sample */
                             body { font-family: Arial, sans-serif; }
                             """},
        {MonacoLanguage.JSON, """
                              {
                                "message": "Hello World from JSON"
                              }
                              """},
        {MonacoLanguage.XML, """
                             <!-- XML Sample -->
                             <message>Hello World from XML</message>
                             """},
        {MonacoLanguage.PowerShell, """
                                    # PowerShell Sample
                                    Write-Host "Hello World from PowerShell"
                                    """}
    };
}