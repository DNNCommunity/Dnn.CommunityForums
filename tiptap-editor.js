                    suggestion: {
                        items: async (query) => {
                            const queryString = (query?.query || query || '').toString().toLowerCase();
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
                                
                                return await response.json();
                            } catch (error) {
                                console.error('Failed to fetch users for mentions:', error);
                                return [];
                            }
                        },