window.monacoInterop = {
    editorInstances: {},
    editorInstanceTracker: {},
    editorInstanceInitialContent: {},
    editorInstanceHasChanges: {},
    dotnetReference: null,
    isMonacoInteropScriptLoaded: true,
    isMonacoLoaderScriptLoaded: false,
    testIsMonacoLoaded: (() => {
        return !!(window.monaco && window.monaco.editor);
    }),
    monacoConfigPath: '',
    monacoLoaderSource: "local",
    setMonacoLoaderSource: async (newSource) => {
        if (newSource !== undefined) {
            window.monacoInterop.monacoLoaderSource = newSource;
        }
        await window.monacoInterop.addMonacoLoaderToDom();
    },
    addMonacoLoaderToDom: function () {
        return new Promise((resolve, reject) => {
            let scriptName = "vs/loader";
            let scriptTag = document.querySelector(`script[src*="{scriptName}"]`)
            const inDom = (scriptTag !== null);
            if (!inDom) {
                let loaderLocation = monacoInterop.monacoLoaderSource;
                scriptTag = document.createElement("script");
                if (loaderLocation.substring(0, 4) !== "http") {
                    const importMap = JSON.parse(document.querySelector('script[type="importmap"]').textContent).imports;
                    const partialMatchKey = Object.keys(importMap).find(key => key.includes("loader.js"));

                    if (!partialMatchKey) {
                        script.onerror = () => reject(new Error('Failed to locate loader source in importmap: loader.js'));
                    }
                    loaderLocation = importMap[partialMatchKey].replace("./", "/");
                    window.monacoInterop.monacoLoaderSource = loaderLocation;
                }
                scriptTag.src = loaderLocation;
                scriptTag.onload = () => {
                    const regex = /\/loader\.js$/i;
                    const configPath = window.monacoInterop.monacoLoaderSource.replace(regex, '');
                    require.config({paths: {'vs': configPath}});
                    require(['vs/editor/editor.main'], function () {
                        window.monacoInterop.monacoEditorScriptLoaded = true;
                        resolve(true);
                    });
                    resolve(true);
                };
                document.head.appendChild(scriptTag);
            }
        });
    },

    initializeMonacoEditorInstance: async (elementId, initialCode, editorOptions, dotnetReference) => {
        if (monacoInterop.editorInstances[elementId]) {
            monacoInterop.editorInstances[elementId].dispose();
        }
        monacoInterop.dotnetReference = dotnetReference;
        monacoInterop.editorInstanceTracker[elementId] = {
            netReference: dotnetReference,
            hasChanges: false,
            initialCode: initialCode
        };

        let retryCounter = 0;
        const checkMonacoLoaded = () => {
            console.log("checkMonacoLoaded");
            if (window.monaco && window.monaco.editor) {

                const editor = monaco.editor.create(
                    document.getElementById(elementId),
                    JSON.parse(editorOptions)
                );
                editor.setValue(initialCode);
                editor.onDidChangeModelContent(function () {
                    let content = editor.getValue();
                    let editorElementId = editor._domElement["id"];

                    if (monacoInterop.editorInstanceTracker[elementId].initialCode === content) {
                        monacoInterop.editorInstanceTracker[elementId].hasChanges = false;
                        console.log("changed. haschanges: " + monacoInterop.editorInstanceTracker[elementId].hasChanges);
                        return monacoInterop.editorInstanceTracker[elementId].netReference.invokeMethodAsync('OnEditorContentChanged', false);
                    }
                    if (!monacoInterop.editorInstanceTracker[elementId].hasChanges) {
                        monacoInterop.editorInstanceTracker[elementId].hasChanges = true;
                        console.log("changed. haschanges: " + monacoInterop.editorInstanceTracker[elementId].hasChanges);
                        return monacoInterop.editorInstanceTracker[elementId].netReference.invokeMethodAsync('OnEditorContentChanged', true);
                    }
                });
                editor.addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyCode.KeyS, function() {
                    monacoInterop.editorInstanceTracker[elementId].netReference.invokeMethodAsync('OnEditorSaveRequest');
                });
                monacoInterop.editorInstances[elementId] = editor;

            } else {
                retryCounter++;
                if (retryCounter < 200) {
                    setTimeout(checkMonacoLoaded, 100);
                } else {
                    console.warn("Failed to load editor " + elementId);
                }
            }
        };
        checkMonacoLoaded();
    },
    getEditorContent: function (elementId, resetChangedOnRead) {
        const editor = monacoInterop.editorInstances[elementId];
        if (editor) {
            let editorValue = editor.getValue();
            if (resetChangedOnRead) {
                monacoInterop.editorInstanceTracker[elementId].hasChanges = false;
                monacoInterop.editorInstanceTracker[elementId].initialCode = editorValue;
            }
            return editorValue;
        }
    },
    setEditorContent: function (elementId, newContent) {
        const editor = monacoInterop.editorInstances[elementId];
        if (editor) {
            monacoInterop.editorInstanceTracker[elementId].hasChanges = false;
            monacoInterop.editorInstanceTracker[elementId].initialCode = newContent;
            return editor.setValue(newContent);
        }
        return null;
    },
    updateEditorConfiguration: function (elementId, newConfiguration) {
        const editor = monacoInterop.editorInstances[elementId];
        if (editor) {
            editor.updateOptions(JSON.parse(newConfiguration));
            monacoInterop.editorInstances[elementId] = editor;
        }
        return null;
    }
};