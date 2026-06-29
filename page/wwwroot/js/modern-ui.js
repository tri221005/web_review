/**
 * Modern UI Enhancement JavaScript
 * Vietnam Hanh Trinh - Travel Review Platform
 * 
 * Features:
 * - Dark/Light Theme Toggle with persistence
 * - Ripple Effect on buttons
 * - Scroll Reveal Animations
 * - Toast Notification System
 * - Skeleton Loading helpers
 */

(function() {
  'use strict';

  // =========================================
  // THEME MANAGEMENT
  // =========================================
  const ThemeManager = {
    init() {
      this.loadTheme();
      this.createToggleButton();
      this.bindEvents();
    },

    loadTheme() {
      const savedTheme = localStorage.getItem('theme') || 'dark';
      document.documentElement.setAttribute('data-theme', savedTheme);
      this.updateToggleIcon(savedTheme);
    },

    toggleTheme() {
      const currentTheme = document.documentElement.getAttribute('data-theme');
      const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
      
      document.documentElement.setAttribute('data-theme', newTheme);
      localStorage.setItem('theme', newTheme);
      this.updateToggleIcon(newTheme);
      
      // Animate transition
      document.body.style.transition = 'background-color 0.3s, color 0.3s';
      
      // Show toast notification
      Toast.show({
        type: 'info',
        title: 'Theme Changed',
        message: `Switched to ${newTheme} mode`,
        duration: 2000
      });
    },

    updateToggleIcon(theme) {
      const toggle = document.querySelector('.theme-toggle');
      if (toggle) {
        toggle.innerHTML = theme === 'dark' 
          ? '<i class="fas fa-sun"></i>' 
          : '<i class="fas fa-moon"></i>';
      }
    },

    createToggleButton() {
      // Check if toggle already exists
      if (document.querySelector('.theme-toggle')) return;

      const toggle = document.createElement('button');
      toggle.className = 'theme-toggle';
      toggle.setAttribute('aria-label', 'Toggle dark/light mode');
      toggle.innerHTML = '<i class="fas fa-sun"></i>';
      
      document.body.appendChild(toggle);
    },

    bindEvents() {
      const toggle = document.querySelector('.theme-toggle');
      if (toggle) {
        toggle.addEventListener('click', () => this.toggleTheme());
      }
    }
  };

  // =========================================
  // RIPPLE EFFECT
  // =========================================
  const RippleEffect = {
    init() {
      this.bindEvents();
    },

    bindEvents() {
      document.addEventListener('click', (e) => {
        const button = e.target.closest('.btn-modern');
        if (!button) return;

        const rect = button.getBoundingClientRect();
        const x = e.clientX - rect.left;
        const y = e.clientY - rect.top;

        const ripple = document.createElement('span');
        ripple.className = 'ripple';
        ripple.style.left = `${x}px`;
        ripple.style.top = `${y}px`;
        
        button.appendChild(ripple);

        setTimeout(() => ripple.remove(), 600);
      });
    }
  };

  // =========================================
  // SCROLL REVEAL ANIMATIONS
  // =========================================
  const ScrollReveal = {
    init() {
      this.observeElements();
    },

    observeElements() {
      const elements = document.querySelectorAll('.reveal, .reveal-left, .reveal-right');
      
      if ('IntersectionObserver' in window) {
        const observer = new IntersectionObserver((entries) => {
          entries.forEach(entry => {
            if (entry.isIntersecting) {
              entry.target.classList.add('active');
              observer.unobserve(entry.target);
            }
          });
        }, {
          threshold: 0.1,
          rootMargin: '0px 0px -50px 0px'
        });

        elements.forEach(el => observer.observe(el));
      } else {
        // Fallback for older browsers
        elements.forEach(el => el.classList.add('active'));
      }
    }
  };

  // =========================================
  // TOAST NOTIFICATION SYSTEM
  // =========================================
  const Toast = {
    container: null,

    init() {
      this.createContainer();
    },

    createContainer() {
      if (document.querySelector('.toast-container')) {
        this.container = document.querySelector('.toast-container');
        return;
      }

      this.container = document.createElement('div');
      this.container.className = 'toast-container';
      document.body.appendChild(this.container);
    },

    show(options = {}) {
      const {
        type = 'info',
        title = 'Notification',
        message = '',
        duration = 4000,
        closable = true
      } = options;

      const toast = document.createElement('div');
      toast.className = `toast-modern ${type}`;
      
      const icon = this.getIcon(type);
      
      toast.innerHTML = `
        ${icon}
        <div class="toast-content">
          <div class="toast-title">${title}</div>
          <div class="toast-message">${message}</div>
        </div>
        ${closable ? '<button class="toast-close" aria-label="Close">&times;</button>' : ''}
        <div class="toast-progress" style="animation-duration: ${duration}ms"></div>
      `;

      this.container.appendChild(toast);

      // Close button handler
      const closeBtn = toast.querySelector('.toast-close');
      if (closeBtn) {
        closeBtn.addEventListener('click', () => this.dismiss(toast));
      }

      // Auto dismiss
      if (duration > 0) {
        setTimeout(() => this.dismiss(toast), duration);
      }

      return toast;
    },

    dismiss(toast) {
      toast.style.animation = 'slideOutRight 0.4s cubic-bezier(0.4, 0, 0.2, 1) forwards';
      setTimeout(() => toast.remove(), 400);
    },

    getIcon(type) {
      const icons = {
        success: '<i class="fas fa-check-circle" style="color: var(--green); font-size: 24px;"></i>',
        error: '<i class="fas fa-exclamation-circle" style="color: var(--red); font-size: 24px;"></i>',
        warning: '<i class="fas fa-exclamation-triangle" style="color: var(--gold-hover); font-size: 24px;"></i>',
        info: '<i class="fas fa-info-circle" style="color: var(--gold); font-size: 24px;"></i>'
      };
      return icons[type] || icons.info;
    },

    success(title, message, duration) {
      return this.show({ type: 'success', title, message, duration });
    },

    error(title, message, duration) {
      return this.show({ type: 'error', title, message, duration });
    },

    warning(title, message, duration) {
      return this.show({ type: 'warning', title, message, duration });
    },

    info(title, message, duration) {
      return this.show({ type: 'info', title, message, duration });
    }
  };

  // =========================================
  // SKELETON LOADING HELPER
  // =========================================
  const SkeletonLoader = {
    show(containerId, type = 'card') {
      const container = document.getElementById(containerId);
      if (!container) return;

      let skeleton = '';
      
      if (type === 'card') {
        skeleton = `
          <div class="skeleton-image"></div>
          <div class="skeleton-title"></div>
          <div class="skeleton-text"></div>
          <div class="skeleton-text" style="width: 80%"></div>
        `;
      } else if (type === 'list') {
        skeleton = `
          <div class="skeleton-text" style="height: 40px; margin-bottom: 16px;"></div>
          <div class="skeleton-text" style="height: 40px; margin-bottom: 16px;"></div>
          <div class="skeleton-text" style="height: 40px; margin-bottom: 16px;"></div>
        `;
      }

      container.innerHTML = `<div class="skeleton-wrapper">${skeleton}</div>`;
    },

    hide(containerId, content) {
      const container = document.getElementById(containerId);
      if (!container) return;

      container.innerHTML = content;
    }
  };

  // =========================================
  // IMAGE LAZY LOADING
  // =========================================
  const LazyLoader = {
    init() {
      if ('loading' in HTMLImageElement.prototype) {
        // Native lazy loading supported
        document.querySelectorAll('img[data-src]').forEach(img => {
          img.src = img.dataset.src;
        });
      } else {
        // Fallback with Intersection Observer
        const imageObserver = new IntersectionObserver((entries) => {
          entries.forEach(entry => {
            if (entry.isIntersecting) {
              const img = entry.target;
              img.src = img.dataset.src;
              img.classList.add('loaded');
              imageObserver.unobserve(img);
            }
          });
        });

        document.querySelectorAll('img[data-src]').forEach(img => {
          imageObserver.observe(img);
        });
      }
    }
  };

  // =========================================
  // INITIALIZE ALL MODULES
  // =========================================
  function init() {
    ThemeManager.init();
    RippleEffect.init();
    ScrollReveal.init();
    Toast.init();
    LazyLoader.init();

    // Expose Toast to global scope for manual usage
    window.Toast = Toast;
    window.SkeletonLoader = SkeletonLoader;

    console.log('✨ Modern UI System initialized successfully');
  }

  // Run on DOM ready
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init);
  } else {
    init();
  }
})();
