// Site-wide JavaScript functionality
(function() {
    'use strict';

    // DOM ready function
    function domReady(fn) {
        if (document.readyState === 'complete' || document.readyState === 'interactive') {
            setTimeout(fn, 1);
        } else {
            document.addEventListener('DOMContentLoaded', fn);
        }
    }

    // Utility functions
    const utils = {
        // Get element by selector
        $(selector) {
            return document.querySelector(selector);
        },

        // Get all elements by selector
        $$(selector) {
            return document.querySelectorAll(selector);
        },

        // Add event listener helper
        on(element, event, handler) {
            if (typeof element === 'string') {
                element = this.$(element);
            }
            if (element) {
                element.addEventListener(event, handler);
            }
        },

        // Toggle class helper
        toggleClass(element, className) {
            if (typeof element === 'string') {
                element = this.$(element);
            }
            if (element) {
                element.classList.toggle(className);
            }
        }
    };

    // Initialize site functionality
    function initSite() {
        console.log('Site initialized');

        // Example: Add click handlers to buttons
        utils.$$('button').forEach(button => {
            utils.on(button, 'click', function(e) {
                console.log('Button clicked:', this.textContent);
            });
        });

        // Example: Handle navigation menu toggle
        const navToggle = utils.$('.nav-toggle');
        const navMenu = utils.$('.nav-menu');
        
        if (navToggle && navMenu) {
            utils.on(navToggle, 'click', function() {
                utils.toggleClass(navMenu, 'active');
            });
        }

        // Example: Simple form validation
        const forms = utils.$$('form');
        forms.forEach(form => {
            utils.on(form, 'submit', function(e) {
                const requiredFields = this.querySelectorAll('[required]');
                let isValid = true;

                requiredFields.forEach(field => {
                    if (!field.value.trim()) {
                        isValid = false;
                        field.classList.add('error');
                    } else {
                        field.classList.remove('error');
                    }
                });

                if (!isValid) {
                    e.preventDefault();
                    console.log('Form validation failed');
                }
            });
        });
    }

    // Make utils available globally for testing
    window.siteUtils = utils;

    // Initialize when DOM is ready
    domReady(initSite);
})();