

    // Paste JavaScript here
    document.addEventListener('DOMContentLoaded', function () {
        // DOM Elements
    const chatButton = document.getElementById('chat-button');
    const chatbotContainer = document.getElementById('chatbot-container');
    const closeButton = document.getElementById('close-button');
    const conversation = document.getElementById('conversation');
    const messageInput = document.getElementById('message-input');
    const sendButton = document.getElementById('send-button');

    // Toggle chatbot visibility
    chatButton.addEventListener('click', function () {
        chatbotContainer.classList.toggle('active');
    chatButton.classList.add('bounce');

            // Remove bounce animation after it plays
            setTimeout(() => {
        chatButton.classList.remove('bounce');
            }, 500);
        });

    // Close chatbot
    closeButton.addEventListener('click', function () {
        chatbotContainer.classList.remove('active');
    chatButton.classList.add('bounce');

            setTimeout(() => {
        chatButton.classList.remove('bounce');
            }, 500);
        });

    // Send message function
    async function sendMessage() {
            const content = messageInput.value.trim();

            // Don't send empty messages
            if (content === '') return;

            // Add user message to conversation
            addMessage(content, 'user');

            // Clear input field
            messageInput.value = '';

            // Show typing indicator
            showTypingIndicator();

            //comment this when in production , AI generated response XD
            // Get bot response after delay
            /*setTimeout(() => {
                // Remove typing indicator
                removeTypingIndicator();
                
                // Generate and add bot response
                const response = getBotResponse(content);
                addMessage(response, 'bot');
            }, 1000);*/

            //uncomment this when in production , AI generated response XD
            try {
                const res =  fetch('/api/HuggingFace/ask', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({ content })
                });

                const response = await res.json();
                removeTypingIndicator();
                addMessage(response.data || response.Data || "No response from AI", 'bot');
            } catch (error) {
                removeTypingIndicator();
                addMessage("Oops! Something went wrong. 😞", 'bot');
            }


            
        }

    // Send message on button click
    sendButton.addEventListener('click', sendMessage);

    // Send message on Enter key
    messageInput.addEventListener('keypress', function (e) {
            if (e.key === 'Enter') {
                sendMessage();
            }
        });

    // Add message to conversation
    function addMessage(text, sender) {
    const messageDiv = document.createElement('div');
    messageDiv.classList.add('message', sender + '-message');

    const messageContent = document.createElement('div');
    messageContent.classList.add('message-content');
    messageContent.textContent = text;

    messageDiv.appendChild(messageContent);
    conversation.appendChild(messageDiv);

    // Scroll to bottom
    conversation.scrollTop = conversation.scrollHeight;
        }

    // Show typing indicator
    function showTypingIndicator() {
        const typingDiv = document.createElement('div');
        typingDiv.classList.add('typing-indicator');
        typingDiv.id = 'typing-indicator';

        for (let i = 0; i < 3; i++) {
            const dot = document.createElement('span');
            typingDiv.appendChild(dot);
        }

        conversation.appendChild(typingDiv);
        conversation.scrollTop = conversation.scrollHeight;
    }

    // Remove typing indicator
    function removeTypingIndicator() {
            const typingIndicator = document.getElementById('typing-indicator');
    if (typingIndicator) {
        conversation.removeChild(typingIndicator);
            }
        }

    // Generate bot response
    function getBotResponse(input) {
            const responses = [
    "Hello, how can I help you today? 😊",
    "I'm sorry, I didn't understand your question. Could you please rephrase it? 😕",
    "I'm here to assist you with any questions or concerns you may have. 📩",
    "I'm sorry, I'm not able to browse the internet or access external information. Is there anything else I can help with? 💻",
    "What would you like to know? 🤔",
    "I'm here to assist you with any questions or problems you may have. How can I help you today? 🚀",
    "Is there anything specific you'd like to talk about? 💬",
    "I'm happy to help with any questions or concerns you may have. Just let me know how I can assist you. 😊",
    "I'm here to assist you with any questions or problems you may have. What can I help you with today? 🤗",
    "Is there anything specific you'd like to ask or talk about? I'm here to help with any questions or concerns you may have. 💬",
    "I'm here to assist you with any questions or problems you may have. How can I help you today? 💡",
    ];

    // Return a random response
    return responses[Math.floor(Math.random() * responses.length)];
        }

    // Tab switching detection
    document.addEventListener('visibilitychange', function () {
            if (document.hidden && chatbotContainer.classList.contains('active')) {
        // Add a message when user switches tabs
        setTimeout(() => {
            if (document.hidden) {
                addMessage("I noticed you switched tabs. I'll be here when you get back!", 'bot');
            }
        }, 2000);
            }
        });
    });