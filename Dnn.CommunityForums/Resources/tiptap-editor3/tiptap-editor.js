// Copyright (c) by DNN Community
//
// DNN Community licenses this file to you under the MIT license.
//
// See the LICENSE file in the project root for more information.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions
// of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

import { Editor } from '@tiptap/core'
import { Focus } from '@tiptap/extensions'
import StarterKit from '@tiptap/starter-kit'
import Image from '@tiptap/extension-image'
import FileHandler from '@tiptap/extension-file-handler'
import DragHandle from '@tiptap/extension-drag-handle'
import Mention from '@tiptap/extension-mention'

export default class TipTapEditorController {
    constructor(editorID, initialText, tabId, moduleId, forumId) {
        this.createEditor(editorID, initialText, tabId, moduleId, forumId)
    };    

    createEditor(editorID, initialText, tabId, moduleId, forumId) {
        this.editorElement = document.querySelector('#' + editorID)
        this.actions = [
            { name: "bold", title: amaf.resx.BoldDesc, fontTag: "fa-bold", command: 'toggleBold', disable: true },
            { name: "italic", title: amaf.resx.ItalicDesc, fontTag: "fa-italic", command: 'toggleItalic', disable: true },
            { name: "underline", title: amaf.resx.UnderlineDesc, fontTag: "fa-underline", command: 'toggleUnderline', disable: true },
            { name: "strike", title: amaf.resx.StrikeDesc, fontTag: "fa-strikethrough", command: 'toggleStrike', disable: true },
            { name: "undo", title: amaf.resx.UndoDesc, fontTag: "fa-undo", command: 'undo', disable: true },
            { name: "redo", title: amaf.resx.RedoDesc, fontTag: "fa-repeat", command: 'redo', disable: true },
            { name: "code", title: amaf.resx.CodeDesc, fontTag: "fa-code", command: 'toggleCode', disable: true },
            { name: "orderedList", title: amaf.resx.OrderedListDesc, fontTag: "fa-list-ol", command: 'toggleOrderedList' },
            { name: "bulletList", title: amaf.resx.BulletedListDesc, fontTag: "fa-list-ul", command: 'toggleBulletList' },
            { name: "blockquote", title: amaf.resx.QuoteDesc, fontTag: "fa-quote-left", command: 'toggleBlockquote' },
            { 
                name: "showBoxes",
                title: amaf.resx.ShowBoxesDesc || "Show boxes",
                fontTag: "fa-object-group",
                command: "toggleShowBoxes",
                isActive: () => {
                    const target = this.editor?.view?.dom || this.editorElement;
                    return target.classList.contains("dcf-tiptap-show-boxes");
                },
            },
        ];
        for (let i in this.actions) {
            let a = this.actions[i];
            a.button = document.createElement("button");
            a.button.setAttribute("title", a.title || a.name);
            a.button.innerHTML = "<i class=\"fa " + a.fontTag + "\"></i>"
            a.button.addEventListener("click", (event) => {
                event.preventDefault();
                this.executeAction(a);
            });
            this.editorElement.appendChild(a.button);
        };

        // Extend the Mention node so it explicitly declares the attributes we will store.
        // This ensures they are preserved in the document JSON and available to renderHTML.
        const CustomMention = Mention.extend({
            addAttributes() {
                return {
                    id: { default: null },
                    // store a plain serializable object for label that contains url and imgSrc
                    label: { default: { label: null, url: null, imgSrc: null, imgClass: null, mentionClass: null, } },
                };
            },
            // Provide a safe renderHTML so missing fields don't throw.
            renderHTML({ node, options }) {
                const labelObj = node.attrs.label || {};
                const char = (labelObj.suggestionChar || '');
                const imgSrc = (labelObj.imgSrc || '').replace(/&amp;/g, '&');
                const imgClass = labelObj.imgClass || 'af-avatar';
                const mentionClass = labelObj.mentionClass || 'dcf-mention';

                const children = [];

                if (imgSrc) {
                    children.push(['img', { class: imgClass, src: imgSrc, alt: labelObj.label ?? node.attrs.id }]);
                }

                children.push(` ${char}${labelObj.label ?? node.attrs.id}`);

                return [
                    'a',
                    { href: labelObj.url || '#', class: mentionClass },
                    ...children
                ];
            },
            renderText({ node, options }) {
                const char = (options && options.suggestion && options.suggestion.char) ? options.suggestion.char : '@';
                return `${char}${node.attrs.label?.label ?? node.attrs.id}`;
            },
        });

        this.editorElement.style.position = this.editorElement.style.position || 'relative';

        const positionSuggestionPopup = (props, popup) => {
            const caretRect = props?.clientRect?.();
            if (!caretRect || !popup) {
                return;
            }

            const hostRect = this.editorElement.getBoundingClientRect();

            popup.style.left = `${caretRect.left - hostRect.left + this.editorElement.scrollLeft}px`;
            popup.style.top = `${caretRect.bottom - hostRect.top + this.editorElement.scrollTop + 6}px`;
        };

        this.editor = new Editor({
            element: this.editorElement,
            autofocus: true,
            content: `${initialText}`,
            extensions: [
                StarterKit,
                Image.configure({
                   allowBase64: true,
                   inline: true,
                   resize: {
                    enabled: true,
                    directions: ['top', 'bottom', 'left', 'right'], // can be any direction or diagonal combination
                    minWidth: 50,
                    minHeight: 50,
                    alwaysPreserveAspectRatio: true,
                    }
                }),
                Focus.configure({
                    className: 'dcf-tiptap-has-focus',
                    mode: 'all',
                }),
                DragHandle.configure({
                      render: () => {
                        const element = document.createElement('div')
                        element.classList.add('dcf-tiptap-drag-handle')

                        return element
                      },
                    }),
                FileHandler.configure({
                    allowedMimeTypes: ['image/png', 'image/jpeg', 'image/gif'],
                    onDrop: (currentEditor, files, pos) => {
                        files.forEach(file => {
                            const fileReader = new FileReader()
                            fileReader.readAsDataURL(file)
                            fileReader.onload = () => {
                                currentEditor
                                  .chain()
                                  .insertContentAt(pos, {
                                    type: 'image',
                                    attrs: {
                                      src: fileReader.result,
                                    },
                                  })
                                  .focus()
                                  .run()
                              }
                            })
                          },
                    onPaste: (currentEditor, files) => {
                    files.forEach(file => {
                        const fileReader = new FileReader()

                        fileReader.readAsDataURL(file)
                        fileReader.onload = () => {
                        currentEditor
                            .chain()
                            .insertContentAt(currentEditor.state.selection.anchor, {
                            type: 'image',
                            attrs: {
                                src: fileReader.result,
                            },
                            })
                            .focus()
                            .run()
                        }
                        })
                    },
                }),
                // Use the extended Mention node and configure the suggestion UI as before.
                CustomMention.configure({
                    HTMLAttributes: {
                      class: 'dcf-mention'},
                    suggestions: [
                        {
                            char: '@',
                            items: async (query) => {
                                const queryString = (query?.query || query || '').toString().toLowerCase();
                                // Return empty array if query is empty or too short
                                if (typeof(queryString) != 'string' || queryString?.length < 3 || queryString === "[object object]") {
                                    return [];
                                }
                                const siteRoot = dnn.getVar('sf_siteRoot', '/');
                                const url = `${siteRoot}API/ActiveForums/User/GetUsersForEditorMentions?forumId=${forumId}&query=${encodeURIComponent(queryString)}`;
    
                                try {
                                    const response = await fetch(url, {
                                        method: 'GET',
                                        headers: {
                                            'RequestVerificationToken': $('[name="__RequestVerificationToken"]').val(),
                                            'ModuleId': moduleId,
                                            'TabId': tabId,
                                        },
                                    });
        
                                    if (!response.ok) {
                                        console.error(`API error: ${response.status}`);
                                        return [];
                                    }       
                                
                                    // Read response once and check if it's empty
                                    const responseText = await response.text();
                                    console.log('Response text:', responseText);
                                
                                    if (!responseText || responseText.trim() === '') {
                                        console.warn('Empty response body for query:', queryString);
                                        return [];
                                    }

                                    // Parse the JSON
                                    try {
                                        const data = JSON.parse(responseText);
                                        console.log('Parsed data:', data);
                                    
                                        if (!Array.isArray(data)) {
                                            console.warn('Expected array response, got:', typeof data, data);
                                            return [];
                                        }

                                        return data;
                                    } catch (parseError) {
                                        console.error('Failed to parse JSON:', parseError, 'Response was:', responseText);
                                        return [];
                                    }
                                } catch (error) {
                                    console.error('Failed to fetch users for mentions:', error);
                                    return [];
                                }

                            },
                            render: () => {
                                let component;
                                let popup;
                                // keep a reference to the latest props so we can access range and command reliably
                                let suggestionProps;
                                return {
                                  onStart: (props) => {
                                    suggestionProps = props;
                                    popup = document.createElement('div');
                                    popup.className = 'dcf-mention-popup';
                                    this.editorElement.appendChild(popup);
                                    positionSuggestionPopup(props, popup);
                                    component = {
                                      updateItems: (items) => {                                          
                                        popup.hidden = !(Array.isArray(items) && items.length > 0);
                                        popup.innerHTML = `<ul class="items">
                                        ${items.map(item => `<li data-id="${item.id}" data-name="${item.name}" data-url="${item.userProfileUrl}" data-imgtag="${item.avatarImgTag}" data-imgsrc="${item.avatarImgSrc}" class="dcf-mentions-user item ${item.selected ? 'is-selected' : ''}">${item.avatarImgTag} ${item.name}</li>`).join('')}
                                        </ul>`;
                                        },
                                      destroy: () => popup.remove(),  
                                    };  
                                    popup.addEventListener('click', (e) => {
                                      const li = e.target.closest('li');
                                      if (li && suggestionProps) {
                                          const id = li.dataset.id;
                                          const name = li.dataset.name;
                                          const labelObj = {
                                              label: li.dataset.name,
                                              url: li.dataset.url,
                                              imgSrc: li.dataset.imgsrc,
                                              imgClass: 'af-avatar',
                                              mentionClass: 'dcf-mention-user',
                                              suggestionChar: '@',
                                          };

                                          // Prefer using the suggestion's command if it is a function that accepts an object.
                                          // Some Suggestion implementations accept an object ({ id, label }) while others expect a string id.
                                          let commandUsed = false;
                                          if (typeof suggestionProps.command === 'function') {
                                              try {
                                                  suggestionProps.command({ id: id, label: labelObj });
                                                  commandUsed = true;
                                              } catch (err) {
                                                  // ignore - we'll fall back to manual insertion below
                                              }
                                          }

                                          // Fallback: always ensure the mention node is inserted using the editor API so we can pass
                                          // complex attributes (url, imgSrc) reliably. Use the suggestion range (atomic replacement).
                                          const range = suggestionProps.range;
                                          if (!commandUsed && range && this.editor) {
                                              this.editor
                                                  .chain()
                                                  .focus()
                                                  .insertContentAt(range, {
                                                      type: 'mention',
                                                      attrs: { id: id, label: labelObj }
                                                  })
                                                  .run();
                                          }

                                          // Remove any leftover decoration elements from the DOM after the transaction completes.
                                          requestAnimationFrame(() => {
                                              const suggestionNodes = this.editorElement.querySelectorAll('.suggestion');
                                              suggestionNodes.forEach(n => n && n.parentNode && n.parentNode.removeChild(n));
                                          });
                                      }
                                    });
                                    component?.updateItems(props.items);
                                  },
                                onUpdate(props) {
                                    // keep suggestionProps current so click handler always has the latest range
                                    suggestionProps = props;
                                    component?.updateItems(props.items);
                                    positionSuggestionPopup(props, popup);
                                    },
                                onExit: () => component?.destroy(),
                                };
                            },
                        },
                        {
                            char: '#',
                            items: async (query) => {
                                const queryString = (query?.query || query || '').toString().toLowerCase();
                                // Return empty array if query is empty or too short
                                if (typeof (queryString) != 'string' || queryString?.length < 0 || queryString === "[object object]") {
                                    return [];
                                }
                                const siteRoot = dnn.getVar('sf_siteRoot', '/');
                                const url = `${siteRoot}API/ActiveForums/Tag/Matches?forumId=${forumId}&query=${encodeURIComponent(queryString)}`;
    
                                try {
                                    const response = await fetch(url, {
                                        method: 'GET',
                                        headers: {
                                            'RequestVerificationToken': $('[name="__RequestVerificationToken"]').val(),
                                            'ModuleId': moduleId,
                                            'TabId': tabId,
                                        },
                                    });
        
                                    if (!response.ok) {
                                        console.error(`API error: ${response.status}`);
                                        return [];
                                    }       
                                
                                    // Read response once and check if it's empty
                                    const responseText = await response.text();
                                    console.log('Response text:', responseText);
                                
                                    if (!responseText || responseText.trim() === '') {
                                        console.warn('Empty response body for query:', queryString);
                                        return [];
                                    }

                                    // Parse the JSON
                                    try {
                                        const data = JSON.parse(responseText);
                                        console.log('Parsed data:', data);
                                    
                                        if (!Array.isArray(data)) {
                                            console.warn('Expected array response, got:', typeof data, data);
                                            return [];
                                        }

                                        return data;
                                    } catch (parseError) {
                                        console.error('Failed to parse JSON:', parseError, 'Response was:', responseText);
                                        return [];
                                    }
                                } catch (error) {
                                    console.error('Failed to fetch tags:', error);
                                    return [];
                                }

                            },
                            render: () => {
                                let component;
                                let popup;
                                // keep a reference to the latest props so we can access range and command reliably
                                let suggestionProps;
                                return {
                                  onStart: (props) => {
                                    suggestionProps = props;
                                    popup = document.createElement('div');
                                    popup.className = 'dcf-mention-popup';
                                    this.editorElement.appendChild(popup);
                                    positionSuggestionPopup(props, popup);
                                    component = {
                                      updateItems: (items) => {      
                                        popup.hidden = !(Array.isArray(items) && items.length > 0);
                                        popup.innerHTML = `<ul class="items">
                                        ${items.map(item => `<li data-id="${item.id}" data-name="${item.name}" class="dcf-mentions-tag item ${item.selected ? 'is-selected' : ''}">${item.name}</li>`).join('')}
                                        </ul>`;
                                        },
                                      destroy: () => popup.remove(),  
                                    };  
                                    popup.addEventListener('click', (e) => {
                                      const li = e.target.closest('li');
                                      if (li && suggestionProps) {
                                          const id = li.dataset.id;
                                          const name = li.dataset.name;
                                          const labelObj = {
                                              label: li.dataset.name,
                                              url: li.dataset.url,
                                              imgSrc: null,
                                              imgClass: null,
                                              mentionClass: 'dcf-tag-link',
                                              suggestionChar: '#',
                                          };

                                          // Prefer using the suggestion's command if it is a function that accepts an object.
                                          // Some Suggestion implementations accept an object ({ id, label }) while others expect a string id.
                                          let commandUsed = false;
                                          if (typeof suggestionProps.command === 'function') {
                                              try {
                                                  suggestionProps.command({ id: id, label: labelObj });
                                                  commandUsed = true;
                                              } catch (err) {
                                                  // ignore - we'll fall back to manual insertion below
                                              }
                                          }

                                          // Fallback: always ensure the mention node is inserted using the editor API so we can pass
                                          // complex attributes (url, imgSrc) reliably. Use the suggestion range (atomic replacement).
                                          const range = suggestionProps.range;
                                          if (!commandUsed && range && this.editor) {
                                              this.editor
                                                  .chain()
                                                  .focus()
                                                  .insertContentAt(range, {
                                                      type: 'mention',
                                                      attrs: { id: id, label: labelObj }
                                                  })
                                                  .run();
                                          }

                                          // Remove any leftover decoration elements from the DOM after the transaction completes.
                                          requestAnimationFrame(() => {
                                              const suggestionNodes = this.editorElement.querySelectorAll('.suggestion');
                                              suggestionNodes.forEach(n => n && n.parentNode && n.parentNode.removeChild(n));
                                          });
                                      }
                                    });
                                    component?.updateItems(props.items);
                                  },
                                onUpdate(props) {
                                    // keep suggestionProps current so click handler always has the latest range
                                    suggestionProps = props;
                                    component?.updateItems(props.items);
                                    positionSuggestionPopup(props, popup);
                                    },
                                onExit: () => component?.destroy(),
                                };
                            },
                        },
                    ],
                }),
            ],
            onTransaction: () => {
                this.checkActive()
            }
        })
    };
    
    getHTML() {
        return this.editor.getHTML();
    }
    getText() {
        return this.editor.getText();
    }
    checkActive() {
        for (let i in this.actions) {
            let a = this.actions[i];

            if (a.disable) {
                if (this.editor.can().chain().focus()[a.command]().run()) {
                    a.button.removeAttribute("disabled");
                } else {
                    a.button.setAttribute("disabled", "true");
                }
            }

            if (typeof a.isActive === "function") {
                if (a.isActive()) {
                    a.button.classList.add("is-active");
                } else {
                    a.button.classList.remove("is-active");
                }

                continue;
            }

            if (a.button.classList.contains("is-active")) {
                if (!this.editor.isActive(a.name)) {
                    a.button.classList.remove("is-active");
                }
            } else if (this.editor.isActive(a.name)) {
                a.button.classList.add("is-active");
            }
        }
    };
    
    executeAction(action) {
        if (!action) {
            return;
        }

        if (action.command === "toggleShowBoxes") {
            const target = this.editor?.view?.dom || this.editorElement;
            target.classList.toggle("dcf-tiptap-show-boxes");
            this.checkActive();
            return;
        }

        this.editor.chain().focus()[action.command]().run();
    }
 }