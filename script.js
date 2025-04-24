/**
 * Channel Dungeons Website - Discord-like interface
 * Main application script
 */
document.addEventListener('DOMContentLoaded', function() {
  /**
   * App Configuration
   */
  const config = {
    transitionDurations: {
      typing: 1000,
      messageBetweenDelay: 200,
      uiShowDelay: 500
    },
    breakpoints: {
      mobile: 768
    }
  };

  /**
   * Application State
   */
  const state = {
    currentChannel: 'welcome',
    selectedAutocompleteIndex: -1,
    channelMessagesCache: {},
    activeTimeouts: [],
    sidebarShown: false,
    inputShown: false,
    isMobileView: window.innerWidth <= config.breakpoints.mobile,
    typingTimeoutId: null,
    isInitialPageLoad: true,
    hasChannelInUrl: window.location.hash.length > 1
  };

  /**
   * Available Commands
   */
  const availableCommands = [
    { name: 'about', description: 'Learn about Channel Dungeons' },
    { name: 'how-to-play', description: 'Get started with the game' },
    { name: 'features', description: 'Discover game features' },
    { name: 'welcome', description: 'Return to welcome channel' },
    { name: 'live-discord', description: 'See the live Discord experience' }
  ];

  /**
   * DOM Elements
   */
  const elements = {
    messagesContainer: document.getElementById('messages'),
    channels: document.querySelectorAll('.channel'),
    currentChannelDisplay: document.getElementById('current-channel'),
    commandInputElement: document.getElementById('command-input'),
    commandInputContainer: document.querySelector('.command-input-container'),
    sidebar: document.querySelector('.sidebar'),
    contentArea: document.querySelector('.content-area'),
    typingIndicator: document.getElementById('typing-indicator'),
    autocompleteDropdown: document.getElementById('autocomplete-dropdown'),
    sidebarToggle: document.querySelector('.sidebar-toggle'),
    messageArea: document.querySelector('.message-area')
  };

  /**
   * Initialize the application
   */
  function init() {
    // Initialize channel from URL hash
    state.currentChannel = getChannelFromHash();
    elements.currentChannelDisplay.textContent = state.currentChannel;
    
    // Load initial content and update UI
    loadChannelContent(state.currentChannel);
    updateActiveChannelInSidebar(state.currentChannel);
    
    // Show UI elements for direct channel access
    if (state.hasChannelInUrl) {
      if (!state.isMobileView) {
        toggleSidebar(true);
      }
      state.sidebarShown = true;
      showCommandInput();
    }
    
    bindEvents();
    handleHeightCalculation();
  }

  /**
   * Bind all event listeners
   */
  function bindEvents() {
    // Channel switching via sidebar using event delegation
    document.querySelector('.channels-list').addEventListener('click', function(e) {
      const channel = e.target.closest('.channel');
      if (channel) {
        const channelId = channel.getAttribute('data-channel');
        switchChannel(channelId);
        if (state.isMobileView) toggleSidebar(false);
      }
    });
    
    // Handle browser navigation
    window.addEventListener('hashchange', function() {
      const channelFromHash = getChannelFromHash();
      if (channelFromHash !== state.currentChannel) {
        switchChannel(channelFromHash, false);
      }
    });
    
    // Command input handling
    elements.commandInputElement.addEventListener('keypress', function(e) {
      if (e.key === 'Enter') processCommand();
    });
    
    // Autocomplete
    elements.commandInputElement.addEventListener('input', function() {
      showAutocomplete(elements.commandInputElement.value);
    });
    
    // Keyboard navigation in autocomplete
    elements.commandInputElement.addEventListener('keydown', handleAutocompleteKeydown);
    
    // Hide autocomplete on click outside
    document.addEventListener('click', function(e) {
      if (!elements.commandInputElement.contains(e.target) && 
          !elements.autocompleteDropdown.contains(e.target)) {
        hideAutocomplete();
      }
    });
    
    // Mobile touch handling for sidebar
    setupMobileTouchHandling();
    
    // Window resize handler
    window.addEventListener('resize', handleWindowResize);
    
    // Sidebar toggle button handler
    if (elements.sidebarToggle) {
      elements.sidebarToggle.addEventListener('click', function(e) {
        e.preventDefault();
        e.stopPropagation();
        toggleSidebar();
      });
    }
  }

  /**
   * Channel Management Functions
   */
  
  // Get channel from URL hash
  function getChannelFromHash() {
    const hash = window.location.hash.substring(1);
    const validChannels = Array.from(elements.channels).map(channel => 
      channel.getAttribute('data-channel')
    );
    return validChannels.includes(hash) ? hash : 'welcome';
  }
  
  // Switch channel
  function switchChannel(channelId, updateUrl = true) {
    try {
      state.isInitialPageLoad = false;
      clearAllTimeouts();
      updateActiveChannelInSidebar(channelId);
      
      if (updateUrl) {
        window.location.hash = channelId;
      }
      
      elements.currentChannelDisplay.textContent = channelId;
      state.currentChannel = channelId;
      elements.messagesContainer.innerHTML = '';
      
      if (state.channelMessagesCache[channelId]) {
        displayCachedMessages(channelId);
      } else {
        showTypingIndicator(2000);
        loadChannelContent(channelId);
      }
    } catch (error) {
      console.error('Error switching channels:', error);
    }
  }
  
  // Load channel content
  function loadChannelContent(channelId) {
    const contentTemplateId = `${channelId}-content`;
    const contentTemplate = document.getElementById(contentTemplateId);
    
    if (contentTemplate) {
      processContentTemplate(contentTemplate, channelId);
    } else {
      handleEmptyChannel(channelId);
    }
  }
  
  // Process content template
  function processContentTemplate(template, channelId) {
    // Process template content
    const content = document.importNode(template.content, true);
    const processedMessages = [];
    
    // Process simple messages
    const simpleMessages = content.querySelectorAll('.simple-message');
    simpleMessages.forEach(simpleMsg => {
      const messageOptions = {
        username: simpleMsg.getAttribute('data-username') || 'Channel Dungeons',
        timestamp: getCurrentTime(),
        content: simpleMsg.innerHTML
      };
      
      processedMessages.push(createMessageHTML(messageOptions));
    });
    
    // Process old format messages
    const oldFormatMessages = content.querySelectorAll('.message');
    oldFormatMessages.forEach(message => {
      const timestampElement = message.querySelector('.message-timestamp');
      if (timestampElement) {
        timestampElement.textContent = getCurrentTime();
      }
      processedMessages.push(message.outerHTML);
    });
    
    // Create temporary container
    const tempContainer = document.createElement('div');
    tempContainer.innerHTML = processedMessages.join('');
    
    // Cache content
    state.channelMessagesCache[channelId] = tempContainer.innerHTML;
    
    // Display messages
    if (state.isInitialPageLoad && state.hasChannelInUrl) {
      displayMessagesImmediately(channelId);
    } else {
      animateMessages(tempContainer, channelId);
    }
  }
  
  // Display messages immediately (no animation)
  function displayMessagesImmediately(channelId) {
    elements.messagesContainer.innerHTML = state.channelMessagesCache[channelId];
    scrollToBottom();
  }
  
  // Animate messages appearance
  function animateMessages(container, channelId) {
    const messages = Array.from(container.children);
    const showTypingDuration = config.transitionDurations.typing;
    
    messages.forEach((message, index) => {
      const messageClone = message.cloneNode(true);
      const delayBetweenMessages = index * (showTypingDuration + config.transitionDurations.messageBetweenDelay);
      
      // Set typing indicator timeout
      const typingTimeoutId = setTimeout(() => {
        if (state.currentChannel === channelId) {
          showTypingIndicator(showTypingDuration);
        }
      }, delayBetweenMessages);
      state.activeTimeouts.push(typingTimeoutId);
      
      // Set message display timeout
      const messageTimeoutId = setTimeout(() => {
        if (state.currentChannel === channelId) {
          hideTypingIndicator();
          elements.messagesContainer.appendChild(messageClone);
          scrollToBottom();
          
          // Show UI for last welcome message
          if (channelId === 'welcome' && !state.hasChannelInUrl && index === messages.length - 1) {
            setTimeout(() => {
              if (!state.isMobileView) toggleSidebar(true);
              showCommandInput();
            }, config.transitionDurations.uiShowDelay);
          }
        }
      }, delayBetweenMessages + showTypingDuration);
      
      state.activeTimeouts.push(messageTimeoutId);
    });
  }
  
  // Handle empty channel
  function handleEmptyChannel(channelId) {
    if (state.isInitialPageLoad && state.hasChannelInUrl) {
      // Direct URL access
      const botResponse = `Welcome to the #${channelId} channel. This channel has no content yet.`;
      elements.messagesContainer.innerHTML = createMessageHTML({ content: botResponse });
      state.channelMessagesCache[channelId] = elements.messagesContainer.innerHTML;
      scrollToBottom();
    } else {
      // Normal navigation
      showTypingIndicator(config.transitionDurations.typing);
      
      setTimeout(() => {
        const botResponse = `Welcome to the #${channelId} channel. This channel has no content yet.`;
        addBotResponse(botResponse);
        state.channelMessagesCache[channelId] = elements.messagesContainer.innerHTML;
      }, config.transitionDurations.typing);
    }
  }
  
  // Display cached messages
  function displayCachedMessages(channelId) {
    hideTypingIndicator();
    elements.messagesContainer.innerHTML = state.channelMessagesCache[channelId];
    scrollToBottom();
  }
  
  // Update active channel in sidebar
  function updateActiveChannelInSidebar(channelId) {
    elements.channels.forEach(channel => {
      if (channel.getAttribute('data-channel') === channelId) {
        channel.classList.add('active');
      } else {
        channel.classList.remove('active');
      }
    });
  }

  /**
   * UI Manipulation Functions
   */
  
  // Sidebar management
  function toggleSidebar(show = null) {
    const shouldShow = show !== null ? show : !elements.sidebar.classList.contains('visible');
    
    if (shouldShow) {
      elements.sidebar.classList.add('visible');
      if (!state.isMobileView) elements.contentArea.classList.add('sidebar-visible');
      state.sidebarShown = true;
      if (elements.sidebarToggle) elements.sidebarToggle.setAttribute('aria-expanded', 'true');
    } else {
      elements.sidebar.classList.remove('visible');
      if (!state.isMobileView) elements.contentArea.classList.remove('sidebar-visible');
      state.sidebarShown = false;
      if (elements.sidebarToggle) elements.sidebarToggle.setAttribute('aria-expanded', 'false');
    }
  }

  // Show command input with animation
  function showCommandInput() {
    if (state.inputShown) return;
    state.inputShown = true;
    elements.commandInputContainer.classList.add('visible');
    setTimeout(() => elements.commandInputElement.focus(), 300);
  }
  
  // Show typing indicator
  function showTypingIndicator(duration = config.transitionDurations.typing) {
    if (state.typingTimeoutId) clearTimeout(state.typingTimeoutId);
    
    elements.typingIndicator.classList.add('active');
    
    // Move typing indicator after the last message
    const lastMessage = elements.messagesContainer.lastElementChild;
    if (lastMessage) {
      lastMessage.insertAdjacentElement('afterend', elements.typingIndicator);
    } else {
      elements.messagesContainer.appendChild(elements.typingIndicator);
    }
    
    scrollToBottom();
    
    state.typingTimeoutId = setTimeout(() => {
      hideTypingIndicator();
    }, duration);
    
    return state.typingTimeoutId;
  }
  
  // Hide typing indicator
  function hideTypingIndicator() {
    elements.typingIndicator.classList.remove('active');
    if (state.typingTimeoutId) {
      clearTimeout(state.typingTimeoutId);
      state.typingTimeoutId = null;
    }
  }
  
  // Clear all timeouts
  function clearAllTimeouts() {
    state.activeTimeouts.forEach(timeoutId => clearTimeout(timeoutId));
    state.activeTimeouts = [];
    
    if (state.typingTimeoutId) {
      clearTimeout(state.typingTimeoutId);
      state.typingTimeoutId = null;
    }
    
    hideTypingIndicator();
  }
  
  // Add bot response
  function addBotResponse(text) {
    hideTypingIndicator();
    
    const messageHTML = createMessageHTML({
      content: text
    });
    
    elements.messagesContainer.insertAdjacentHTML('beforeend', messageHTML);
    scrollToBottom();
    
    // Update cache
    state.channelMessagesCache[state.currentChannel] = elements.messagesContainer.innerHTML;
  }
  
  // Create message HTML
  function createMessageHTML(options = {}) {
    const {
      username = 'Channel Dungeons',
      timestamp = getCurrentTime(),
      content = '',
    } = options;
    
    return `
      <div class="message">
        <div class="message-avatar" aria-hidden="true"></div>
        <div class="message-content">
          <div class="message-header">
            <span class="message-username">${username}</span>
            <span class="message-timestamp">${timestamp}</span>
          </div>
          <div class="message-text">${content}</div>
        </div>
      </div>
    `;
  }
  
  // Scroll to bottom of message area
  function scrollToBottom() {
    if (elements.messageArea) {
      setTimeout(() => {
        elements.messageArea.scrollTop = elements.messageArea.scrollHeight;
      }, 10);
    }
  }
  
  // Handle height calculation for mobile
  function handleHeightCalculation() {
    const header = document.querySelector('.channel-header');
    const messageArea = document.querySelector('.message-area');
    const footerArea = document.querySelector('footer');
    
    if (header && messageArea && footerArea && window.innerWidth <= config.breakpoints.mobile) {
      const headerHeight = header.offsetHeight;
      const footerHeight = footerArea.offsetHeight;
      messageArea.style.height = `calc(100% - ${headerHeight + footerHeight}px)`;
    }
  }

  // Handle window resize
  function handleWindowResize() {
    const wasInMobileView = state.isMobileView;
    state.isMobileView = window.innerWidth <= config.breakpoints.mobile;
    
    // Transition between mobile and desktop view
    if (wasInMobileView && !state.isMobileView) {
      if (state.sidebarShown) toggleSidebar(true);
    } else if (!wasInMobileView && state.isMobileView) {
      toggleSidebar(false);
    }
    
    // Recalculate heights
    handleHeightCalculation();
  }

  /**
   * Command and Autocomplete Functions
   */
  
  // Process command
  function processCommand() {
    const command = elements.commandInputElement.value.trim();
    if (command === '') return;
    
    hideAutocomplete();
    showTypingIndicator();
    
    if (command.startsWith('/')) {
      handleSlashCommand(command.substring(1).toLowerCase());
    } else {
      addBotResponse(`This is a demonstration of a Discord-like interface. Try using commands like <span class='discord-command'>about</span> to navigate.`);
    }
    
    elements.commandInputElement.value = '';
    elements.commandInputElement.focus();
  }
  
  // Handle slash commands
  function handleSlashCommand(cleanCommand) {
    const commandNames = availableCommands.map(cmd => cmd.name);
    
    if (commandNames.includes(cleanCommand)) {
      switchChannel(cleanCommand);
    } else {
      addBotResponse(`Unknown command: /${cleanCommand}. Available commands are: ${commandNames.map(cmd => `<span class='discord-command'>/${cmd}</span>`).join(', ')}`);
    }
  }
  
  // Show autocomplete dropdown
  function showAutocomplete(inputValue) {
    if (!inputValue.startsWith('/')) {
      hideAutocomplete();
      return;
    }
    
    const searchTerm = inputValue.substring(1).toLowerCase();
    const matchingCommands = availableCommands.filter(cmd => 
      cmd.name.toLowerCase().startsWith(searchTerm)
    );
    
    if (matchingCommands.length === 0) {
      hideAutocomplete();
      return;
    }
    
    elements.autocompleteDropdown.innerHTML = '';
    
    matchingCommands.forEach((cmd, index) => {
      const item = document.createElement('div');
      item.className = 'autocomplete-item';
      item.dataset.commandName = cmd.name;
      item.innerHTML = `<span class="command-name">/${cmd.name}</span><span class="command-description">${cmd.description}</span>`;
      elements.autocompleteDropdown.appendChild(item);
    });
    
    // Add a single event listener using event delegation
    if (!elements.autocompleteDropdown._hasClickListener) {
      elements.autocompleteDropdown.addEventListener('click', (e) => {
        const item = e.target.closest('.autocomplete-item');
        if (item) {
          const commandName = item.dataset.commandName;
          selectCommand(commandName);
        }
      });
      elements.autocompleteDropdown._hasClickListener = true;
    }
    
    elements.autocompleteDropdown.classList.add('visible');
    state.selectedAutocompleteIndex = 0;
    updateAutocompleteSelection();
  }
  
  // Handle autocomplete keyboard navigation
  function handleAutocompleteKeydown(e) {
    const items = elements.autocompleteDropdown.querySelectorAll('.autocomplete-item');
    if (!elements.autocompleteDropdown.classList.contains('visible')) return;
    
    switch(e.key) {
      case 'ArrowDown':
        e.preventDefault();
        state.selectedAutocompleteIndex = Math.min(state.selectedAutocompleteIndex + 1, items.length - 1);
        updateAutocompleteSelection();
        break;
      case 'ArrowUp':
        e.preventDefault();
        state.selectedAutocompleteIndex = Math.max(state.selectedAutocompleteIndex - 1, 0);
        updateAutocompleteSelection();
        break;
      case 'Tab':
      case 'Enter':
        if (state.selectedAutocompleteIndex >= 0 && state.selectedAutocompleteIndex < items.length) {
          e.preventDefault();
          const commandName = items[state.selectedAutocompleteIndex].querySelector('.command-name').textContent.substring(1);
          selectCommand(commandName);
        }
        break;
      case 'Escape':
        hideAutocomplete();
        break;
    }
  }
  
  // Hide autocomplete dropdown
  function hideAutocomplete() {
    elements.autocompleteDropdown.classList.remove('visible');
    state.selectedAutocompleteIndex = -1;
  }
  
  // Update autocomplete selection
  function updateAutocompleteSelection() {
    const items = elements.autocompleteDropdown.querySelectorAll('.autocomplete-item');
    
    items.forEach(item => item.classList.remove('selected'));
    
    if (state.selectedAutocompleteIndex >= 0 && state.selectedAutocompleteIndex < items.length) {
      items[state.selectedAutocompleteIndex].classList.add('selected');
      items[state.selectedAutocompleteIndex].scrollIntoView({ block: 'nearest' });
    }
  }
  
  // Select command from autocomplete
  function selectCommand(commandName) {
    elements.commandInputElement.value = '/' + commandName;
    hideAutocomplete();
    elements.commandInputElement.focus();
  }

  /**
   * Mobile Touch Handling
   */
  function setupMobileTouchHandling() {
    let touchStartX = 0;
    let touchEndX = 0;

    document.addEventListener('touchstart', (e) => {
      touchStartX = e.changedTouches[0].screenX;
    }, { passive: true });

    document.addEventListener('touchend', (e) => {
      touchEndX = e.changedTouches[0].screenX;
      handleSwipe(touchStartX, touchEndX);
    }, { passive: true });
  }
  
  // Handle swipe gesture
  function handleSwipe(startX, endX) {
    const swipeDistance = endX - startX;
    const minSwipeDistance = 50;
    
    // Right swipe to show sidebar
    if (swipeDistance > minSwipeDistance && !elements.sidebar.classList.contains('visible')) {
      toggleSidebar(true);
    }
    
    // Left swipe to hide sidebar
    if (swipeDistance < -minSwipeDistance && elements.sidebar.classList.contains('visible')) {
      toggleSidebar(false);
    }
  }

  /**
   * Utility Functions
   */
  
  // Get current time
  function getCurrentTime() {
    const now = new Date();
    const timeStr = now.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', hour12: true });
    return timeStr.replace(/am|pm/i, match => match.toUpperCase());
  }

  // Initialize the application
  init();
});
