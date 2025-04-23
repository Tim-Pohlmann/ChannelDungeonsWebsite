document.addEventListener('DOMContentLoaded', function() {
  // DOM Elements
  const messagesContainer = document.getElementById('messages');
  const sendButton = document.getElementById('send-button');
  const channels = document.querySelectorAll('.channel');
  const currentChannelDisplay = document.getElementById('current-channel');
  const commandInputElement = document.getElementById('command-input');
  
  // Current active channel
  let currentChannel = 'welcome';
  
  // Utility function to create message HTML
  function createMessageHTML(options = {}) {
    const {
      avatar = '',
      username = 'ChannelDungeonsBot',
      timestamp = getCurrentTime(),
      content = '',
      isSystem = false
    } = options;
    
    if (isSystem) {
      return `
        <div class="message system-message">
          <div class="message-content">
            ${content}
          </div>
        </div>
      `;
    }
    
    return `
      <div class="message">
        <div class="message-avatar" aria-hidden="true">${avatar}</div>
        <div class="message-content">
          <div class="message-header">
            <span class="message-username">${username}</span>
            <span class="message-timestamp">${timestamp}</span>
          </div>
          <div class="message-text">
            ${content}
          </div>
        </div>
      </div>
    `;
  }
  
  // Initialize the welcome channel content
  loadChannelContent(currentChannel);
  
  // Handle channel switching by clicking on channel names
  channels.forEach(channel => {
    channel.addEventListener('click', function() {
      const channelId = this.getAttribute('data-channel');
      switchChannel(channelId);
    });
  });
  
  // Command input handling
  commandInputElement.addEventListener('keypress', function(e) {
    if (e.key === 'Enter') {
      processCommand();
    }
  });
  
  sendButton.addEventListener('click', processCommand);
  
  // Command processing function
  function processCommand() {
    const command = commandInputElement.value.trim();
    
    if (command === '') return;
    
    // Add user message to chat
    addUserMessage(command);
    
    // Process the command
    if (command.startsWith('/')) {
      handleSlashCommand(command.substring(1).toLowerCase());
    } else {
      // Treat non-command messages as general chat
      addBotResponse(`This is a demonstration of a Discord-like interface. Try using commands like <span class='discord-command'>help</span> to navigate.`);
    }
    
    // Clear input field
    commandInputElement.value = '';
    
    // Focus back on input for better UX
    commandInputElement.focus();
  }
  
  // Handle slash commands
  function handleSlashCommand(cleanCommand) {
    const availableCommands = ['about', 'how-to-play', 'features', 'welcome', 'live-discord'];
    
    if (cleanCommand === 'help') {
      addBotResponse(`Available commands: ${availableCommands.map(cmd => `<span class='discord-command'>${cmd}</span>`).join(', ')}`);
    } else if (availableCommands.includes(cleanCommand)) {
      switchChannel(cleanCommand);
    } else {
      addBotResponse(`Unknown command: /${cleanCommand}. Type <span class='discord-command'>help</span> for available commands.`);
    }
  }
  
  // Add a user message to the chat
  function addUserMessage(text) {
    const messageHTML = createMessageHTML({
      avatar: 'U',
      username: 'User',
      content: text
    });
    
    messagesContainer.insertAdjacentHTML('beforeend', messageHTML);
    scrollToBottom();
  }
  
  // Add a bot response to the chat
  function addBotResponse(text) {
    const messageHTML = createMessageHTML({
      content: text
    });
    
    messagesContainer.insertAdjacentHTML('beforeend', messageHTML);
    scrollToBottom();
  }
  
  // Switch to a different channel
  function switchChannel(channelId) {
    try {
      // Update active channel in sidebar
      channels.forEach(channel => {
        if (channel.getAttribute('data-channel') === channelId) {
          channel.classList.add('active');
        } else {
          channel.classList.remove('active');
        }
      });
      
      // Update current channel display
      currentChannelDisplay.textContent = channelId;
      currentChannel = channelId;
      
      // Clear current messages
      messagesContainer.innerHTML = '';
      
      // Load channel content
      loadChannelContent(channelId);
    } catch (error) {
      console.error('Error switching channels:', error);
    }
  }
  
  // Load specific channel content
  function loadChannelContent(channelId) {
    const contentTemplateId = `${channelId}-content`;
    const contentTemplate = document.getElementById(contentTemplateId);
    
    if (contentTemplate) {
      // Get template content
      const content = document.importNode(contentTemplate.content, true);
      
      // Process simple-message elements if any exist
      const simpleMessages = content.querySelectorAll('.simple-message');
      simpleMessages.forEach(simpleMsg => {
        const messageOptions = {
          username: simpleMsg.getAttribute('data-username') || 'Channel Dungeons',
          timestamp: getCurrentTime(), // Always use current time
          content: simpleMsg.innerHTML,
          isSystem: simpleMsg.hasAttribute('data-system')
        };
        
        const messageHTML = createMessageHTML(messageOptions);
        simpleMsg.outerHTML = messageHTML;
      });
      
      // For older message format, update timestamps to current time
      const oldFormatMessages = content.querySelectorAll('.message .message-timestamp');
      oldFormatMessages.forEach(timestampElement => {
        timestampElement.textContent = getCurrentTime();
      });
      
      // Add each message with a delay for animation effect
      const messages = Array.from(content.children);
      messages.forEach((message, index) => {
        setTimeout(() => {
          messagesContainer.appendChild(message);
          scrollToBottom();
        }, index * 1000); // 1000ms (1 second) delay between messages
      });
    } else {
      addBotResponse(`Welcome to the #${channelId} channel. This channel has no content yet.`);
    }
  }
  
  // Get the current time in local format
  function getCurrentTime() {
    const now = new Date();
    const timeStr = now.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', hour12: true });
    // Convert am/pm to uppercase
    return timeStr.replace(/am|pm/i, match => match.toUpperCase());
  }
  
  // Scroll to the bottom of the messages
  function scrollToBottom() {
    const messageArea = document.querySelector('.message-area');
    messageArea.scrollTop = messageArea.scrollHeight;
  }
});
