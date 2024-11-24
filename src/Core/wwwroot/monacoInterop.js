window.monacoInterop = {
    editorInstances: {},
    editorInstanceTracker: {},
    editorInstanceInitialContent: {},
    editorInstanceHasChanges: {},
    dotnetReference: null,
    isMonacoInteropScriptLoaded: true,
    isMonacoLoaderScriptLoaded: false,
    monacoConfigPath: '',


    initializeMonacoEditorInstance: async (elementId, initialCode, language, dotnetReference) => {
        console.log(elementId, initialCode, language);

        if (monacoInterop.editorInstances[elementId]) {
            monacoInterop.editorInstances[elementId].dispose();
        }
        monacoInterop.dotnetReference = dotnetReference;
        monacoInterop.editorInstanceTracker[elementId] = {
            netReference: dotnetReference,
            hasChanges: false,
            initialCode: initialCode
        };
        console.table(monacoInterop.editorInstanceTracker[elementId]);
        let retryCounter = 0;
        const checkMonacoLoaded = () => {
            console.log("checkMonacoLoaded");
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

                    if (monacoInterop.editorInstanceTracker[elementId].initialCode === content) {
                        monacoInterop.editorInstanceTracker[elementId].hasChanges = false;
                        console.log("trigger callback")
                        return monacoInterop.editorInstanceTracker[elementId].netReference.invokeMethodAsync('OnEditorContentChanged', false);
                    }
                    if (!monacoInterop.editorInstanceTracker[elementId].hasChanges) {
                        monacoInterop.editorInstanceTracker[elementId].hasChanges = true;
                        console.log("trigger callback")
                        return monacoInterop.editorInstanceTracker[elementId].netReference.invokeMethodAsync('OnEditorContentChanged', true);
                    }
                });
                monacoInterop.editorInstances[elementId] = editor;

            } else {
                retryCounter++;
                
                if (retryCounter < 200) {
                    setTimeout(checkMonacoLoaded, 100);
                } else {
                    console.warn("Failed to load editor " + elementId );
                }
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