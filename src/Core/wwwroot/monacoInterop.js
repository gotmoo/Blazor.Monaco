window.monacoInterop = {
    initialized: false,
    isInitializing: false,
    editorInstances: {},
    editorInstanceTracker: {},
    editorInstanceInitialContent: {},
    editorInstanceHasChanges: {},
    dotnetReference: null,


    initialize: async function (loaderUrl) {
        if (this.initialized) {
            console.log("monacoInterop: Monaco Editor already initialized. Skipping reinitialization.");
            return; // Skip initialization if already initialized
        }
        if (this.isInitializing) {
            console.log("monacoInterop: Monaco Editor initialization in progress. Waiting for completion...");
            // Wait until initialization is complete
            while (this.isInitializing) {
                await new Promise(r => setTimeout(r, 100)); // Poll every 100ms
            }
            if (this.initialized) {
                console.log("monacoInterop: Monaco Editor already initialized after initialization wait. Skipping.");
                return;
            }
        }
        // Mark as initializing
        this.isInitializing = true;
        console.log("monacoInterop: Starting Monaco Editor initialization...");

        try {
            if (!loaderUrl) {
                console.error("cdnLoaderUrl must be provided to initialize Monaco Editor.");
                return;
            }
            if (!loaderUrl.endsWith("/loader.js")) {
                console.error(
                    `The provided loader url does not end with 'loader.js'. Check your configuration to ensure the correct loader is being used: ${loaderUrl}`
                );
            }

            const vsPath = loaderUrl.replace(/\/loader\.js$/, "");

            await this._loadMonacoScript(loaderUrl);

            for (let i = 0; i < 5; i++) {
                if (typeof require !== "undefined") {
                    break;
                }
                console.log("Waiting for require to be defined...");
                await new Promise(r => setTimeout(r, 200));
            }

            if (typeof require === "undefined") {
                throw new Error("'require' is not defined. Ensure loader.js is loaded properly.");
            }

            require.config({paths: {'vs': vsPath}});
            await new Promise((resolve, reject) => {
                require(['vs/editor/editor.main'], resolve, reject);
            });

            this.initialized = true;
            console.log("Monaco Editor initialized successfully.");
        } catch (error) {
            console.error("Monaco Editor initialization failed:", error);
        } finally {
            // Reset isInitializing, whether successful or not
            this.isInitializing = false;
        }
    },

    createEditor1: function (elementId, language, initialValue) {
        const element = document.getElementById(elementId);
        if (element) {
            element.editor = monaco.editor.create(element, {
                value: initialValue || "",
                language: language || "plaintext",
                automaticLayout: true
            });
        }
    },
    createEditor: (elementId, initialCode, editorOptions, dotnetReference) => {
        const element = document.getElementById(elementId);
        if (element) {
            if (monacoInterop.editorInstances[elementId]) {
                monacoInterop.editorInstances[elementId].dispose();
            }
            monacoInterop.dotnetReference = dotnetReference;
            monacoInterop.editorInstanceTracker[elementId] = {
                netReference: dotnetReference,
                hasChanges: false,
                initialCode: initialCode
            };
            element.editor = monaco.editor.create(
                document.getElementById(elementId),
                JSON.parse(editorOptions)
            );
            element.editor.setValue(initialCode);
            element.editor.onDidChangeModelContent(function () {
                let content = element.editor.getValue();
                let editorElementId = element.editor._domElement["id"];

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
            element.editor.addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyCode.KeyS, function () {
                monacoInterop.editorInstanceTracker[elementId].netReference.invokeMethodAsync('OnEditorSaveRequest');
            });
            monacoInterop.editorInstances[elementId] = element.editor;
        }
    },
    setEditorValue: function (elementId, value) {
        const element = document.getElementById(elementId);
        if (element && element.editor) {
            element.editor.setValue(value);
        }
    },
    getEditorValue: function (elementId) {
        const element = document.getElementById(elementId);
        if (element && element.editor) {
            return element.editor.getValue();
        }
        return "";
    },
    setEditorContent: function (elementId, newContent) {
        const element = document.getElementById(elementId);
        const editor = monacoInterop.editorInstances[elementId];
        if (element && editor) {
            monacoInterop.editorInstanceTracker[elementId].hasChanges = false;
            monacoInterop.editorInstanceTracker[elementId].initialCode = newContent;
            editor.setValue(newContent);
        }
    },
    getEditorContent: function (elementId, resetChangedOnRead) {
        const element = document.getElementById(elementId);
        const editor = monacoInterop.editorInstances[elementId];
        if (element && editor) {
            let editorValue = editor.getValue();
            if (resetChangedOnRead) {
                monacoInterop.editorInstanceTracker[elementId].hasChanges = false;
                monacoInterop.editorInstanceTracker[elementId].initialCode = editorValue;
            }
            return editorValue;
        }
        return "";
    },
    updateEditorConfiguration: function (elementId, newConfiguration) {
        const element = document.getElementById(elementId);
        const editor = monacoInterop.editorInstances[elementId];
        if (element && editor) {
            editor.updateOptions(JSON.parse(newConfiguration));
            monacoInterop.editorInstances[elementId] = editor;
        }
    },
    disposeEditor: function (elementId) {
        const element = document.getElementById(elementId);
        if (element && element.editor) {
            element.editor.dispose();
            element.editor = null;
        }
    },
    _loadMonacoScript: async function (src) {
        return new Promise((resolve, reject) => {
            if (document.querySelector(`script[src="${src}"]`)) {
                console.log(`Loader already loaded from: ${src}`);
                resolve();
                return;
            }

            const script = document.createElement("script");
            script.src = src;
            script.onload = () => {
                console.log("Successfully loaded loader.js, require should now be defined.");
                resolve(); // Now require should be available
            };
            script.onerror = (e) => {
                console.error(`Failed to load loader.js. URL attempted: ${src}`);
                reject(e);
            };
            document.head.appendChild(script);
        });
    }

};