globalThis.channelDungeons = {
    isMobile: function() {
        return globalThis.innerWidth <= 768;
    },

    setupCommandInputKeyHandler: function(inputElement) {
        if (!inputElement) return;

        inputElement.addEventListener('keydown', function(e) {
            // Only prevent default for navigation keys when autocomplete is visible
            const autocomplete = inputElement.closest('.autocomplete-container')?.querySelector('.autocomplete-dropdown');
            if (!autocomplete || autocomplete.childElementCount === 0) return;

            // Prevent default for Tab, ArrowUp, ArrowDown when autocomplete is active
            if (e.key === 'Tab' || e.key === 'ArrowUp' || e.key === 'ArrowDown') {
                e.preventDefault();
            }
        });
    }
};