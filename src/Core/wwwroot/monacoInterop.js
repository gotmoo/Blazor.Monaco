window.monacoInterop = {
    editorInstances: {},
    editorInstanceTracker: {},
    editorInstanceInitialContent: {},
    editorInstanceHasChanges: {},
    dotnetReference: null,
    isMonacoInteropScriptLoaded: true,
    isMonacoLoaderScriptLoaded: false,
    monacoConfigPath: '',
    

    loadMonacoLoaderScript: () => {
        return new Promise((resolve, reject) => {
            if (!monacoInterop.isMonacoLoaderScriptLoaded) {
                // const vsLoaderScript = document.createElement('script');
                // vsLoaderScript.src = "https://cdnjs.cloudflare.com/ajax/libs/monaco-editor/0.52.0/min/vs/loader.js";
                // // vsLoaderScript.src = "_content/Blazor.Monaco/min/vs/loader.js";
                // vsLoaderScript.onload = () => {
                    require.config({ paths: { 'vs': 'https://cdnjs.cloudflare.com/ajax/libs/monaco-editor/0.52.0/min/vs' } });
                    // require.config({paths: {'vs': '_content/Blazor.Monaco/min/vs'}});
                    require(['vs/editor/editor.main'], function () {
                        monacoInterop.isMonacoLoaderScriptLoaded = true;
                        resolve(true);
                    });
                // };
                // vsLoaderScript.onerror = () => reject(new Error("Failed to load Monaco Editor script."));
                // document.body.appendChild(vsLoaderScript);
            } else {
                resolve(true);
            }
        });
    },

    initializeMonacoEditorInstance: async (elementId, initialCode, language, dotnetReference) => {
        console.log(initialCode);

        if (monacoInterop.editorInstances[elementId]) {
            monacoInterop.editorInstances[elementId].dispose();
        }
        monacoInterop.dotnetReference = dotnetReference;
        monacoInterop.editorInstanceTracker[elementId] = {
            netReference: dotnetReference,
            hasChanges: false,
            initialCode: initialCode
        };
        const checkMonacoLoaded = () => {
            if (window.monaco && window.monaco.editor) {
                const editor = monaco.editor.create(
                    document.getElementById(elementId),
                    {
                        value: initialCode,
                        language: language,
                        theme: 'vs-dark'
                    }
                );
                editor.onDidChangeModelContent(function () {
                    let content = editor.getValue();
                    let editorElementId = editor._domElement["id"];
                    console.log("editorElementId: " + editorElementId);
                    let changesChanged = false;
                    
                    if (monacoInterop.editorInstanceTracker[elementId].initialCode===content){
                        monacoInterop.editorInstanceTracker[elementId].hasChanges = false;
                        return monacoInterop.editorInstanceTracker[elementId].netReference.invokeMethodAsync('OnEditorContentChanged', false);
                    }
                    if (!monacoInterop.editorInstanceTracker[elementId].hasChanges){
                        monacoInterop.editorInstanceTracker[elementId].hasChanges = true;
                        return monacoInterop.editorInstanceTracker[elementId].netReference.invokeMethodAsync('OnEditorContentChanged', true);
                    }
                });
                monacoInterop.editorInstances[elementId] = editor;

            } else {
                setTimeout(checkMonacoLoaded, 100);
            }
        };
        checkMonacoLoaded();
    },
    // // Set a callback function to be invoked when the content changes
    // setContentChangeCallback: function (elementId, dotNetHelper) {
    //     const editor = monacoInterop.editorInstances[elementId];
    //     if (editor) {
    //         editor.onDidChangeModelContent(() => {
    //             const content = editor.getValue();
    //             dotNetHelper.invokeMethodAsync('NotifyContentChange', content);
    //         });
    //     }
    // }, 
    getEditorContent: function (elementId) {
        const editor = monacoInterop.editorInstances[elementId];
        if (editor) {
            return editor.getValue();
        }
        return null;
    }, 
    setEditorContent: function (elementId, newContent) {
        const editor = monacoInterop.editorInstances[elementId];
        if (editor) {
            monacoInterop.editorInstanceTracker[elementId].hasChanges = false;
            monacoInterop.editorInstanceTracker[elementId].initialCode = newContent;
            return editor.setValue(newContent);
        }
        return null;
    }
};