document.addEventListener('DOMContentLoaded', function() {
  // DOM Elements
  const messagesContainer = document.getElementById('messages');
  const sendButton = document.getElementById('send-button');
  const channels = document.querySelectorAll('.channel');
  const currentChannelDisplay = document.getElementById('current-channel');
  const commandInputElement = document.getElementById('command-input');
  const sidebar = document.querySelector('.sidebar');
  const contentArea = document.querySelector('.content-area');
  const typingIndicator = document.getElementById('typing-indicator');
  
  // Current active channel
  let currentChannel = 'welcome';
  
  // Cache to store loaded channel messages
  const channelMessagesCache = {};
  
  // Track which channels are currently loading
  const channelsLoading = {};
  
  // Array to track animation timeouts so they can be cancelled
  let activeTimeouts = [];
  
  // Flag to track if the sidebar has been shown
  let sidebarShown = false;
  
  // Typing indicator state
  let typingTimeoutId = null;
  
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
  
  // Show typing indicator for a specific duration
  function showTypingIndicator(duration = 1200) {
    // Clear any existing timeout
    if (typingTimeoutId) {
      clearTimeout(typingTimeoutId);
    }
    
    // Show typing indicator
    typingIndicator.classList.add('active');
    scrollToBottom();
    
    // Set timeout to hide typing indicator after duration
    typingTimeoutId = setTimeout(() => {
      hideTypingIndicator();
    }, duration);
    
    return typingTimeoutId;
  }
  
  // Hide typing indicator
  function hideTypingIndicator() {
    typingIndicator.classList.remove('active');
    if (typingTimeoutId) {
      clearTimeout(typingTimeoutId);
      typingTimeoutId = null;
    }
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
    
    // Show typing indicator before processing command
    showTypingIndicator();
    
    // Process the command with a delay to simulate typing
    setTimeout(() => {
      if (command.startsWith('/')) {
        handleSlashCommand(command.substring(1).toLowerCase());
      } else {
        // Treat non-command messages as general chat
        addBotResponse(`This is a demonstration of a Discord-like interface. Try using commands like <span class='discord-command'>help</span> to navigate.`);
      }
    }, 1000); // 1 second delay to simulate typing
    
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
    // Hide typing indicator if it's still showing
    hideTypingIndicator();
    
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
    
    // Also clear typing indicator timeout
    if (typingTimeoutId) {
      clearTimeout(typingTimeoutId);
      typingTimeoutId = null;
    }
    
    // Hide typing indicator
    hideTypingIndicator();
  }
  
  // Show the sidebar with animation
  function showSidebar() {
    if (sidebarShown) return; // Prevent showing sidebar multiple times
    
    sidebarShown = true;
    sidebar.classList.add('visible');
    contentArea.classList.add('sidebar-visible');
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
        // Display cached messages immediately without showing typing indicator
        displayCachedMessages(channelId);
      } else {
        // Show typing indicator only for new content that's being loaded for the first time
        showTypingIndicator(2000);
        
        // Load and cache channel content
        loadChannelContent(channelId);
      }
    } catch (error) {
      console.error('Error switching channels:', error);
    }
  }
  
  // Display cached messages for a channel
  function displayCachedMessages(channelId) {
    // Hide typing indicator
    hideTypingIndicator();
    
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
        
        // Show the typing indicator before each message
        const showTypingDuration = 1000; // 1 second of typing
        const delayBetweenMessages = index * (showTypingDuration + 200); // Total delay for this message
        
        // Set a timeout for showing the typing indicator
        const typingTimeoutId = setTimeout(() => {
          // Only show typing if we're still on the same channel
          if (currentChannel === channelId) {
            showTypingIndicator(showTypingDuration);
          }
        }, delayBetweenMessages);
        activeTimeouts.push(typingTimeoutId);
        
        // Set a timeout for the animation
        const messageTimeoutId = setTimeout(() => {
          // Only add the message if we're still on the same channel
          if (currentChannel === channelId) {
            hideTypingIndicator();
            messagesContainer.appendChild(messageClone);
            scrollToBottom();
            
            // If this is the last message of the welcome channel, show the sidebar
            if (channelId === 'welcome' && index === messages.length - 1) {
              // Add a small delay before showing the sidebar to ensure the message is visible
              setTimeout(() => {
                showSidebar();
              }, 500);
            }
          }
        }, delayBetweenMessages + showTypingDuration); // Add the message after the typing duration
        
        // Store the timeout ID so it can be cancelled if needed
        activeTimeouts.push(messageTimeoutId);
      });
    } else {
      // Show typing indicator for a short time before displaying the default message
      showTypingIndicator(1000);
      
      setTimeout(() => {
        const botResponse = `Welcome to the #${channelId} channel. This channel has no content yet.`;
        addBotResponse(botResponse);
        
        // Cache the message immediately
        channelMessagesCache[channelId] = messagesContainer.innerHTML;
        
        // If this is the welcome channel (which shouldn't happen with the current code,
        // but just in case the template is removed), show the sidebar
        if (channelId === 'welcome') {
          setTimeout(() => {
            showSidebar();
          }, 500);
        }
      }, 1000);
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
