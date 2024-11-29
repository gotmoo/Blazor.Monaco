# Monaco Editor for Blazor components

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET C#](https://img.shields.io/badge/.NET-C%23-blue)](https://docs.microsoft.com/en-us/dotnet/csharp/)

**This package is for use in .NET 9 Blazor projects.**
## Introduction

The `Blazor.Monaco` package provides a [Blazor](https://blazor.net) component which adds the [Monaco Editor](https://github.com/microsoft/monaco-editor) to your Blazor pages. This is the same engine that powers [VS Code](https://github.com/microsoft/vscode). The package handles adding all the required components to your site and the interaction between your C# and JavaScript.


## Setup

To add to your project, add the package with dotnet.exe:
```powershell
dotnet add package Blazor.Monaco --prerelease
```

To install, add one line to your `program.cs`:
```csharp
builder.Services.AddBlazorMonacoComponents();
```
## Usage
To add to your page, simply add:
```csharp
<MonacoEditor 
    ElementId="editor-id" 
    Language="Language.JavaScript" 
    ScriptContent="@MyScript"/>
@code {
    private string MyScript { get; set; } = string.Empty;
}    
```

If the ScriptContent is null or empty, it will print add example text, relevant to the language.

### Component Parameters
- `ElementId`: The id for the editor component. If not set, a GUID is used.
- `ScriptContent`: The text that should be displayed.
- `Language`: Quick way to set the language for the editor. This changes how the contents are displayed.
- `EditorOptions`: Provide an options object here to specify any option. The Language property above will override the language set in the options. Use `private EditorOptions _editorOptions = new();` to initialize a new instance.
- `Style`: Apply this CSS styling to the editor component.
- `Class`: Apply this CSS class to the editor component.
- `OnEventCallback<bool> OnContentChanged`: This is fired the first time when content has changed from initial `ScriptContent`, and again when changes are manually reverted to original content.
- `EventCallback OnSaveRequested`: This is fired when the user presses Ctrl+S in the editor window.

If both the Style and Class are undefined, a fallback style of `min-height: 10em;` is applied. Be sure to provice the proper height for your pages. 

### Component Interaction
For two-way interaction with the Monaco Editor, such as getting the current contents, you need to apply a reference to the component tag and access its methods:

```csharp
@page "/YourPage"
@using Blazor.Monaco
@rendermode InteractiveServer
<MonacoEditor @ref="_monacoEditorInstance" />
@code {
    private MonacoEditor _monacoEditorInstance = null!;
}
```

#### Editor Methods
```csharp
@page "/YourPage"
@using Blazor.Monaco
@rendermode InteractiveServer
<MonacoEditor @ref="_editor" />
@code {
    private MonacoEditor _editor = null!;
    private EditorOptions _editorOptions = new();
    private async Task MyAction()
    {
        //Read current contents
        var contents = await _editor.GetEditorContent();
        //Set updated contents
        await _editor.SetEditorContent(contents);
        //update editor's configuration
        await _editor.UpdateEditorConfiguration(_editorOptions);
        //re-initialize the editor in the browser
        await _editor.ReInitializeEditor();
    }
}
```

#### Editor Callbacks
```csharp
@page "/YourPage"
@using Blazor.Monaco
@rendermode InteractiveServer
<MonacoEditor
    OnContentChanged="OnEditorContentChanged"
    OnSaveRequested="OnEditorSaveRequested"
    @ref="_editor" />
@code {
    private MonacoEditor _editor = null!;
    
    private void OnEditorContentChanged(bool hasChanged)
    {
        Console.WriteLine($"OnEditorContentChanged: {hasChanged}");
    }

    private async Task OnEditorSaveRequested()
    {
        Console.WriteLine("OnEditorSaveRequested");
        var contents = await _editor.GetEditorContent(resetChangedOnRead: true);
        // handle saving the contents
    }
}
```

## Notes
If the editor does not display at all, you may be missing interactive rendermode. Either add this with `@rendermode InteractiveServer` at the top of your page, or in `App.razor`:
```csharp
<!DOCTYPE html>
<html lang="en">
.... snip ....
<body>
    <Routes @rendermode="InteractiveServer"/>
    <script src="_framework/blazor.web.js"></script>
</body>

</html>
```