globalThis.channelDungeons = {
    isMobile: function() {
        return globalThis.innerWidth <= 768;
    },

    setupCommandInputKeyHandler: function(inputElement) {
        if (!inputElement || inputElement._keyHandler) return;

        inputElement._keyHandler = function(e) {
            // Only prevent default for navigation keys when autocomplete is visible
            const autocomplete = inputElement.closest('.autocomplete-container')?.querySelector('.autocomplete-dropdown');
            if (!autocomplete || autocomplete.childElementCount === 0) return;

            // Prevent default for Tab, ArrowUp, ArrowDown when autocomplete is active
            if (e.key === 'Tab' || e.key === 'ArrowUp' || e.key === 'ArrowDown') {
                e.preventDefault();
            }
        };
        inputElement.addEventListener('keydown', inputElement._keyHandler);
    },

    removeCommandInputKeyHandler: function(inputElement) {
        if (!inputElement || !inputElement._keyHandler) return;
        inputElement.removeEventListener('keydown', inputElement._keyHandler);
        delete inputElement._keyHandler;
    }
};