/**
 * User Role Manager
 * Handles dynamic form field display based on selected role
 * Manages form validation and password confirmation
 * For Uni_Selector User Management System
 */

(function () {
    'use strict';

    // Role definitions
    const ROLES = {
        STUDENT: 'Student',
        UNIVERSITY_REP: 'UniversityRep',
        PLATFORM_ADMIN: 'PlatformAdmin',
        BTEC_AUTHORITY: 'BtecAuthority'
    };

    // DOM Elements Cache
    const elements = {
        roleSelect: null,
        universityRepSection: null,
        studentSection: null,
        adminSection: null,
        btecSection: null,
        universitySelect: null,
        universityRequired: null,
        password: null,
        confirmPassword: null,
        newPassword: null,
        confirmNewPassword: null,
        form: null
    };

    /**
     * Initialize the User Role Manager
     */
    function init() {
        // Cache DOM elements
        cacheElements();

        // Setup event listeners
        setupEventListeners();

        // Initial role check on page load
        handleRoleChange();

        // Setup form validation
        setupFormValidation();

        // Setup password validation
        setupPasswordValidation();
    }

    /**
     * Cache all required DOM elements
     */
    function cacheElements() {
        elements.roleSelect = document.getElementById('roleSelect');
        elements.universityRepSection = document.getElementById('universityRepSection');
        elements.studentSection = document.getElementById('studentSection');
        elements.adminSection = document.getElementById('adminSection');
        elements.btecSection = document.getElementById('btecSection');
        elements.universitySelect = document.getElementById('universitySelect');
        elements.universityRequired = document.getElementById('universityRequired');
        elements.password = document.getElementById('password');
        elements.confirmPassword = document.getElementById('confirmPassword');
        elements.newPassword = document.getElementById('newPassword');
        elements.confirmNewPassword = document.getElementById('confirmNewPassword');
        elements.form = document.querySelector('form');
    }

    /**
     * Setup all event listeners
     */
    function setupEventListeners() {
        // Role selection change
        if (elements.roleSelect) {
            elements.roleSelect.addEventListener('change', handleRoleChange);
        }

        // Password confirmation (Create form)
        if (elements.password && elements.confirmPassword) {
            elements.password.addEventListener('input', validatePasswordMatch);
            elements.confirmPassword.addEventListener('input', validatePasswordMatch);
        }

        // Password confirmation (Edit form)
        if (elements.newPassword && elements.confirmNewPassword) {
            elements.newPassword.addEventListener('input', function () {
                validateNewPasswordMatch();
                // If both fields are now empty, clear validation completely
                if (elements.newPassword.value === '' && elements.confirmNewPassword.value === '') {
                    elements.newPassword.classList.remove('is-invalid', 'is-valid');
                    elements.confirmNewPassword.classList.remove('is-invalid', 'is-valid');
                }
            });

            elements.confirmNewPassword.addEventListener('input', function () {
                validateNewPasswordMatch();
                // If both fields are now empty, clear validation completely
                if (elements.newPassword.value === '' && elements.confirmNewPassword.value === '') {
                    elements.newPassword.classList.remove('is-invalid', 'is-valid');
                    elements.confirmNewPassword.classList.remove('is-invalid', 'is-valid');
                }
            });
        }
    }

    /**
     * Handle role selection change
     * Shows/hides relevant sections based on selected role
     */
    function handleRoleChange() {
        if (!elements.roleSelect) return;

        const selectedRole = elements.roleSelect.value;

        // Hide all sections first
        hideAllSections();

        // Show relevant section based on role
        switch (selectedRole) {
            case ROLES.UNIVERSITY_REP:
                showUniversityRepSection();
                break;
            case ROLES.STUDENT:
                showStudentSection();
                break;
            case ROLES.PLATFORM_ADMIN:
                showAdminSection();
                break;
            case ROLES.BTEC_AUTHORITY:
                showBtecSection();
                break;
            default:
                // No specific section for empty selection
                break;
        }
    }

    /**
     * Hide all role-specific sections
     */
    function hideAllSections() {
        const sections = [
            elements.universityRepSection,
            elements.studentSection,
            elements.adminSection,
            elements.btecSection
        ];

        sections.forEach(section => {
            if (section) {
                section.style.display = 'none';
            }
        });

        // Remove university field requirement
        if (elements.universitySelect) {
            elements.universitySelect.removeAttribute('required');
        }
        if (elements.universityRequired) {
            elements.universityRequired.style.display = 'none';
        }
    }

    /**
     * Show University Representative section
     */
    function showUniversityRepSection() {
        if (elements.universityRepSection) {
            elements.universityRepSection.style.display = 'block';

            // Make university field required
            if (elements.universitySelect) {
                elements.universitySelect.setAttribute('required', 'required');
            }
            if (elements.universityRequired) {
                elements.universityRequired.style.display = 'inline';
            }

            // Smooth scroll to section
            setTimeout(() => {
                elements.universityRepSection.scrollIntoView({
                    behavior: 'smooth',
                    block: 'nearest'
                });
            }, 100);
        }
    }

    /**
     * Show Student section
     */
    function showStudentSection() {
        if (elements.studentSection) {
            elements.studentSection.style.display = 'block';
        }
    }

    /**
     * Show Platform Admin section
     */
    function showAdminSection() {
        if (elements.adminSection) {
            elements.adminSection.style.display = 'block';
        }
    }

    /**
     * Show BTEC Authority section
     */
    function showBtecSection() {
        if (elements.btecSection) {
            elements.btecSection.style.display = 'block';
        }
    }

    /**
     * Validate password match for Create form
     */
    function validatePasswordMatch() {
        if (!elements.password || !elements.confirmPassword) return;

        const password = elements.password.value;
        const confirmPassword = elements.confirmPassword.value;

        if (confirmPassword === '') {
            elements.confirmPassword.setCustomValidity('');
            elements.confirmPassword.classList.remove('is-invalid');
            return;
        }

        if (password !== confirmPassword) {
            elements.confirmPassword.setCustomValidity('Passwords do not match');
            elements.confirmPassword.classList.add('is-invalid');
        } else {
            elements.confirmPassword.setCustomValidity('');
            elements.confirmPassword.classList.remove('is-invalid');
            elements.confirmPassword.classList.add('is-valid');
        }
    }

    /**
     * Validate new password match for Edit form
     */
    function validateNewPasswordMatch() {
        if (!elements.newPassword || !elements.confirmNewPassword) return;

        const newPassword = elements.newPassword.value;
        const confirmNewPassword = elements.confirmNewPassword.value;

        // If both are empty, it's valid (optional password change)
        if (newPassword === '' && confirmNewPassword === '') {
            elements.confirmNewPassword.setCustomValidity('');
            elements.confirmNewPassword.classList.remove('is-invalid');
            elements.confirmNewPassword.classList.remove('is-valid');
            elements.newPassword.setCustomValidity('');
            elements.newPassword.classList.remove('is-invalid');
            elements.newPassword.classList.remove('is-valid');
            return;
        }

        // Check minimum length if password is entered
        if (newPassword !== '' && newPassword.length < 6) {
            elements.newPassword.setCustomValidity('Password must be at least 6 characters');
            elements.newPassword.classList.add('is-invalid');
            return;
        } else if (newPassword !== '') {
            elements.newPassword.setCustomValidity('');
            elements.newPassword.classList.remove('is-invalid');
            elements.newPassword.classList.add('is-valid');
        }

        // If new password is filled but confirm is empty
        if (newPassword !== '' && confirmNewPassword === '') {
            elements.confirmNewPassword.setCustomValidity('Please confirm your password');
            elements.confirmNewPassword.classList.add('is-invalid');
            return;
        }

        // Check if passwords match
        if (newPassword !== confirmNewPassword) {
            elements.confirmNewPassword.setCustomValidity('Passwords do not match');
            elements.confirmNewPassword.classList.add('is-invalid');
        } else {
            elements.confirmNewPassword.setCustomValidity('');
            elements.confirmNewPassword.classList.remove('is-invalid');
            if (confirmNewPassword !== '') {
                elements.confirmNewPassword.classList.add('is-valid');
            }
        }
    }

    /**
     * Setup Bootstrap form validation
     */
    function setupFormValidation() {
        if (!elements.form) return;

        elements.form.addEventListener('submit', function (event) {
            // For Edit form, ensure empty passwords don't block submission
            if (elements.newPassword && elements.confirmNewPassword) {
                const newPassword = elements.newPassword.value;
                const confirmNewPassword = elements.confirmNewPassword.value;

                // If both empty, clear any validation states
                if (newPassword === '' && confirmNewPassword === '') {
                    elements.newPassword.setCustomValidity('');
                    elements.confirmNewPassword.setCustomValidity('');
                    elements.newPassword.classList.remove('is-invalid');
                    elements.confirmNewPassword.classList.remove('is-invalid');
                }
            }

            // Check if University Rep role is selected but no university chosen
            if (elements.roleSelect && elements.roleSelect.value === ROLES.UNIVERSITY_REP) {
                if (elements.universitySelect && !elements.universitySelect.value) {
                    event.preventDefault();
                    event.stopPropagation();

                    // Show error message
                    showValidationError('Please select a university for the University Representative role.');

                    // Highlight the university field
                    if (elements.universitySelect) {
                        elements.universitySelect.classList.add('is-invalid');
                        elements.universitySelect.focus();
                    }

                    elements.form.classList.add('was-validated');
                    return false;
                }
            }

            // Check standard form validity
            if (!elements.form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();

                showValidationError('Please fill in all required fields correctly.');
                elements.form.classList.add('was-validated');
                return false;
            }

            elements.form.classList.add('was-validated');
        }, false);
    }

    /**
     * Show validation error with SweetAlert
     * @param {string} message - Error message to display
     */
    function showValidationError(message) {
        if (typeof Swal !== 'undefined') {
            Swal.fire({
                icon: 'error',
                title: 'Validation Error',
                text: message,
                confirmButtonColor: '#6366f1',
                confirmButtonText: 'OK'
            });
        } else {
            alert(message);
        }
    }

    /**
     * Show success message with SweetAlert
     * @param {string} message - Success message to display
     */
    function showSuccessMessage(message) {
        if (typeof Swal !== 'undefined') {
            Swal.fire({
                icon: 'success',
                title: 'Success!',
                text: message,
                showConfirmButton: false,
                timer: 3000
            });
        }
    }

    /**
     * Public API
     */
    window.UserRoleManager = {
        init: init,
        handleRoleChange: handleRoleChange,
        showSuccessMessage: showSuccessMessage,
        showValidationError: showValidationError
    };

    // Auto-initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

})();

