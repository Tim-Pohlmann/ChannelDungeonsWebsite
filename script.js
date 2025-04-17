document.addEventListener('DOMContentLoaded', function() {
  // DOM Elements
  const messagesContainer = document.getElementById('messages');
  const commandInput = document.getElementById('commandInput');
  const sendButton = document.getElementById('send-button');
  const channels = document.querySelectorAll('.channel');
  const currentChannelDisplay = document.getElementById('current-channel');
  const commandInputElement = document.getElementById('command-input');
  
  // Current active channel
  let currentChannel = 'welcome';
  
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
  
  sendButton.addEventListener('click', function() {
    processCommand();
  });
  
  // Command processing function
  function processCommand() {
    const command = commandInputElement.value.trim();
    
    if (command === '') return;
    
    // Add user message to chat
    addUserMessage(command);
    
    // Process the command
    if (command.startsWith('/')) {
      const cleanCommand = command.substring(1).toLowerCase();
      
      switch (cleanCommand) {
        case 'help':
          addBotResponse("Available commands: <code>/about</code>, <code>/how-to-play</code>, <code>/features</code>, <code>/welcome</code>");
          break;
        case 'about':
        case 'how-to-play':
        case 'features':
        case 'welcome':
          switchChannel(cleanCommand);
          break;
        default:
          addBotResponse(`Unknown command: ${command}. Type <code>/help</code> for available commands.`);
      }
    } else {
      // Treat non-command messages as general chat
      addBotResponse(`This is a demonstration of a Discord-like interface. Try using commands like <code>/help</code> to navigate.`);
    }
    
    // Clear input field
    commandInputElement.value = '';
  }
  
  // Add a user message to the chat
  function addUserMessage(text) {
    const messageDiv = document.createElement('div');
    messageDiv.className = 'message';
    
    messageDiv.innerHTML = `
      <div class="message-avatar">U</div>
      <div class="message-content">
        <div class="message-header">
          <span class="message-username">User</span>
          <span class="message-timestamp">${getCurrentTime()}</span>
        </div>
        <div class="message-text">
          ${text}
        </div>
      </div>
    `;
    
    messagesContainer.appendChild(messageDiv);
    scrollToBottom();
  }
  
  // Add a bot response to the chat
  function addBotResponse(text) {
    const messageDiv = document.createElement('div');
    messageDiv.className = 'message';
    
    messageDiv.innerHTML = `
      <div class="message-avatar">CD</div>
      <div class="message-content">
        <div class="message-header">
          <span class="message-username">ChannelDungeonsBot</span>
          <span class="message-timestamp">${getCurrentTime()}</span>
        </div>
        <div class="message-text">
          ${text}
        </div>
      </div>
    `;
    
    messagesContainer.appendChild(messageDiv);
    scrollToBottom();
  }
  
  // Switch to a different channel
  function switchChannel(channelId) {
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
    
    // Add channel switch message
    addBotResponse(`You've switched to the <strong>#${channelId}</strong> channel.`);
  }
  
  // Load specific channel content
  function loadChannelContent(channelId) {
    const contentTemplateId = `${channelId}-content`;
    const contentTemplate = document.getElementById(contentTemplateId);
    
    if (contentTemplate) {
      // Clone template content and append to messages
      const content = contentTemplate.cloneNode(true);
      content.removeAttribute('id');
      
      // Add each message from the template one by one
      const messages = content.children;
      while (messages.length > 0) {
        messagesContainer.appendChild(messages[0]);
      }
    } else {
      addBotResponse(`Welcome to the #${channelId} channel. This channel has no content yet.`);
    }
    
    scrollToBottom();
  }
  
  // Get the current time in Discord format
  function getCurrentTime() {
    const now = new Date();
    return `Today at ${now.getHours()}:${now.getMinutes().toString().padStart(2, '0')}`;
  }
  
  // Scroll to the bottom of the messages
  function scrollToBottom() {
    const messageArea = document.querySelector('.message-area');
    messageArea.scrollTop = messageArea.scrollHeight;
  }
});
