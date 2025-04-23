document.addEventListener('DOMContentLoaded', function() {
  // DOM Elements
  const messagesContainer = document.getElementById('messages');
  const sendButton = document.getElementById('send-button');
  const channels = document.querySelectorAll('.channel');
  const currentChannelDisplay = document.getElementById('current-channel');
  const commandInputElement = document.getElementById('command-input');
  
  // Current active channel
  let currentChannel = 'welcome';
  
  // Cache to store loaded channel messages
  const channelMessagesCache = {};
  
  // Track which channels are currently loading
  const channelsLoading = {};
  
  // Array to track animation timeouts so they can be cancelled
  let activeTimeouts = [];
  
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
    
    // Update cache for the current channel
    channelMessagesCache[currentChannel] = messagesContainer.innerHTML;
  }
  
  // Add a bot response to the chat
  function addBotResponse(text) {
    const messageHTML = createMessageHTML({
      content: text
    });
    
    messagesContainer.insertAdjacentHTML('beforeend', messageHTML);
    scrollToBottom();
    
    // Update cache for the current channel
    channelMessagesCache[currentChannel] = messagesContainer.innerHTML;
  }
  
  // Clear all pending timeouts
  function clearAllTimeouts() {
    activeTimeouts.forEach(timeoutId => clearTimeout(timeoutId));
    activeTimeouts = [];
  }
  
  // Switch to a different channel
  function switchChannel(channelId) {
    try {
      // Cancel any pending animation timeouts
      clearAllTimeouts();
      
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
      
      // Store previous channel for reference
      const previousChannel = currentChannel;
      
      // Update current channel
      currentChannel = channelId;
      
      // Clear current messages
      messagesContainer.innerHTML = '';
      
      // Check if channel content is already cached
      if (channelMessagesCache[channelId]) {
        // Display cached messages
        displayCachedMessages(channelId);
      } else {
        // Load and cache channel content
        loadChannelContent(channelId);
      }
    } catch (error) {
      console.error('Error switching channels:', error);
    }
  }
  
  // Display cached messages for a channel
  function displayCachedMessages(channelId) {
    const cachedContent = channelMessagesCache[channelId];
    
    // Append all cached messages to the container at once
    messagesContainer.innerHTML = cachedContent;
    
    // Scroll to bottom
    scrollToBottom();
  }
  
  // Load specific channel content
  function loadChannelContent(channelId) {
    // Mark this channel as currently loading
    channelsLoading[channelId] = true;
    
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
      
      // Create a temporary container for the complete content
      const tempContainer = document.createElement('div');
      const messages = Array.from(content.children);
      
      // Add all messages to the temp container immediately
      messages.forEach(message => {
        const messageClone = message.cloneNode(true);
        tempContainer.appendChild(messageClone);
      });
      
      // Cache the complete content immediately
      channelMessagesCache[channelId] = tempContainer.innerHTML;
      
      // Now animate adding the messages one by one for the current view only
      messages.forEach((message, index) => {
        // Create a clone of the message to avoid issues with content being moved
        const messageClone = message.cloneNode(true);
        
        // Set a timeout for the animation
        const timeoutId = setTimeout(() => {
          // Only add the message if we're still on the same channel
          if (currentChannel === channelId) {
            messagesContainer.appendChild(messageClone);
            scrollToBottom();
          }
        }, index * 1000); // 1000ms (1 second) delay between messages
        
        // Store the timeout ID so it can be cancelled if needed
        activeTimeouts.push(timeoutId);
      });
    } else {
      const botResponse = `Welcome to the #${channelId} channel. This channel has no content yet.`;
      addBotResponse(botResponse);
      
      // Cache the message immediately
      channelMessagesCache[channelId] = messagesContainer.innerHTML;
    }
    
    // Mark this channel as no longer loading
    channelsLoading[channelId] = false;
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
