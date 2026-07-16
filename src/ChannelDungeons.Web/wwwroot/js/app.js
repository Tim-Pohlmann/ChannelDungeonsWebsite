// Small DOM interop helpers for the Blazor Channel Dungeons UI.
// Kept intentionally thin: anything that can be expressed in Razor/C# lives there instead.

export function getHash() {
  return window.location.hash.substring(1);
}

export function setHash(value) {
  window.location.hash = value;
}

export function scrollToBottom(elementId) {
  const el = document.getElementById(elementId);
  if (el) {
    el.scrollTop = el.scrollHeight;
  }
}

export function focusElement(elementId) {
  const el = document.getElementById(elementId);
  if (el) {
    el.focus();
  }
}

export function scrollIntoView(elementId) {
  const el = document.getElementById(elementId);
  if (el) {
    el.scrollIntoView({ block: 'nearest' });
  }
}

export function isMobileViewport(breakpointPx) {
  return window.innerWidth <= breakpointPx;
}

let dotNetRef = null;
let touchStartX = 0;
let mobileBreakpointPx = 768;

function onHashChange() {
  dotNetRef?.invokeMethodAsync('OnHashChanged', window.location.hash.substring(1));
}

function onResize() {
  dotNetRef?.invokeMethodAsync('OnWindowResized', window.innerWidth <= mobileBreakpointPx);
}

function onTouchStart(e) {
  touchStartX = e.changedTouches[0].screenX;
}

function onTouchEnd(e) {
  const touchEndX = e.changedTouches[0].screenX;
  dotNetRef?.invokeMethodAsync('OnSwipe', touchEndX - touchStartX);
}

export function registerAppListeners(reference, breakpointPx) {
  dotNetRef = reference;
  mobileBreakpointPx = breakpointPx;
  window.addEventListener('hashchange', onHashChange);
  window.addEventListener('resize', onResize);
  document.addEventListener('touchstart', onTouchStart, { passive: true });
  document.addEventListener('touchend', onTouchEnd, { passive: true });
}

export function unregisterAppListeners() {
  window.removeEventListener('hashchange', onHashChange);
  window.removeEventListener('resize', onResize);
  document.removeEventListener('touchstart', onTouchStart);
  document.removeEventListener('touchend', onTouchEnd);
  dotNetRef = null;
}
