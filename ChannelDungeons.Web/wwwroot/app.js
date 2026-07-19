/**
 * Browser interop module for the Channel Dungeons Blazor app: viewport
 * tracking, swipe gestures, scrolling, and mobile height calculation.
 * Loaded on demand by Services/BrowserInterop.cs.
 */

const MOBILE_BREAKPOINT = 768;
const MIN_SWIPE_DISTANCE = 50;

let listeners = [];

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

function addListener(target, type, handler, options) {
  target.addEventListener(type, handler, options);
  listeners.push({ target, type, handler });
}

function removeListeners() {
  for (const { target, type, handler } of listeners) {
    target.removeEventListener(type, handler);
  }
  listeners = [];
}

function wireErrorUiDismiss() {
  const errorUi = document.getElementById('blazor-error-ui');
  const dismiss = errorUi && errorUi.querySelector('.dismiss');
  if (dismiss && !dismiss.dataset.wired) {
    dismiss.dataset.wired = 'true';
    dismiss.addEventListener('click', function () {
      errorUi.style.display = 'none';
    });
  }
}

/**
 * Registers viewport and swipe listeners that call back into .NET.
 * Returns whether the viewport is currently mobile-sized.
 */
export function init(dotNetRef) {
  removeListeners();
  wireErrorUiDismiss();

  addListener(window, 'resize', function () {
    dotNetRef.invokeMethodAsync('OnViewportResized', isMobileView());
    recalcMobileHeight();
  });

  let touchStartX = 0;
  addListener(document, 'touchstart', function (e) {
    touchStartX = e.changedTouches[0].screenX;
  }, { passive: true });

  addListener(document, 'touchend', function (e) {
    const distance = e.changedTouches[0].screenX - touchStartX;
    if (Math.abs(distance) > MIN_SWIPE_DISTANCE) {
      dotNetRef.invokeMethodAsync('OnSwipe', distance > 0);
    }
  }, { passive: true });

  recalcMobileHeight();
  return isMobileView();
}

/** Detaches the listeners registered by init. */
export function dispose() {
  removeListeners();
}

export function scrollToBottom(element) {
  if (element) {
    setTimeout(function () {
      element.scrollTop = element.scrollHeight;
    }, 10);
  }
}

/** Scrolls the container's child at the given index into view. */
export function scrollItemIntoView(container, index) {
  const item = container && container.children[index];
  if (item) {
    item.scrollIntoView({ block: 'nearest' });
  }
}

/**
 * Suppresses browser defaults for autocomplete navigation keys while the
 * dropdown is open, so the caret does not move and Tab does not steal
 * focus. The Blazor keydown handler performs the actual navigation.
 */
export function registerInputKeys(input, dropdown) {
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
