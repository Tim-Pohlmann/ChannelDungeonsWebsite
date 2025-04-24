document.addEventListener('DOMContentLoaded', function() {
  // DOM Elements
  const messagesContainer = document.getElementById('messages');
  const channels = document.querySelectorAll('.channel');
  const currentChannelDisplay = document.getElementById('current-channel');
  const commandInputElement = document.getElementById('command-input');
  const commandInputContainer = document.querySelector('.command-input-container');
  const sidebar = document.querySelector('.sidebar');
  const contentArea = document.querySelector('.content-area');
  const typingIndicator = document.getElementById('typing-indicator');
  const autocompleteDropdown = document.getElementById('autocomplete-dropdown');
  
  // Available commands with descriptions
  const availableCommands = [
    { name: 'about', description: 'Learn about Channel Dungeons' },
    { name: 'how-to-play', description: 'Get started with the game' },
    { name: 'features', description: 'Discover game features' },
    { name: 'welcome', description: 'Return to welcome channel' },
    { name: 'live-discord', description: 'See the live Discord experience' }
  ];
  
  // Autocomplete state
  let selectedAutocompleteIndex = -1;
  
  // Current active channel
  let currentChannel = 'welcome';
  
  // Cache to store loaded channel messages
  const channelMessagesCache = {};
  
  // Array to track animation timeouts so they can be cancelled
  let activeTimeouts = [];
  
  // Flag to track if the sidebar has been shown
  let sidebarShown = false;
  
  // Flag to track if the command input has been shown
  let inputShown = false;
  
  // Typing indicator state
  let typingTimeoutId = null;
  
  // Flag to track if this is initial page load
  let isInitialPageLoad = true;
  
  // Flag to track if the URL has a specific channel on load
  const hasChannelInUrl = window.location.hash.length > 1;
  
  // Flag to track current view mode (mobile or desktop)
  let isMobileView = window.innerWidth <= 768;

  // Function to show the sidebar with animation
  function showSidebar() {
    if (sidebarShown) return; // Prevent showing sidebar multiple times
    
    if (window.innerWidth <= 768) {
      // Don't automatically show sidebar on mobile, wait for swipe or toggle button click
    } else {
      sidebarShown = true;
      // Add visible class for desktop view
      sidebar.classList.add('visible');
    }
  }

  // Show the command input with animation
  function showCommandInput() {
    if (inputShown) return; // Prevent showing input multiple times
    
    inputShown = true;
    commandInputContainer.classList.add('visible');
    
    // Set initial focus to command input
    setTimeout(() => {
      commandInputElement.focus();
    }, 300); // Small delay to ensure animation completes
  }
  
  // Check if URL has a hash to determine initial channel
  function getChannelFromHash() {
    const hash = window.location.hash.substring(1); // Remove the # symbol
    // Check if this hash corresponds to a valid channel
    const validChannels = Array.from(channels).map(channel => 
      channel.getAttribute('data-channel')
    );
    
    return validChannels.includes(hash) ? hash : 'welcome';
  }
  
  // Initialize with channel from URL or default to welcome
  currentChannel = getChannelFromHash();
  
  // Update the channel name in the header to match the initial channel
  currentChannelDisplay.textContent = currentChannel;
  
  // Utility function to create message HTML
  function createMessageHTML(options = {}) {
    const {
      avatar = '',
      username = 'Channel Dungeons',
      timestamp = getCurrentTime(),
      content = '',
    } = options;
    
    return `
      <div class="message">
        <div class="message-avatar ${avatar !== '' ? 'no-background-image' : ''}" aria-hidden="true">${avatar}</div>
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
  
  // Initialize the channel content based on URL hash or default
  loadChannelContent(currentChannel);
  
  // Update active channel in sidebar based on current channel
  updateActiveChannelInSidebar(currentChannel);
  
  // For initial direct access via a channel-specific URL, show UI elements immediately
  if (hasChannelInUrl) {
    showSidebar();
    showCommandInput();
  }
  
  // Handle channel switching by clicking on channel names
  channels.forEach(channel => {
    channel.addEventListener('click', function() {
      const channelId = this.getAttribute('data-channel');
      switchChannel(channelId);
    });
  });
  
  // Listen for hash changes to handle browser back/forward navigation
  window.addEventListener('hashchange', function() {
    const channelFromHash = getChannelFromHash();
    // Only switch if it's different from current to avoid loops
    if (channelFromHash !== currentChannel) {
      switchChannel(channelFromHash, false); // Don't update URL again
    }
  });
  
  // Command input handling
  commandInputElement.addEventListener('keypress', function(e) {
    if (e.key === 'Enter') {
      processCommand();
    }
  });
  
  // Add input event listener for autocomplete
  commandInputElement.addEventListener('input', function() {
    showAutocomplete(commandInputElement.value);
  });
  
  // Add keydown event listener for keyboard navigation in autocomplete
  commandInputElement.addEventListener('keydown', function(e) {
    const items = autocompleteDropdown.querySelectorAll('.autocomplete-item');
    
    // Only handle navigation keys when dropdown is visible
    if (!autocompleteDropdown.classList.contains('visible')) {
      return;
    }
    
    switch(e.key) {
      case 'ArrowDown':
        e.preventDefault(); // Prevent cursor movement in input
        selectedAutocompleteIndex = Math.min(selectedAutocompleteIndex + 1, items.length - 1);
        updateAutocompleteSelection();
        break;
        
      case 'ArrowUp':
        e.preventDefault(); // Prevent cursor movement in input
        selectedAutocompleteIndex = Math.max(selectedAutocompleteIndex - 1, 0);
        updateAutocompleteSelection();
        break;
        
      case 'Tab':
      case 'Enter':
        // Only process if dropdown is visible and an item is selected
        if (selectedAutocompleteIndex >= 0 && selectedAutocompleteIndex < items.length) {
          e.preventDefault(); // Prevent form submission
          const commandName = items[selectedAutocompleteIndex].querySelector('.command-name').textContent.substring(1);
          selectCommand(commandName);
        }
        break;
        
      case 'Escape':
        hideAutocomplete();
        break;
    }
  });
  
  // Function to show autocomplete dropdown
  function showAutocomplete(inputValue) {
    // Only show autocomplete for / commands
    if (!inputValue.startsWith('/')) {
      hideAutocomplete();
      return;
    }
    
    // Get search term without the /
    const searchTerm = inputValue.substring(1).toLowerCase();
    
    // Filter commands that match the search term
    const matchingCommands = availableCommands.filter(cmd => 
      cmd.name.toLowerCase().startsWith(searchTerm)
    );
    
    // Hide dropdown if no matches or empty search
    if (matchingCommands.length === 0) {
      hideAutocomplete();
      return;
    }
    
    // Clear previous autocomplete items
    autocompleteDropdown.innerHTML = '';
    
    // Create and add autocomplete items
    matchingCommands.forEach((cmd, index) => {
      const item = document.createElement('div');
      item.className = 'autocomplete-item';
      item.innerHTML = `<span class="command-name">/${cmd.name}</span><span class="command-description">${cmd.description}</span>`;
      
      // Add click handler to select this command
      item.addEventListener('click', () => {
        selectCommand(cmd.name);
      });
      
      autocompleteDropdown.appendChild(item);
    });
    
    // Show autocomplete dropdown
    autocompleteDropdown.classList.add('visible');
    
    // Reset selection
    selectedAutocompleteIndex = 0;
    updateAutocompleteSelection();
  }
  
  // Function to hide autocomplete dropdown
  function hideAutocomplete() {
    autocompleteDropdown.classList.remove('visible');
    selectedAutocompleteIndex = -1;
  }
  
  // Function to update the selected item in the autocomplete dropdown
  function updateAutocompleteSelection() {
    const items = autocompleteDropdown.querySelectorAll('.autocomplete-item');
    
    // Remove selected class from all items
    items.forEach(item => item.classList.remove('selected'));
    
    // Add selected class to the current selection if valid
    if (selectedAutocompleteIndex >= 0 && selectedAutocompleteIndex < items.length) {
      items[selectedAutocompleteIndex].classList.add('selected');
      items[selectedAutocompleteIndex].scrollIntoView({ block: 'nearest' });
    }
  }
  
  // Function to select a command from autocomplete
  function selectCommand(commandName) {
    commandInputElement.value = '/' + commandName;
    hideAutocomplete();
    commandInputElement.focus();
  }
  
  // Hide autocomplete dropdown when clicking outside
  document.addEventListener('click', function(e) {
    if (!commandInputElement.contains(e.target) && !autocompleteDropdown.contains(e.target)) {
      hideAutocomplete();
    }
  });
  
  // Command processing function
  function processCommand() {
    const command = commandInputElement.value.trim();
    
    if (command === '') return;
    
    // Hide autocomplete
    hideAutocomplete();
    
    // Show typing indicator before processing command
    showTypingIndicator();
    
    // Process the command
    if (command.startsWith('/')) {
      handleSlashCommand(command.substring(1).toLowerCase());
    } else {
      // Treat non-command messages as general chat
      addBotResponse(`This is a demonstration of a Discord-like interface. Try using commands like <span class='discord-command'>about</span> to navigate.`);
    }
    
    // Clear input field
    commandInputElement.value = '';
    
    // Focus back on input for better UX
    commandInputElement.focus();
  }
  
  // Handle slash commands
  function handleSlashCommand(cleanCommand) {
    const availableCommands = ['about', 'how-to-play', 'features', 'welcome', 'live-discord'];
    
    if (availableCommands.includes(cleanCommand)) {
      switchChannel(cleanCommand);
    } else {
      addBotResponse(`Unknown command: /${cleanCommand}. Available commands are: ${availableCommands.map(cmd => `<span class='discord-command'>/${cmd}</span>`).join(', ')}`);
    }
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
  
  // Switch to a different channel
  function switchChannel(channelId, updateUrl = true) {
    try {
      // Set isInitialPageLoad to false since we're now navigating between channels
      isInitialPageLoad = false;
      
      // Cancel any pending animation timeouts
      clearAllTimeouts();
      
      // Update active channel in sidebar
      updateActiveChannelInSidebar(channelId);
      
      // Update URL hash if requested (default is true)
      if (updateUrl) {
        window.location.hash = channelId;
      }
      
      // Update current channel display
      currentChannelDisplay.textContent = channelId;
      
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
    const contentTemplateId = `${channelId}-content`;
    const contentTemplate = document.getElementById(contentTemplateId);
    
    if (contentTemplate) {
      // Get template content
      const content = document.importNode(contentTemplate.content, true);
      
      // Create an array to store processed message HTML
      const processedMessages = [];
      
      // Process simple-message elements if any exist
      const simpleMessages = content.querySelectorAll('.simple-message');
      simpleMessages.forEach(simpleMsg => {
        const messageOptions = {
          username: simpleMsg.getAttribute('data-username') || 'Channel Dungeons',
          timestamp: getCurrentTime(), // Always use current time
          content: simpleMsg.innerHTML,
          isSystem: simpleMsg.hasAttribute('data-system')
        };
        
        // Create HTML for the message and add to array instead of modifying DOM
        processedMessages.push(createMessageHTML(messageOptions));
      });
      
      // For older message format, update timestamps to current time
      const oldFormatMessages = content.querySelectorAll('.message');
      oldFormatMessages.forEach(message => {
        const timestampElement = message.querySelector('.message-timestamp');
        if (timestampElement) {
          timestampElement.textContent = getCurrentTime();
        }
        processedMessages.push(message.outerHTML);
      });
      
      // Create a temporary container for the complete content
      const tempContainer = document.createElement('div');
      tempContainer.innerHTML = processedMessages.join('');
      
      // Cache the complete content immediately
      channelMessagesCache[channelId] = tempContainer.innerHTML;
      
      // If accessing any channel via URL (including welcome), show all messages immediately without animation
      if (isInitialPageLoad && hasChannelInUrl) {
        // Add all messages at once without animation
        messagesContainer.innerHTML = channelMessagesCache[channelId];
        scrollToBottom();
      }
      else {
        // Otherwise use the normal animation sequence
        // Now animate adding the messages one by one for the current view only
        const messages = Array.from(tempContainer.children);
        
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
              
              // If this is the last message of the welcome channel without URL hash, show the sidebar and command input
              if (channelId === 'welcome' && !hasChannelInUrl && index === messages.length - 1) {
                // Add a small delay before showing the sidebar to ensure the message is visible
                setTimeout(() => {
                  showSidebar();
                  showCommandInput();
                }, 500);
              }
            }
          }, delayBetweenMessages + showTypingDuration); // Add the message after the typing duration
          
          // Store the timeout ID so it can be cancelled if needed
          activeTimeouts.push(messageTimeoutId);
        });
      }
    } else {
      // No template exists for this channel
      
      if (isInitialPageLoad && hasChannelInUrl) {
        // If direct URL access, show message immediately without typing animation
        const botResponse = `Welcome to the #${channelId} channel. This channel has no content yet.`;
        messagesContainer.innerHTML = createMessageHTML({ content: botResponse });
        
        // Cache the message immediately
        channelMessagesCache[channelId] = messagesContainer.innerHTML;
        
        scrollToBottom();
      } else {
        // Show typing indicator for a short time before displaying the default message
        showTypingIndicator(1000);
        
        setTimeout(() => {
          const botResponse = `Welcome to the #${channelId} channel. This channel has no content yet.`;
          addBotResponse(botResponse);
          
          // Cache the message immediately
          channelMessagesCache[channelId] = messagesContainer.innerHTML;
        }, 1000);
      }
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
  
  // Function to update the active channel in the sidebar
  function updateActiveChannelInSidebar(channelId) {
    channels.forEach(channel => {
      if (channel.getAttribute('data-channel') === channelId) {
        channel.classList.add('active');
      } else {
        channel.classList.remove('active');
      }
    });
  }

  // Mobile touch handling for sidebar
  let touchStartX = 0;
  let touchEndX = 0;
  const minSwipeDistance = 50; // Minimum distance to detect a swipe

  // Create close button for mobile sidebar
  const createMobileSidebarHeader = () => {
    // Check if header already exists
    if (sidebar.querySelector('.mobile-sidebar-header')) return;
    
    const header = document.createElement('div');
    header.className = 'mobile-sidebar-header';
    
    const closeButton = document.createElement('button');
    closeButton.className = 'close-sidebar';
    closeButton.innerHTML = '×';
    closeButton.setAttribute('aria-label', 'Close sidebar');
    
    closeButton.addEventListener('click', () => {
      hideSidebar();
    });
    
    header.appendChild(closeButton);
    sidebar.insertBefore(header, sidebar.firstChild);
  };

  // Show sidebar (mobile)
  const showMobileSidebar = () => {
    createMobileSidebarHeader();
    sidebar.classList.add('visible');
    // Set sidebarShown to true to track state correctly
    sidebarShown = true;
  };

  // Hide sidebar (mobile)
  const hideSidebar = () => {
    sidebar.classList.remove('visible');
    // We don't set sidebarShown to false here since it tracks if sidebar has been shown at any point
  };

  // Handle touch start
  document.addEventListener('touchstart', (e) => {
    touchStartX = e.changedTouches[0].screenX;
  }, { passive: true });

  // Handle touch end
  document.addEventListener('touchend', (e) => {
    touchEndX = e.changedTouches[0].screenX;
    handleSwipe();
  }, { passive: true });

  // Process swipe gesture
  const handleSwipe = () => {
    const swipeDistance = touchEndX - touchStartX;
    
    // Right swipe (to show sidebar) - works from anywhere on screen
    if (swipeDistance > minSwipeDistance && !sidebar.classList.contains('visible')) {
      showMobileSidebar();
    }
    
    // Left swipe (to hide sidebar when visible) - works from anywhere on screen
    if (swipeDistance < -minSwipeDistance && sidebar.classList.contains('visible')) {
      hideSidebar();
    }
  };

  // Hide sidebar when clicking on a channel on mobile
  channels.forEach(channel => {
    channel.addEventListener('click', function() {
      if (window.innerWidth <= 768) {
        hideSidebar();
      }
    });
  });

  // Hide sidebar when clicking outside on mobile
  document.addEventListener('click', function(e) {
    if (window.innerWidth <= 768 && 
        sidebar.classList.contains('visible') && 
        !sidebar.contains(e.target)) {
      hideSidebar();
    }
  });

  // Window resize handler
  window.addEventListener('resize', () => {
    // Check if we're transitioning between view modes
    const wasInMobileView = isMobileView;
    isMobileView = window.innerWidth <= 768;
    
    // If we're transitioning from mobile to desktop view
    if (wasInMobileView && !isMobileView) {
      // Restore desktop sidebar behavior
      if (sidebarShown) {
        // Remove mobile-specific header if it exists
        const mobileHeader = sidebar.querySelector('.mobile-sidebar-header');
        if (mobileHeader) {
          mobileHeader.remove();
        }
        
        // Show sidebar properly for desktop view
        sidebar.classList.add('visible');
        contentArea.classList.add('sidebar-visible');
      }
    } 
    // If transitioning from desktop to mobile view
    else if (!wasInMobileView && isMobileView) {
      // In mobile view, sidebar should be hidden initially
      sidebar.classList.remove('visible');
      contentArea.classList.remove('sidebar-visible');
      
      // If sidebar should be shown, ensure mobile header exists
      if (sidebarShown) {
        createMobileSidebarHeader();
      }
    }
  });

  // Get the sidebar toggle button
  const sidebarToggle = document.querySelector('.sidebar-toggle');
  
  // Add event listener for sidebar toggle button
  if (sidebarToggle) {
    sidebarToggle.addEventListener('click', (e) => {
      // Prevent any default action
      e.preventDefault();
      e.stopPropagation();
      
      if (window.innerWidth <= 768) {
        // For mobile, always show the mobile sidebar
        showMobileSidebar();
      } else {
        // For desktop, use the standard sidebar toggle
        sidebar.classList.toggle('visible');
      }
    });
  }
});
