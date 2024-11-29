# Monaco Editor for Blazor components

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET C#](https://img.shields.io/badge/.NET-C%23-blue)](https://docs.microsoft.com/en-us/dotnet/csharp/)

**This package is for use in .NET 9 Blazor projects.**
## Introduction

The `Blazor.Monaco` package provides a [Blazor](https://blazor.net) component which adds the [Monaco Editor](https://github.com/microsoft/monaco-editor) to your Blazor pages. This is the same engine that powers [VS Code](https://github.com/microsoft/vscode). The package handles adding all the required components to your site and the interaction between your C# and JavaScript.


## Setup

To install, add one line to your `program.cs`:
```
builder.Services.AddBlazorMonacoComponents();
```

To add to your page, simply add:
```
<MonacoEditor 
    ElementId="editor-id" 
    Language="Language.JavaScript" 
    ScriptContent="@MyScript"/>
@code {
    private string MyScript { get; set; } = string.Empty;
}    
```

If the ScriptContent is null or empty, it will print add example text, relevant to the language.


