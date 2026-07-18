/**
 * Browser interop for the Channel Dungeons Blazor app: viewport tracking,
 * swipe gestures, scrolling, and mobile height calculation.
 */
window.channelDungeons = (function () {
  'use strict';

  const MOBILE_BREAKPOINT = 768;
  const MIN_SWIPE_DISTANCE = 50;

  function isMobileView() {
    return window.innerWidth <= MOBILE_BREAKPOINT;
  }

  function recalcMobileHeight() {
    const header = document.querySelector('.channel-header');
    const messageArea = document.querySelector('.message-area');
    const footer = document.querySelector('footer');

    if (header && messageArea && footer && isMobileView()) {
      const reserved = header.offsetHeight + footer.offsetHeight;
      messageArea.style.height = `calc(100% - ${reserved}px)`;
    }
  }

  return {
    /**
     * Registers viewport and swipe listeners that call back into .NET.
     * Returns whether the viewport is currently mobile-sized.
     */
    init: function (dotNetRef) {
      window.addEventListener('resize', function () {
        dotNetRef.invokeMethodAsync('OnViewportResized', isMobileView());
        recalcMobileHeight();
      });

      let touchStartX = 0;
      document.addEventListener('touchstart', function (e) {
        touchStartX = e.changedTouches[0].screenX;
      }, { passive: true });

      document.addEventListener('touchend', function (e) {
        const distance = e.changedTouches[0].screenX - touchStartX;
        if (Math.abs(distance) > MIN_SWIPE_DISTANCE) {
          dotNetRef.invokeMethodAsync('OnSwipe', distance > 0);
        }
      }, { passive: true });

      recalcMobileHeight();
      return isMobileView();
    },

    scrollToBottom: function (element) {
      if (element) {
        setTimeout(function () {
          element.scrollTop = element.scrollHeight;
        }, 10);
      }
    },

    /**
     * Suppresses browser defaults for autocomplete navigation keys while the
     * dropdown is open, so the caret does not move and Tab does not steal
     * focus. The Blazor keydown handler performs the actual navigation.
     */
    registerInputKeys: function (input, dropdown) {
      if (!input || !dropdown) {
        return;
      }
      input.addEventListener('keydown', function (e) {
        const handledKeys = ['ArrowDown', 'ArrowUp', 'Tab', 'Enter'];
        if (dropdown.classList.contains('visible') && handledKeys.includes(e.key)) {
          e.preventDefault();
        }
      });
    }
  };
})();
