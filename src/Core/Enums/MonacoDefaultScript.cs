namespace Blazor.Monaco;

internal static class MonacoDefaultScript
{
    public static readonly Dictionary<Language, string> Get = new()
    {
        {Language.PlainText, @"Sample Text"},
        {
            Language.Markdown, """
                                # Markdown Sample
                                ## Sub-heading
                                This is a sample Markdown file.
                                <!-- html madness -->
                                <div class="custom-class" markdown="1">
                                  <div>
                                    nested div
                                  </div>
                                  <script type='text/x-koka'>
                                    function( x: int ) { return x*x; }
                                  </script>
                                  This is a div _with_ underscores
                                  and a & <b class="bold">bold</b> element.
                                  <style>
                                    body { font: "Consolas" }
                                  </style>
                                </div>
                                """
            
        },
        {Language.CSharp, """
                                // C# Sample
                                Console.WriteLine("Hello World from C#");
                                """},
        {Language.JavaScript, """
                                    // JavaScript Sample
                                    console.log('Hello World from JavaScript');
                                    """},
        {Language.TypeScript, """
                                    // TypeScript Sample
                                    console.log('Hello World from TypeScript');
                                    """},
        {Language.Html, """
                              <!-- HTML Sample -->
                              <p>Hello World from HTML</p>
                              """},
        {Language.Css, """
                             /* CSS Sample */
                             body { font-family: Arial, sans-serif; }
                             """},
        {Language.Json, """
                              {
                                "message": "Hello World from JSON"
                              }
                              """},
        {Language.Xml, """
                             <!-- XML Sample -->
                             <message>Hello World from XML</message>
                             """},
        {Language.PowerShell, """
                                    # PowerShell Sample
                                    Write-Host "Hello World from PowerShell"
                                    """}
    };
}